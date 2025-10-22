using CookMaster.Managers;
using CookMaster.MVVM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace CookMaster.ViewModels;

public class UserListWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    // Close event for the view (nullable: true = success, false = explicit failure, null = just close (like X) )
    public event Action<bool?>? RequestClose;

    public RelayCommand PerformAddUserCommand { get; }
    public RelayCommand PerformViewUserCommand { get; }
    public RelayCommand PerformDeleteUserCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    public UserListWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;

        PerformAddUserCommand = new RelayCommand(_ => PerformAddUser());
        PerformViewUserCommand = new RelayCommand(_ => PerformViewUser());
        PerformDeleteUserCommand = new RelayCommand(_ => PerformDeleteUser());
        PerformCloseCommand = new RelayCommand(_ => CloseWindow());
    }

    public void CloseWindow() {
        // Invoke with null to indicate "just close" (same behavior as clicking the X)
        RequestClose?.Invoke(null);
    }

    private void PerformAddUser() {
        // TODO: implement user creation
    }

    private void PerformViewUser() {
        // open user detail window
        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<Views.UserDetailWindow>();
        window.ShowDialog();
    }

    private void PerformDeleteUser() {
        var result = MessageBox.Show("Are you sure you want to delete the selected user?",
                    "Delete User",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

        //handle messagebox answer
        if (result == MessageBoxResult.Yes) {
            MessageBox.Show("User deleted.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}