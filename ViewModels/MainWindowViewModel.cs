using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;

    // Inject UserManager so MainWindowViewModel can access user state/operations
    public MainWindowViewModel(UserManager userManager) {
        _userManager = userManager;
    }
}
