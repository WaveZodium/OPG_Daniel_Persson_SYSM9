using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class UserListWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    public UserListWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;
    }
}
