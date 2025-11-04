using System.Collections.ObjectModel;
using System.Windows;

using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;
using CookMaster.Services;
using CookMaster.Views;

using Microsoft.Extensions.DependencyInjection;

namespace CookMaster.ViewModels;

public class UserListWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogService;

    // Close event for the view (nullable: true = success, false = explicit failure, null = just close (like X) )
    public event Action<bool?>? RequestClose;

    // Expose an ObservableCollection so the UI receives collection change notifications
    private ObservableCollection<User> _users = new();
    public ObservableCollection<User> Users {
        get => _users;
        private set => Set(ref _users, value);
    }

    public RelayCommand PerformAddUserCommand { get; }
    public RelayCommand PerformViewUserCommand { get; }
    public RelayCommand PerformDeleteUserCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    private User? _selectedUser;
    public User? SelectedUser {
        get => _selectedUser;
        set {
            if (Set(ref _selectedUser, value)) {
                // Notify that the command's ability to execute may have changed
                PerformViewUserCommand?.RaiseCanExecuteChanged();
                PerformDeleteUserCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public UserListWindowViewModel(UserManager userManager, IServiceProvider services, IDialogService dialogService) {
        _userManager = userManager;
        _services = services;
        _dialogService = dialogService;

        // Initialize ObservableCollection from manager
        RefreshUsers();

        PerformAddUserCommand = new RelayCommand(_ => PerformAddUser());
        PerformViewUserCommand = new RelayCommand(_ => PerformViewUser(), _ => SelectedUser != null);
        PerformDeleteUserCommand = new RelayCommand(_ => PerformDeleteUser(), _ => SelectedUser != null);
        PerformCloseCommand = new RelayCommand(_ => CloseWindow());
    }

    private void RefreshUsers() {
        Users = new ObservableCollection<User>(_userManager.GetAllUsers());
        Users.Remove(_userManager.GetLoggedIn()!);
    }

    public void CloseWindow() {
        // Invoke with null to indicate "just close" (same behavior as clicking the X)
        RequestClose?.Invoke(null);
    }

    private void PerformAddUser() {
        // TODO: implement user creation
    }

    private void PerformViewUser() {
        if (SelectedUser == null) return;
        // open user detail window and pass the selected user to its VM
        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<Views.UserDetailWindow>();

        // Prefer the window's DataContext if it already is the expected VM instance,
        // otherwise resolve a VM from the scope and assign it as the DataContext.
        var vm = window.DataContext as UserDetailWindowViewModel
                 ?? scope.ServiceProvider.GetRequiredService<UserDetailWindowViewModel>();

        // Provide the selected user to the detail VM
        vm.LoadUser(SelectedUser);

        // Ensure window is using the VM instance we just prepared
        window.DataContext = vm;

        window.ShowDialog();
    }

    private void PerformDeleteUser() {
        if (SelectedUser == null) return;

        // TODO: Add checks if the selected user has any recipes. If so, prevent deletion and show error.
        using var scope = _services.CreateScope();
        var recipeManager = scope.ServiceProvider.GetRequiredService<RecipeManager>();
        var userRecipes = recipeManager.GetByOwner(SelectedUser);
        if (userRecipes.Count() > 0) {
            MessageBox.Show("Cannot delete user who has recipes. Please delete their recipes first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Use dialog service instead of MessageBox
        var owner = Application.Current?.Windows.OfType<UserListWindow>().FirstOrDefault() ?? Application.Current?.MainWindow;
        var confirm = _dialogService.ShowDeleteConfirmationDialog(owner);

        // If dialog wasn't shown or closed unexpectedly, do nothing
        if (!confirm.HasValue) return;

        if (confirm.Value) {
            var username = SelectedUser.Username;
            var deleted = _userManager.DeleteUser(username);
            if (deleted) {
                // Remove from ObservableCollection so UI updates immediately
                Users.Remove(SelectedUser);
                SelectedUser = null;

                MessageBox.Show("User deleted.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else {
                MessageBox.Show("Failed to delete user.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}