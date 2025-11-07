using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class UserDetailWindowViewModel : ViewModelBase {
    // 2) Dependencies
    private readonly UserManager _userManager;

    // 3) Events
    public event Action<bool>? RequestClose;

    // 4) Constructor
    public UserDetailWindowViewModel(UserManager userManager) {
        _userManager = userManager;

        PerformSaveCommand = new RelayCommand(_ => PerformSave(), _ => CanSave());
        PerformCloseCommand = new RelayCommand(_ => PerformClose());

        UsernameError = string.Empty;
    }

    // 5) Commands
    public RelayCommand PerformSaveCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    private bool CanSave() =>
        User != null &&
        string.IsNullOrWhiteSpace(UsernameError) &&
        !string.IsNullOrWhiteSpace(User.Username);

    private void PerformSave() {
        // Final guard
        ValidateUsername();
        if (!string.IsNullOrEmpty(UsernameError)) return;

        if (_originalUser != null && User != null) {
            _originalUser.Username = User.Username;
            if (IsAdmin) {
                _originalUser.Role = User.Role;
            }
            _originalUser.Country = User.Country;
            _originalUser.Email = User.Email;
            _originalUser.SecurityQuestion = User.SecurityQuestion;
            _originalUser.SecurityAnswer = User.SecurityAnswer;
            _originalUser.SetPassword(User.Password);
        }
        RequestClose?.Invoke(true);
    }

    private void PerformClose() => RequestClose?.Invoke(false);

    // 6) Bindable state
    // Original reference (for saving back)
    private User? _originalUser;
    private string _originalUsername = string.Empty;

    private User? _user;
    public User? User {
        get => _user;
        set {
            if (Set(ref _user, value)) {
                // notify proxy properties
                OnPropertyChanged(nameof(Username));
                PerformSaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    // Proxy for username editing/validation
    public string Username {
        get => User?.Username ?? string.Empty;
        set {
            if (User == null) return;
            if (value == User.Username) return;
            User.Username = value;
            OnPropertyChanged(); // Username
            ValidateUsername();
        }
    }

    // 7) Validation state
    // Policy
    private const int MinUsernameLength = 3;

    private string _usernameError = string.Empty;
    public string UsernameError {
        get => _usernameError;
        set {
            if (Set(ref _usernameError, value)) {
                PerformSaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    // 8) Derived/computed
    public bool IsAdmin => _userManager.IsAdmin;

    // 9) Collections
    public IEnumerable<Country> Countries { get; } =
        Enum.GetValues(typeof(Country)).Cast<Country>();

    public IEnumerable<UserRole> Roles { get; } =
        Enum.GetValues(typeof(UserRole)).Cast<UserRole>();

    // 10) Helpers
    public void LoadUser(User user) {
        _originalUser = user;
        _originalUsername = user.Username;
        User = _userManager.CopyUser(user);
        UsernameError = string.Empty; // no check on initial load
        ValidateUsername(); // enforce new policy on load
    }

    private void ValidateUsername() {
        if (User == null) {
            UsernameError = "No user loaded.";
            return;
        }

        var current = User.Username.Trim();

        if (string.IsNullOrWhiteSpace(current)) {
            UsernameError = "Username is required.";
            return;
        }

        if (current.Length < MinUsernameLength) {
            UsernameError = $"Username must be at least {MinUsernameLength} characters.";
            return;
        }

        // unchanged from original and meets length requirement
        if (string.Equals(current, _originalUsername, StringComparison.Ordinal)) {
            UsernameError = string.Empty;
            return;
        }

        UsernameError = _userManager.UserExists(current)
            ? "Username already exists. Please choose another."
            : string.Empty;
    }
}
