using System;
using System.Windows;
using CookMaster.MVVM;
using CookMaster.Managers;
using CookMaster.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CookMaster.ViewModels;

public class RegisterWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    // Event used to ask the view to close. Parameter indicates success (true) or cancel/fail (false).
    public event Action<bool>? RequestClose;

    public RelayCommand PerformRegisterCommand { get; }
    public RelayCommand PerformCancelCommand { get; }

    // (add any bindable properties like Username/Password/ConfirmPassword/SelectedCountry here)

    public RegisterWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;

        PerformRegisterCommand = new RelayCommand(_ => PerformRegister());
        PerformCancelCommand = new RelayCommand(_ => PerformCancel());
    }

    private void PerformRegister() {
        MessageBox.Show("Performing registration...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

        // For now assume registration succeeded and request close:
        RequestClose?.Invoke(true);
    }
    private void PerformCancel() {
        // Request the view to close and indicate cancellation
        RequestClose?.Invoke(false);
    }
}