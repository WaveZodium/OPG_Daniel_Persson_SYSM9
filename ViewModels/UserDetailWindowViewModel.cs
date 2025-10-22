using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class UserDetailWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    public UserDetailWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;
    }
}
