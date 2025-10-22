using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class UserDetailWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    public RelayCommand PerformSaveCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    public UserDetailWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;

        PerformSaveCommand = new RelayCommand(_ => PerformSave());
        PerformCloseCommand = new RelayCommand(_ => PerformClose());
    }

    private void PerformSave() {
        // Request the view to close and indicate success
        RequestClose?.Invoke(true);
    }

    private void PerformClose() {
        // Request the view to close and indicate cancellation
        // which in this case is that no changes were saved.
        RequestClose?.Invoke(false);
    }
}
