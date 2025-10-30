using CookMaster.Managers;
using CookMaster.MVVM;
using CookMaster.Models;

namespace CookMaster.ViewModels;

public class UserDetailWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

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
        // Optionally create a copy if you want edits to be cancellable.
        // For now we pass the instance through so the UI binds directly.
        User = user;
    }

    private void PerformSave() {
        // Persist changes if needed (UserManager APIs can be used here)
        // Request the view to close and indicate success
        RequestClose?.Invoke(true);
    }

    private void PerformClose() {
        // Request the view to close and indicate cancellation
        // which in this case is that no changes were saved.
        RequestClose?.Invoke(false);
    }
}
