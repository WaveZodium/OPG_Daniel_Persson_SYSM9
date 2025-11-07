using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Linq; // <-- add

using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;
using CookMaster.Services;

namespace CookMaster.ViewModels;

public class RegisterWindowViewModel : ViewModelBase {
    // 1) Constants / static
    private const int MinUsernameLength = 3; // minimum username length policy

    // Simple, pragmatic email regex:
    // - Local part: common characters
    // - Domain: one or more labels separated by dots, labels can't be empty
    // - TLD: letters only, at least 2 chars (rejects trailing dot and one-letter TLDs)
    private static readonly Regex EmailRegex = new(
        @"^[A-Za-z0-9._%+\-']+@(?:[A-Za-z0-9\-]+\.)+[A-Za-z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    // 2) Dependencies (injected services/managers)
    private readonly UserManager _userManager;
    private readonly IDialogService _dialogService;

    // 3) Events
    // Event used to ask the view to close. Parameter indicates success (true) or cancel/fail (false).
    public event Action<bool>? RequestClose;

    // 4) Constructors
    public RegisterWindowViewModel(UserManager userManager, IDialogService dialogService) {
        _userManager = userManager;
        _dialogService = dialogService;

        PerformRegisterCommand = new RelayCommand(_ => PerformRegister(), CanRegister);
        PerformCancelCommand = new RelayCommand(_ => PerformCancel());
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand PerformRegisterCommand { get; }
    public RelayCommand PerformCancelCommand { get; }

    private bool CanRegister(object? _) =>
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        PasswordsMatch &&
        MeetsPasswordPolicy(Password) && // <-- enforce explicit requirement
        !string.IsNullOrWhiteSpace(Email) &&
        string.IsNullOrEmpty(EmailError) &&
        string.IsNullOrEmpty(UsernameError) &&
        !string.IsNullOrWhiteSpace(SecurityQuestion) &&
        !string.IsNullOrWhiteSpace(SecurityAnswer);

    private void PerformRegister() {
        MessageBox.Show("Performing registration...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);

        Username = Username.Trim();
        Email = Email.Trim();
        SecurityQuestion = SecurityQuestion.Trim();
        SecurityAnswer = SecurityAnswer.Trim();

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

    // 6) Bindable state (editable input)
    private string _username = string.Empty;
    public string Username {
        get { return _username; }
        set {
            _username = value;
            ValidateUsername(value);
            PerformRegisterCommand.RaiseCanExecuteChanged();
        }
    }

    private string _password = string.Empty;
    public string Password {
        get { return _password; }
        set {
            if (Set(ref _password, value)) {
                ValidatePasswordStrength(value);
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

    private string _email = string.Empty;
    public string Email {
        get { return _email; }
        set {
            if (Set(ref _email, value)) {
                ValidateEmailFormat(value);
                PerformRegisterCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private Country _selectedCountry;
    public Country SelectedCountry {
        get { return _selectedCountry; }
        set { _selectedCountry = value; PerformRegisterCommand.RaiseCanExecuteChanged(); }
    }

    private string _securityQuestion = string.Empty;
    public string SecurityQuestion {
        get { return _securityQuestion; }
        set { _securityQuestion = value; PerformRegisterCommand.RaiseCanExecuteChanged(); }
    }

    private string _securityAnswer = string.Empty;
    public string SecurityAnswer {
        get { return _securityAnswer; }
        set { _securityAnswer = value; PerformRegisterCommand.RaiseCanExecuteChanged(); }
    }

    // 7) Validation and error/feedback properties
    private string _usernameError = string.Empty;
    public string UsernameError {
        get => _usernameError;
        set => Set(ref _usernameError, value);
    }

    private string _emailError = string.Empty;
    public string EmailError {
        get => _emailError;
        set => Set(ref _emailError, value);
    }

    // Password strength bindables used by the XAML meter
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

    private string _passwordStrengthText = string.Empty;
    public string PasswordStrengthText {
        get => _passwordStrengthText;
        private set => Set(ref _passwordStrengthText, value);
    }

    private Brush _passwordStrengthBrush = Brushes.Gray;
    public Brush PasswordStrengthBrush {
        get => _passwordStrengthBrush;
        private set => Set(ref _passwordStrengthBrush, value);
    }

    // 8) Derived/computed properties
    // 4-segment bar flags
    public bool PasswordStrengthBar1 => PasswordStrengthScore >= 1;
    public bool PasswordStrengthBar2 => PasswordStrengthScore >= 2;
    public bool PasswordStrengthBar3 => PasswordStrengthScore >= 3;
    public bool PasswordStrengthBar4 => PasswordStrengthScore >= 4;

    // Password match bindables (computed)
    public bool PasswordsMatch =>
        !string.IsNullOrEmpty(ConfirmPassword) &&
        string.Equals(Password ?? string.Empty, ConfirmPassword ?? string.Empty, StringComparison.Ordinal);

    public string PasswordMatchMessage =>
        PasswordsMatch ? "Passwords match" : "Passwords do not match";

    public Brush PasswordMatchBrush =>
        PasswordsMatch ? Brushes.ForestGreen : Brushes.IndianRed;

    // Show the message only after user starts typing in Confirm Password
    public bool ShowPasswordMatchMessage => !string.IsNullOrEmpty(ConfirmPassword);

    // 9) Collections
    public IEnumerable<Country> Countries { get; } =
        Enum.GetValues(typeof(Country)).Cast<Country>();

    // 10) Private helpers/validation
    private void UpdatePasswordMatch() {
        OnPropertyChanged(nameof(PasswordsMatch));
        OnPropertyChanged(nameof(PasswordMatchMessage));
        OnPropertyChanged(nameof(PasswordMatchBrush));
        OnPropertyChanged(nameof(ShowPasswordMatchMessage));
    }

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
        var result = PasswordStrengthService.Evaluate(value ?? string.Empty);

        PasswordStrengthScore = result.Score;
        PasswordStrengthText = result.Label;
        PasswordStrengthBrush = result.Score switch {
            0 or 1 => Brushes.IndianRed,
            2 => Brushes.Orange,
            3 => Brushes.YellowGreen,
            4 => Brushes.ForestGreen,
            _ => Brushes.Gray
        };
    }

    // Explicit VG policy: >=8 chars, at least one digit and one special character
    private static bool MeetsPasswordPolicy(string? pwd) {
        var p = pwd ?? string.Empty;
        return p.Length >= 8
            && p.Any(char.IsDigit)
            && p.Any(ch => !char.IsLetterOrDigit(ch));
    }

    private void ValidateUsername(string? value) {
        var current = (value ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(current)) {
            UsernameError = "Username is required.";
            return;
        }

        if (current.Length < MinUsernameLength) {
            UsernameError = $"Username must be at least {MinUsernameLength} characters.";
            return;
        }

        if (_userManager.UserExists(current)) {
            UsernameError = "Username already exists. Please choose another.";
            return;
        }

        UsernameError = string.Empty;
    }

    // 11) Nested types (none)
}