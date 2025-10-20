using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class RegisterWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;

    public RegisterWindowViewModel(UserManager userManager) {
        _userManager = userManager;
    }
}
