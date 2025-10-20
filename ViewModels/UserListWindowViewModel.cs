using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class UserListWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;

    public UserListWindowViewModel(UserManager userManager) {
        _userManager = userManager;
    }
}
