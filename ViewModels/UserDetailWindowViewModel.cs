using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class UserDetailWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    // Keep original reference so we can copy edits back on Save
    private User? _originalUser;

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    public RelayCommand PerformSaveCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    // The user shown/edited in the view. Can be null until LoadUser is called.
    private User? _user;
    public User? User {
        get => _user;
        set => Set(ref _user, value);
    }
    public IEnumerable<Country> Countries { get; } =
        Enum.GetValues(typeof(Country)).Cast<Country>();

    public IEnumerable<UserRole> Roles { get; } =
        Enum.GetValues(typeof(UserRole)).Cast<UserRole>();


    public UserDetailWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;

        PerformSaveCommand = new RelayCommand(_ => PerformSave());
        PerformCloseCommand = new RelayCommand(_ => PerformClose());
    }

    // Called by the caller (UserListWindowViewModel) to provide the selected user.
    public void LoadUser(User user) {
        // Create a copy so edits are cancellable
        _originalUser = user;
        User = _userManager.CopyUser(user);
    }

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
}
