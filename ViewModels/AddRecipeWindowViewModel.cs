using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class AddRecipeWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    public AddRecipeWindowViewModel(RecipeManager recipeManager, UserManager userManager, IServiceProvider services) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _services = services;
    }
}
