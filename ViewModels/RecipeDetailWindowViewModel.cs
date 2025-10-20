using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class RecipeDetailWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;

    public RecipeDetailWindowViewModel(RecipeManager recipeManager) {
        _recipeManager = recipeManager;
    }
}
