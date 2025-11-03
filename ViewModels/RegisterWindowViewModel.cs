using System;
using System.Windows;
using CookMaster.MVVM;
using CookMaster.Managers;
using CookMaster.Models;
using Microsoft.Extensions.DependencyInjection;
using CookMaster.Services;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace CookMaster.ViewModels;

public class RegisterWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogService;

    // Event used to ask the view to close. Parameter indicates success (true) or cancel/fail (false).
    public event Action<bool>? RequestClose;

    public RelayCommand PerformRegisterCommand { get; }
    public RelayCommand PerformCancelCommand { get; }

    // (add any bindable properties like Username/Password/ConfirmPassword/SelectedCountry here)
    private string _username;
    public string Username {
        get { return _username; }
        set {
            if (!string.IsNullOrWhiteSpace(value) && _userManager.UserExists(value)) {
                _usernameError = "Username already exists. Please choose another.";
            }
            else { _usernameError = string.Empty; }

            OnPropertyChanged(nameof(UsernameError));
            _username = value;
            PerformRegisterCommand.RaiseCanExecuteChanged();
        }
    }

    private string _usernameError;
    public string UsernameError {
        get => _usernameError;
        set => Set(ref _usernameError, value);
    }

    private string _password = string.Empty;
    public string Password {
        get { return _password; }
        set {
            if (Set(ref _password, value)) {
                ValidatePasswordStrength(value);   // Re-enabled strength evaluation
                UpdatePasswordMatch();
                PerformRegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword {
        get { return _confirmPassword; }
        set {
            if (Set(ref _confirmPassword, value)) {
                UpdatePasswordMatch();
                PerformRegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string _email;
    public string Email {
        get { return _email; }
        set {
            if (Set(ref _email, value)) {
                ValidateEmailFormat(value);
                PerformRegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string _emailError = string.Empty;
    public string EmailError {
        get => _emailError;
        set => Set(ref _emailError, value);
    }

    // ===== Password strength bindables used by the XAML meter =====
    private int _passwordStrengthScore; // 0..4
    public int PasswordStrengthScore {
        get => _passwordStrengthScore;
        private set {
            if (Set(ref _passwordStrengthScore, value)) {
                OnPropertyChanged(nameof(PasswordStrengthBar1));
                OnPropertyChanged(nameof(PasswordStrengthBar2));
                OnPropertyChanged(nameof(PasswordStrengthBar3));
                OnPropertyChanged(nameof(PasswordStrengthBar4));
            }
        }
    }

    private string _passwordStrengthText;
    public string PasswordStrengthText {
        get => _passwordStrengthText;
        private set => Set(ref _passwordStrengthText, value);
    }

    private Brush _passwordStrengthBrush = Brushes.Gray;
    public Brush PasswordStrengthBrush {
        get => _passwordStrengthBrush;
        private set => Set(ref _passwordStrengthBrush, value);
    }

    // 4-segment bar flags
    public bool PasswordStrengthBar1 => PasswordStrengthScore >= 1;
    public bool PasswordStrengthBar2 => PasswordStrengthScore >= 2;
    public bool PasswordStrengthBar3 => PasswordStrengthScore >= 3;
    public bool PasswordStrengthBar4 => PasswordStrengthScore >= 4;

    // ===== Password match bindables (computed) =====
    public bool PasswordsMatch =>
        !string.IsNullOrEmpty(ConfirmPassword) &&
        string.Equals(Password ?? string.Empty, ConfirmPassword ?? string.Empty, StringComparison.Ordinal);

    public string PasswordMatchMessage =>
        PasswordsMatch ? "Passwords match" : "Passwords do not match";

    public Brush PasswordMatchBrush =>
        PasswordsMatch ? Brushes.ForestGreen : Brushes.IndianRed;

    // Show the message only after user starts typing in Confirm Password
    public bool ShowPasswordMatchMessage => !string.IsNullOrEmpty(ConfirmPassword);

    private void UpdatePasswordMatch() {
        OnPropertyChanged(nameof(PasswordsMatch));
        OnPropertyChanged(nameof(PasswordMatchMessage));
        OnPropertyChanged(nameof(PasswordMatchBrush));
        OnPropertyChanged(nameof(ShowPasswordMatchMessage));
    }

    // Simple, pragmatic email regex:
    // - Local part: common characters
    // - Domain: one or more labels separated by dots, labels can't be empty
    // - TLD: letters only, at least 2 chars (rejects trailing dot and one-letter TLDs)
    private static readonly Regex EmailRegex = new(
        @"^[A-Za-z0-9._%+\-']+@(?:[A-Za-z0-9\-]+\.)+[A-Za-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private void ValidateEmailFormat(string value) {
        var email = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email)) {
            EmailError = "Invalid email format. Example: user@example.com";
        }
        else {
            EmailError = string.Empty;
        }
    }

    // Password strength: 0..4 (Weak -> Very Strong)
    // +1 length >= 8
    // +1 mixed case
    // +1 digit
    // +1 symbol
    private void ValidatePasswordStrength(string? value) {
        var pwd = value ?? string.Empty;

        bool hasLower = pwd.Any(char.IsLower);
        bool hasUpper = pwd.Any(char.IsUpper);
        bool hasDigit = pwd.Any(char.IsDigit);
        bool hasSymbol = pwd.Any(ch => !char.IsLetterOrDigit(ch));

        int score = 0;
        if (pwd.Length >= 8) score++;
        if (hasLower && hasUpper) score++;
        if (hasDigit) score++;
        if (hasSymbol) score++;

        score = Math.Clamp(score, 0, 4);
        PasswordStrengthScore = score;

        switch (score) {
            case 0:
            case 1:
                PasswordStrengthText = pwd.Length > 0 && pwd.Length < 8 ? "Too short" : "Weak";
                PasswordStrengthBrush = Brushes.IndianRed;
                break;
            case 2:
                PasswordStrengthText = "Fair";
                PasswordStrengthBrush = Brushes.Orange;
                break;
            case 3:
                PasswordStrengthText = "Strong";
                PasswordStrengthBrush = Brushes.YellowGreen;
                break;
            case 4:
                PasswordStrengthText = "Very strong";
                PasswordStrengthBrush = Brushes.ForestGreen;
                break;
        }
    }

    private Window? FindOwnerWindow() =>
        Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => ReferenceEquals(w.DataContext, this));

    private Country _selectedCountry;
    public Country SelectedCountry {
        get { return _selectedCountry; }
        set { _selectedCountry = value; PerformRegisterCommand.RaiseCanExecuteChanged(); }
    }

    public IEnumerable<Country> Countries { get; } =
        Enum.GetValues(typeof(Country)).Cast<Country>();

    public string _securityQuestion;
    public string SecurityQuestion {
        get { return _securityQuestion; }
        set { _securityQuestion = value; PerformRegisterCommand.RaiseCanExecuteChanged(); }
    }

    public string _securityAnswer;
    public string SecurityAnswer {
        get { return _securityAnswer; }
        set { _securityAnswer = value; PerformRegisterCommand.RaiseCanExecuteChanged(); }
    }

    public RegisterWindowViewModel(UserManager userManager, IServiceProvider services, IDialogService dialogService) {
        _userManager = userManager;
        _services = services;
        _dialogService = dialogService;

        PerformRegisterCommand = new RelayCommand(_ => PerformRegister(), CanRegister);
        PerformCancelCommand = new RelayCommand(_ => PerformCancel());
    }

    private bool CanRegister(object? _) =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        PasswordsMatch &&
        !string.IsNullOrWhiteSpace(Email) &&
        string.IsNullOrEmpty(EmailError) &&
        string.IsNullOrEmpty(UsernameError) &&
        !string.IsNullOrWhiteSpace(SecurityQuestion) &&
        !string.IsNullOrWhiteSpace(SecurityAnswer);

    private void PerformRegister() {
        MessageBox.Show("Performing registration...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

        Username = Username?.Trim();
        Email = Email?.Trim();
        SecurityQuestion = SecurityQuestion?.Trim();
        SecurityAnswer = SecurityAnswer?.Trim();
        var newUser = new User(
            Username,
            Password,
            UserRole.User,
            SelectedCountry,
            Email,
            SecurityQuestion,
            SecurityAnswer
        );

        _userManager.CreateUser(newUser);

        // For now assume registration succeeded and request close:
        RequestClose?.Invoke(true);
    }
    private void PerformCancel() {
        // Request the view to close and indicate cancellation
        RequestClose?.Invoke(false);
    }
}