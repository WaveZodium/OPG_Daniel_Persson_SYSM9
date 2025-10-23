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
        set {
            _recipe = value;
            // notify wrapper properties
            OnPropertyChanged(nameof(Title));
        }
    }

    // Track whether any editable field was changed
    private bool _isDirty;
    public bool IsDirty {
        get => _isDirty;
        private set {
            if (Set(ref _isDirty, value)) {
                // update Save button availability
                PerformSaveCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    // Added IsAdmin backing field and property so XAML can bind visibility
    private bool _isAdmin;
    public bool IsAdmin {
        get => _isAdmin;
        private set => Set(ref _isAdmin, value);
    }

    // Expose editable Title as a VM property that updates Recipe and sets IsDirty
    public string Title {
        get => Recipe?.Title ?? string.Empty;
        set {
            if (Recipe == null) return;
            if (Recipe.Title == value) return;
            Recipe.Title = value;
            OnPropertyChanged(); // notify binding for Title
            IsDirty = true;
        }
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

        // commands: Save enabled only when IsDirty == true
        PerformSaveCommand = new RelayCommand(_ => PerformSave(), _ => IsDirty);
        PerformCloseCommand = new RelayCommand(_ => PerformClose());
        PerformDeleteCommand = new RelayCommand(_ => PerformDelete());
    }

    private void PerformSave() {
        // persist changes if required (Recipe is updated in-place)
        // e.g. _recipeManager.UpdateRecipe(Recipe) — implement if needed

        // Request the view to close and indicate success
        RequestClose?.Invoke(true);
    }

    private void PerformClose() {
        // Request the view to close and indicate cancellation (no changes saved)
        RequestClose?.Invoke(false);
    }

    private void PerformDelete() {
        // implement deletion via manager if desired before closing
        RequestClose?.Invoke(true);
    }
}
