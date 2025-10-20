using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class RecipeListWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;

    public RecipeListWindowViewModel(RecipeManager recipeManager, UserManager userManager) {
        _recipeManager = recipeManager;
        _userManager = userManager;
    }
}
