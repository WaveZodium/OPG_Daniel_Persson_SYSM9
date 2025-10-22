using CookMaster.Managers;
using CookMaster.MVVM;
using CookMaster.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CookMaster.ViewModels;

public class UserListWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    public RelayCommand PerformAddUserCommand { get; }
    public RelayCommand PerformViewUserCommand { get; }
    public RelayCommand PerformDeleteUserCommand { get; }

    public UserListWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;

        PerformAddUserCommand = new RelayCommand(_ => PerformAddUser());
        PerformViewUserCommand = new RelayCommand(_ => PerformViewUser());
        PerformDeleteUserCommand = new RelayCommand(_ => PerformDeleteUser());
    }

    private void PerformAddUser() {

    }

    private void PerformViewUser() {
        // open user detail window
        // Use a scope and open UserDetailWindow as a modal dialog; dispose immediately after
        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<UserDetailWindow>();

        // ShowDialog blocks until closed; scope is disposed when leaving using block
        window.ShowDialog();
    }

    private void PerformDeleteUser() {

    }
}