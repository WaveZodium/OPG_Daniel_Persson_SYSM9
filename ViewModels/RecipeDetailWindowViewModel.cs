using CookMaster.Managers;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class RecipeDetailWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly IServiceProvider _services;

    public RecipeDetailWindowViewModel(RecipeManager recipeManager, IServiceProvider services) {
        _recipeManager = recipeManager;
        _services = services;
    }
}
