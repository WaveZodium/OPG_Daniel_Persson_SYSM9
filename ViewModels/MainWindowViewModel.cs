using CookMaster.Managers;
using CookMaster.MVVM;
using CookMaster.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CookMaster.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    private string _username = string.Empty;
    public string Username {
        get => _username;
        set {
            if (Set(ref _username, value)) {
                TryLoginCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    private string _password = string.Empty;
    // NOTE: TextBox binding is simple for the assignment. For production use PasswordBox + secure handling.
    public string Password {
        get => _password;
        set {
            if (Set(ref _password, value)) {
                TryLoginCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand TryLoginCommand { get; }
    public RelayCommand OpenRecipeListWindowCommand { get; }
    public RelayCommand OpenRegisterWindowCommand { get; }
    public RelayCommand ForgotPasswordCommand { get; }

    // Inject UserManager so MainWindowViewModel can access user state/operations
    public MainWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;

        // canExecute returns true only when both fields are non-empty
        TryLoginCommand = new RelayCommand(_ => TryLogin(), _ => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password));
        OpenRecipeListWindowCommand = new RelayCommand(_ => OpenRecipeListWindow());
        OpenRegisterWindowCommand = new RelayCommand(_ => OpenRegisterWindow());
        ForgotPasswordCommand = new RelayCommand(_ => ForgotPassword());
    }

    private void TryLogin() {
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
        // Caught by the ViewModel when the Hyperlink is clicked.
        // Replace this with your reset flow (open a window, send email, navigate, etc.)
        MessageBox.Show("Forgot password clicked. Implement the password recovery flow in ForgotPassword().", "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
