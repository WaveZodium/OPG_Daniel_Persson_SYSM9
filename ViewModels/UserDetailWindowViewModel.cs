using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class UserDetailWindowViewModel : ViewModelBase {
    // 1) Constants / static

    // 2) Dependencies (injected services/managers)
    private readonly UserManager _userManager;

    // Keep original reference so we can copy edits back on Save
    private User? _originalUser;

    // 3) Events
    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    // 4) Constructors
    public UserDetailWindowViewModel(UserManager userManager) {
        _userManager = userManager;

        PerformSaveCommand = new RelayCommand(_ => PerformSave());
        PerformCloseCommand = new RelayCommand(_ => PerformClose());
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand PerformSaveCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    private void PerformSave() {
        // Copy edited values back to the original
        if (_originalUser != null && User != null) {
            _originalUser.Username = User.Username;
            _originalUser.Role = User.Role;
            _originalUser.Country = User.Country;
            _originalUser.Email = User.Email;
            _originalUser.SecurityQuestion = User.SecurityQuestion;
            _originalUser.SecurityAnswer = User.SecurityAnswer;
            // Use SetPassword to keep future hashing changes centralized
            _originalUser.SetPassword(User.Password);
        }

        // Request the view to close and indicate success
        RequestClose?.Invoke(true);
    }

    private void PerformClose() {
        // Discard the copy by just closing without copying back
        RequestClose?.Invoke(false);
    }

    // 6) Bindable state (editable input)
    // The user shown/edited in the view. Can be null until LoadUser is called.
    private User? _user;
    public User? User {
        get => _user;
        set => Set(ref _user, value);
    }

    // 7) Validation and error/feedback properties

    // 8) Derived/computed properties

    // 9) Collections
    public IEnumerable<Country> Countries { get; } =
        Enum.GetValues(typeof(Country)).Cast<Country>();

    public IEnumerable<UserRole> Roles { get; } =
        Enum.GetValues(typeof(UserRole)).Cast<UserRole>();

    // 10) Private helpers/validation
    // Called by the caller (UserListWindowViewModel) to provide the selected user.
    public void LoadUser(User user) {
        // Create a copy so edits are cancellable
        _originalUser = user;
        User = _userManager.CopyUser(user);
    }

    // 11) Nested types (none)
}
