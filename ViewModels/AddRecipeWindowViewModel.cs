using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class AddRecipeWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;

    public AddRecipeWindowViewModel(RecipeManager recipeManager, UserManager userManager) {
        _recipeManager = recipeManager;
        _userManager = userManager;
    }
}
