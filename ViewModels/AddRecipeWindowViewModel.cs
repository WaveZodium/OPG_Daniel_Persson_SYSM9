using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;
using CookMaster.Services;
using System;
using System.Windows;

namespace CookMaster.ViewModels;

public class AddRecipeWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IDialogService _dialogService;

    // the original source recipe (not edited directly)
    private readonly Recipe? _sourceRecipe;

    // editable copy exposed to the view
    private Recipe? _recipe;
    public Recipe? Recipe {
        get => _recipe;
        private set {
            _recipe = value;
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Instructions));
            OnPropertyChanged(nameof(Ingredients));
            OnPropertyChanged(nameof(Category));
            // update dependent availability flags when recipe changes
            PerformAddCommand?.RaiseCanExecuteChanged();
            PerformCancelCommand?.RaiseCanExecuteChanged();
        }
    }

    // Expose enum list for ComboBox
    public IEnumerable<RecipeCategory> Categories { get; } =
        Enum.GetValues(typeof(RecipeCategory)).Cast<RecipeCategory>();

    // Bindable wrapper for Category
    public RecipeCategory Category {
        get => Recipe?.Category ?? RecipeCategory.Other;
        set {
            if (Recipe == null) return;
            if (Recipe.Category == value) return;
            Recipe.Category = value;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    // Track whether any editable field was changed
    private bool _isDirty;
    public bool IsDirty {
        get => _isDirty;
        private set {
            if (Set(ref _isDirty, value)) {
                // update Save button availability
                PerformAddCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    // Expose editable Title as a VM property that updates Recipe and sets IsDirty
    public string Title {
        get => Recipe?.Title ?? string.Empty;
        set {
            if (Recipe == null) return;
            if (Recipe.Title == value) return;
            Recipe.Title = value;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public List<string> Ingredients {
        get => Recipe?.Ingredients ?? new List<string>();
        set {
            if (Recipe == null) return;
            if (Recipe.Ingredients == value) return;
            Recipe.Ingredients = value;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string Instructions {
        get => Recipe?.Instructions ?? string.Empty;
        set {
            if (Recipe == null) return;
            if (Recipe.Instructions == value) return;
            Recipe.Instructions = value;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public RelayCommand PerformAddCommand { get; }
    public RelayCommand PerformCancelCommand { get; }


    public AddRecipeWindowViewModel(RecipeManager recipeManager, UserManager userManager, IDialogService dialogService) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _dialogService = dialogService;

        PerformAddCommand = new RelayCommand(_ => PerformAdd());
        PerformCancelCommand = new RelayCommand(_ => PerformCancel());

        // Initialize a new empty recipe
        Recipe = new Recipe(string.Empty, new List<string>(), string.Empty, RecipeCategory.Unknown, userManager.CurrentUser);
    }

    private void PerformAdd() {
        if (_recipe == null || string.IsNullOrWhiteSpace(Title) /*|| Ingredients.Count == 0*/ || string.IsNullOrWhiteSpace(Instructions)) {
            MessageBox.Show("Cannot add recipe: recipe data is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        List<string> ingredients = new List<string> {
            "1 cup flour",
            "2 eggs",
            "1/2 cup sugar"
        };
        Ingredients = ingredients;

        Recipe!.EditRecipe(Title, Ingredients, Instructions, Category);

        _recipeManager.AddRecipe(Recipe);

        IsDirty = false;
        RequestClose?.Invoke(true);
    }

    private void PerformCancel() {
        MessageBox.Show($"Recipe addition cancelled. IsDirty={IsDirty}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        // Request the view to close and indicate cancellation
        RequestClose?.Invoke(false);
    }
}
