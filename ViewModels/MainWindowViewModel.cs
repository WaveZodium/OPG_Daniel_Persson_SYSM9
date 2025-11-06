using System.Diagnostics.Metrics;
using System.Linq;
using System.Windows;

using CookMaster.Managers;
using CookMaster.MVVM;
using CookMaster.Views;

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

        // canExecute returns true only when both fields are non-empty
        TrySignInCommand = new RelayCommand(_ => TrySignIn(), _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));
        OpenRecipeListWindowCommand = new RelayCommand(_ => OpenRecipeListWindow());
        OpenRegisterWindowCommand = new RelayCommand(_ => OpenRegisterWindow());
        ForgotPasswordCommand = new RelayCommand(_ => ForgotPassword());
        SendTwoFactorCodeCommand = new RelayCommand(_ => SendTwoFactorCode());

        // Request initial focus on the username box (bound to the attached behavior)
        FocusUsername = true;
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand TrySignInCommand { get; }
    public RelayCommand OpenRecipeListWindowCommand { get; }
    public RelayCommand OpenRegisterWindowCommand { get; }
    public RelayCommand ForgotPasswordCommand { get; }
    public RelayCommand SendTwoFactorCodeCommand { get; }

    private void TrySignIn() {
        // Use bound properties from the view
        var username = Username?.Trim() ?? string.Empty;
        var password = Password ?? string.Empty;

        if (_userManager.SignIn(username, password)) {

            if (GeneratedTwoFactorCode != TwoFactorCode) {
                MessageBox.Show("Login failed. Please check your 2FA code.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                // Successful login; open recipe list window
                Username = string.Empty;
                Password = string.Empty;

                OpenRecipeListWindow();
            }
        }
        else {
            // Failed login; show message box (in real app, use better UI feedback)
            MessageBox.Show("Login failed. Please check your username and password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenRecipeListWindow() {
        var current = GetActiveWindow();
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

    private void OpenRegisterWindow() {
        // Find the current active window (owner), regardless of type
        var current = GetActiveWindow();

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
            window.ShowDialog();
        }
        finally {
            // When the dialog closes, restore the same window
            if (current != null) {
                current.Show();
                current.Activate();
            }
        }
    }

    private void ForgotPassword() {
        // Find the current active window (owner), regardless of type
        var current = GetActiveWindow();

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
            window.ShowDialog();
        }
        finally {
            // When the dialog closes, restore the same window
            if (current != null) {
                current.Show();
                current.Activate();
            }
        }
    }

    private void SendTwoFactorCode() {
        // Find and hide current window so the dialog appears modally on top
        var current = GetActiveWindow();
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
        }
    }

    // 6) Bindable state (editable input)
    private string _username = string.Empty;
    public string Username {
        get => _username;
        set {
            if (Set(ref _username, value)) {
                TrySignInCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    private string _password = string.Empty;
    // NOTE: TextBox binding is simple for the assignment. For production use PasswordBox + secure handling.
    public string Password {
        get => _password;
        set {
            if (Set(ref _password, value)) {
                TrySignInCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    // New MVVM-friendly focus request property (bind TwoWay to attached behavior)
    private bool _focusUsername = true;
    public bool FocusUsername {
        get => _focusUsername;
        set => Set(ref _focusUsername, value);
    }

    // Holds the last generated two-factor code returned by CodeWindow
    private string? _twoFactorCode;
    public string? TwoFactorCode {
        get => _twoFactorCode;
        set => Set(ref _twoFactorCode, value);
    }

    private string? _generatedTwoFactorCode;
    public string? GeneratedTwoFactorCode {
        get => _generatedTwoFactorCode;
        set => Set(ref _generatedTwoFactorCode, value);
    }

    // 7) Validation and error/feedback properties

    // 8) Derived/computed properties

    // 9) Collections

    // 10) Private helpers/validation
    private static Window? GetActiveWindow() {
        var app = Application.Current;
        if (app == null) return null;

        // Prefer the active focused window; fall back to any visible/enabled; then MainWindow
        return app.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            ?? app.Windows.OfType<Window>().FirstOrDefault(w => w.IsVisible && w.IsEnabled)
            ?? app.MainWindow;
    }

    // 11) Nested types (none)
}
