using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class RecipeDetailWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    private Recipe? _recipe;
    public Recipe? Recipe {
        get => _recipe;
        set => Set(ref _recipe, value);
    }

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    // Added IsAdmin backing field and property so XAML can bind visibility
    private bool _isAdmin;
    public bool IsAdmin {
        get => _isAdmin;
        private set => Set(ref _isAdmin, value);
    }

    public RelayCommand PerformSaveCommand { get; }
    public RelayCommand PerformDeleteCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    public RecipeDetailWindowViewModel(RecipeManager recipeManager, UserManager userManager, IServiceProvider services, Recipe? recipe) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _services = services;

        Recipe = recipe;
        IsAdmin = _userManager.IsAdmin;

        PerformSaveCommand = new RelayCommand(_ => PerformSave());
        PerformCloseCommand = new RelayCommand(_ => PerformClose());
        PerformDeleteCommand = new RelayCommand(_ => PerformDelete());
    }

    private void PerformSave() {
        // Request the view to close and indicate success
        RequestClose?.Invoke(true);
    }

    private void PerformClose() {
        // Request the view to close and indicate cancellation
        // which in this case is that no changes were saved.
        RequestClose?.Invoke(false);
    }

    private void PerformDelete() {
        RequestClose?.Invoke(true);
    }
}
