using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class RegisterWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;


    public RegisterWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;

    }
}
