using System.Diagnostics.Metrics;
using System.Linq;
using System.Windows;

using CookMaster.Helpers;
using CookMaster.Managers;
using CookMaster.MVVM;
using CookMaster.Views;

using Microsoft.Extensions.DependencyInjection;

using static System.Formats.Asn1.AsnWriter;

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
        // Find and hide current window so the new window appears on top
        var current = WindowHelper.GetActiveWindow();

        // Hide the current window
        current?.Hide();

        IServiceScope? scope = null;
        try {
            // Create a DI scope to resolve the window that should be opened.
            scope = _services.CreateScope();

            // Resolve the ForgotPasswordWindow from the DI scope
            var window = scope.ServiceProvider.GetRequiredService<ForgotPasswordWindow>();

            // Set the owner to keep proper window parenting (optional, but recommended).
            // It is needed for WindowStartupLocation.CenterOwner to work correctly.
            if (current != null)
                window.Owner = current;

            // Subscribe to the Closed event to know when it closes.
            window.Closed += (_, _) => {
                // When the forgot password window closes, show the main window again
                if (current != null) {
                    current.Show();
                    current.Activate();
                    FocusUsername = true;

                    // Dispose the scope after the window is done
                    scope?.Dispose();
                }
            };

            // Show the forgot password window (non-modal)
            window.Show();
        }
        // Handle any exceptions that may occur during window creation or showing
        catch (Exception ex) {
            MessageBox.Show($"An error occurred while opening the forgot password window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (current != null) {
                // Restore the main window if there was an error and ensure it is active
                current.Show();
                current.Activate();
            }
            scope?.Dispose();
        }
    }

    private void OpenRecipeListWindow() {
        // Find and hide current window so the new window appears on top
        var current = WindowHelper.GetActiveWindow();

        // Hide the current window
        current?.Hide();

        IServiceScope? scope = null;
        try {
            // Create a DI scope to resolve the window that should be opened.
            scope = _services.CreateScope();

            // Resolve the RecipeListWindow from the DI scope
            var window = scope.ServiceProvider.GetRequiredService<RecipeListWindow>();

            // Set the owner to keep proper window parenting (optional, but recommended).
            // It is needed for WindowStartupLocation.CenterOwner to work correctly.
            if (current != null)
                window.Owner = current;

            // Subscribe to the Closed event to know when it closes.
            window.Closed += (_, _) => {
                try {
                    var vm = window.DataContext as RecipeListWindowViewModel;
                    if (vm?.IsLogout == true) {
                        // User chose to log out -> return to login
                        if (current != null) {
                            current.Show();
                            current.Activate();
                            FocusUsername = true;
                        }
                    }
                    else {
                        // User closed with X (or any non-logout path) -> exit app
                        Application.Current.Shutdown();
                    }
                }
                finally {
                    scope?.Dispose();
                }
            };

            // Show the recipe list window (non-modal)
            window.Show();
        }
        // Handle any exceptions that may occur during window creation or showing
        catch (Exception ex) {
            MessageBox.Show($"An error occurred while opening the recipe list window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (current != null) {
                // Restore the main window if there was an error and ensure it is active
                current.Show();
                current.Activate();
            }
            scope?.Dispose();
        }
    }

    // Register command execution (bound to Register button)
    private void PerformOpenRegisterWindow() {
        // Find and hide current window so the new window appears on top
        var current = WindowHelper.GetActiveWindow();

        // Hide the current window
        current?.Hide();

        IServiceScope? scope = null;
        try {
            // Create a DI scope to resolve the window that should be opened.
            scope = _services.CreateScope();

            // Resolve the RegisterWindow from the DI scope
            var window = scope.ServiceProvider.GetRequiredService<RegisterWindow>();

            // Set the owner to keep proper window parenting (optional, but recommended).
            // It is needed for WindowStartupLocation.CenterOwner to work correctly.
            if (current != null)
                window.Owner = current;

            // Subscribe to the Closed event to know when it closes.
            window.Closed += (_, _) => {
                // When the register window closes, show the main window again
                if (current != null) {
                    current.Show();
                    current.Activate();
                    FocusUsername = true;

                    // Dispose the scope after the window is done
                    scope?.Dispose();
                }
            };

            // Show the register window (non-modal)
            window.Show();
        }
        // Handle any exceptions that may occur during window creation or showing
        catch (Exception ex) {
            MessageBox.Show($"An error occurred while opening the register window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (current != null) {
                // Restore the main window if there was an error and ensure it is active
                current.Show();
                current.Activate();
            }
            scope?.Dispose();
        }
    }

    // Send two-factor code command execution (bound to Send 2FA Code button)
    private void PerformSendTwoFactorCode() {
        // Find and hide current window so the new window appears on top
        var current = WindowHelper.GetActiveWindow();

        // Hide the current window
        current?.Hide();

        IServiceScope? scope = null;
        try {
            // Create a DI scope to resolve the window that should be opened.
            scope = _services.CreateScope();

            // Resolve the CodeWindow from the DI scope
            var window = scope.ServiceProvider.GetRequiredService<CodeWindow>();

            // Set the owner to keep proper window parenting (optional, but recommended).
            // It is needed for WindowStartupLocation.CenterOwner to work correctly.
            if (current != null)
                window.Owner = current;

            // Subscribe to the Closed event to know when it closes.
            window.Closed += (_, _) => {
                try {
                    // Read the code from the window's ViewModel when it closes
                    if (window.DataContext is CodeWindowViewModel vm && !string.IsNullOrWhiteSpace(vm.Code)) {
                        GeneratedTwoFactorCode = vm.Code;
                    }
                }
                finally {
                    // When the 2FA code window closes, show the main window again
                    if (current != null) {
                        current.Show();
                        current.Activate();
                    }
                    // Put caret where the user should type the code
                    FocusTwoFactorCode = true;

                    // Dispose the scope after the window is done
                    scope?.Dispose();
                }
            };

            // Show the 2FA code window (non-modal)
            window.Show();
        }
        // Handle any exceptions that may occur during window creation or showing
        catch (Exception ex) {
            MessageBox.Show($"An error occurred while opening the 2FA code window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (current != null) {
                // Restore the main window if there was an error and ensure it is active
                current.Show();
                current.Activate();
            }
            scope?.Dispose();
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
