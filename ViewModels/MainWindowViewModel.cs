using System.Diagnostics.Metrics;
using System.Linq;
using System.Windows;

using CookMaster.Managers;
using CookMaster.MVVM;
using CookMaster.Views;
using CookMaster.Helpers;

using Microsoft.Extensions.DependencyInjection;

namespace CookMaster.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    // 1) Constants / static

    // 2) Dependencies (injected services/managers)
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    // 3) Events

    // 4) Constructors
    // Inject UserManager so MainWindowViewModel can access user state/operations
    public MainWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;

        // Initialize commands
        PerformOpenForgotPasswordWindowCommand = new RelayCommand(_ => PerformOpenForgotPasswordWindow());
        PerformOpenRegisterWindowCommand = new RelayCommand(_ => PerformOpenRegisterWindow());
        PerformSendTwoFactorCodeCommand = new RelayCommand(_ => PerformSendTwoFactorCode());
        PerformTrySignInCommand = new RelayCommand(_ => PerformTrySignIn(), _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand PerformOpenForgotPasswordWindowCommand { get; }
    public RelayCommand PerformOpenRegisterWindowCommand { get; }
    public RelayCommand PerformSendTwoFactorCodeCommand { get; }
    public RelayCommand PerformTrySignInCommand { get; }

    private void PerformOpenForgotPasswordWindow() {
        // Find the current active window (owner), regardless of type
        var current = WindowHelper.GetActiveWindow();

        // Hide the current window so it disappears behind the modal dialog
        current?.Hide();

        try {
            // Open the ForgotPasswordWindow as a modal dialog from a DI scope
            using var scope = _services.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<ForgotPasswordWindow>();

            // Set the owner to keep proper window parenting (optional, but recommended)
            if (current != null)
                window.Owner = current;

            // Show dialog; this blocks until the register window closes (either via X or Close button)
            var result = window.ShowDialog();
        }
        finally {
            // When the dialog closes, restore the same window
            if (current != null) {
                current.Show();
                current.Activate();
            }
        }
    }

    private void OpenRecipeListWindow() {
        var current = WindowHelper.GetActiveWindow();
        current?.Hide();

        try {
            using var scope = _services.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<RecipeListWindow>();

            if (current != null)
                window.Owner = current;

            var result = window.ShowDialog();

            if (result == true) {
                if (current != null) {
                    current.Show();
                    current.Activate();
                    FocusUsername = true;
                }
            }
            else {
                Application.Current.Shutdown();
            }
        }
        catch {
            if (current != null) {
                current.Show();
                current.Activate();
            }
            throw;
        }
    }

    // Register command execution (bound to Register button)
    private void PerformOpenRegisterWindow() {
        // Find the current active window (owner), regardless of type
        var current = WindowHelper.GetActiveWindow();

        // Hide the current window so it disappears behind the modal dialog
        current?.Hide();

        try {
            // Open the RegisterWindow as a modal dialog from a DI scope
            using var scope = _services.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<RegisterWindow>();

            // Set the owner to keep proper window parenting (optional, but recommended)
            if (current != null)
                window.Owner = current;

            // Show dialog; this blocks until the register window closes (either via X or Close button)
            var result = window.ShowDialog();
        }
        finally {
            // When the dialog closes, restore the same window
            if (current != null) {
                current.Show();
                current.Activate();
            }
        }
    }

    // Send two-factor code command execution (bound to Send 2FA Code button)
    private void PerformSendTwoFactorCode() {
        // Find and hide current window so the dialog appears modally on top
        var current = WindowHelper.GetActiveWindow();
        current?.Hide();

        try {
            using var scope = _services.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<CodeWindow>();

            if (current != null)
                window.Owner = current;

            // Blocks until the code window closes (Copy & Close sets DialogResult and closes)
            var result = window.ShowDialog();

            // If user clicked "Copy & Close", capture the code from the VM
            if (result == true && window.DataContext is CodeWindowViewModel vm) {
                GeneratedTwoFactorCode = vm.Code;
            }
        }
        finally {
            if (current != null) {
                current.Show();
                current.Activate();
            }
            FocusTwoFactorCode = true;
        }
    }

    // Try sign-in command execution (bound to Sign In button)
    private void PerformTrySignIn() {
        // Use bound properties from the view
        var username = Username?.Trim() ?? string.Empty;
        var password = Password ?? string.Empty;

        if (_userManager.SignIn(username, password)) {
            if (GeneratedTwoFactorCode != TwoFactorCode || GeneratedTwoFactorCode == string.Empty) {
                MessageBox.Show("Login failed. Please check your 2FA code.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                // Successful login; open recipe list window
                Username = string.Empty;
                Password = string.Empty;
                GeneratedTwoFactorCode = string.Empty;
                TwoFactorCode = string.Empty;
                OpenRecipeListWindow();
            }
        }
        else {
            // Failed login; show message box (in real app, use better UI feedback)
            MessageBox.Show("Login failed. Please check your username and password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // 6) Bindable state (editable input)
    // Password property (bound TwoWay to PasswordBox via attached behavior)
    private string _password = string.Empty;
    public string Password {
        get => _password;
        set {
            if (Set(ref _password, value)) {
                PerformTrySignInCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    // Username property (bound TwoWay to TextBox)
    private string _username = string.Empty;
    public string Username {
        get => _username;
        set {
            if (Set(ref _username, value)) {
                PerformTrySignInCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    private bool _focusTwoFactorCode = false;
    public bool FocusTwoFactorCode {
        get => _focusTwoFactorCode;
        set => Set(ref _focusTwoFactorCode, value);
    }

    // MVVM-friendly focus request property (bind TwoWay to attached behavior)
    private bool _focusUsername = true;
    public bool FocusUsername {
        get => _focusUsername;
        set => Set(ref _focusUsername, value);
    }

    // Holds the last generated two-factor code sent to the user
    private string _generatedTwoFactorCode = string.Empty;
    public string GeneratedTwoFactorCode {
        get => _generatedTwoFactorCode;
        set => Set(ref _generatedTwoFactorCode, value);
    }

    // Holds the last generated two-factor code returned by CodeWindow
    private string _twoFactorCode = string.Empty;
    public string TwoFactorCode {
        get => _twoFactorCode;
        set => Set(ref _twoFactorCode, value);
    }

    // 7) Validation and error/feedback properties

    // 8) Derived/computed properties

    // 9) Collections

    // 10) Private helpers/validation

    // 11) Nested types (none)
}
