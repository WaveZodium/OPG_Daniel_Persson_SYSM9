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

        // Request initial focus on the username box (bound to the attached behavior)
        FocusUsername = true;
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand TrySignInCommand { get; }
    public RelayCommand OpenRecipeListWindowCommand { get; }
    public RelayCommand OpenRegisterWindowCommand { get; }
    public RelayCommand ForgotPasswordCommand { get; }

    private void TrySignIn() {
        // Use bound properties from the view
        var username = Username?.Trim() ?? string.Empty;
        var password = Password ?? string.Empty;

        if (_userManager.SignIn(username, password)) {
            // Successful login; open recipe list window
            OpenRecipeListWindow();
        }
        else {
            // Failed login; show message box (in real app, use better UI feedback)
            MessageBox.Show("Login failed. Please check your username and password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenRecipeListWindow() {
        // Create a scope so the new window and its dependencies get disposed when closed
        var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<RecipeListWindow>();

        // Dispose the scope when the window closes (non-modal)
        window.Closed += (_, __) => scope.Dispose();

        // Show the recipe list and close the main window (login flow)
        window.Show();

        // Close/hide MainWindow (the VM uses Application.Current to find it)
        var main = Application.Current?.Windows.OfType<MainWindow>().FirstOrDefault();
        main?.Close();
    }

    private void OpenRegisterWindow() {
        // Use a scope and open RegisterWindow as a modal dialog; dispose immediately after
        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<RegisterWindow>();

        // ShowDialog blocks until closed; scope is disposed when leaving using block
        window.ShowDialog();
    }

    private void ForgotPassword() {
        // Use a scope and open ForgotPasswordWindow as a modal dialog; dispose immediately after
        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<ForgotPasswordWindow>();

        // ShowDialog blocks until closed; scope is disposed when leaving using block
        var result = window.ShowDialog();

        if (result == true) {
            MessageBox.Show("Password updated successfully. You can now log in with your new password.", "Password Updated", MessageBoxButton.OK, MessageBoxImage.Information);
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

    // 7) Validation and error/feedback properties

    // 8) Derived/computed properties

    // 9) Collections

    // 10) Private helpers/validation

    // 11) Nested types (none)
}
