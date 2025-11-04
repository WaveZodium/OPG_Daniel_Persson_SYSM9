using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;

using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;
using CookMaster.Services;

namespace CookMaster.ViewModels;

public class AddRecipeWindowViewModel : ViewModelBase {
    // 1) Constants / static

    // 2) Dependencies (injected services/managers)
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IDialogService _dialogService;

    // 3) Events
    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    // 4) Constructors
    public AddRecipeWindowViewModel(RecipeManager recipeManager, UserManager userManager, IDialogService dialogService) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _dialogService = dialogService;

        PerformAddCommand = new RelayCommand(_ => PerformAdd(), CanAdd);
        PerformCancelCommand = new RelayCommand(_ => PerformCancel());

        AddIngredientCommand = new RelayCommand(_ => AddIngredient(), _ => !string.IsNullOrWhiteSpace(NewIngredientText));
        RemoveIngredientCommand = new RelayCommand(_ => RemoveIngredient(), _ => SelectedIngredient != null);

        // Initialize a new empty recipe
        var owner = userManager.CurrentUser;
        if (owner == null) {
            MessageBox.Show("No user is currently logged in. Cannot create a recipe without an owner.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            PerformCancel();
            return;
        }
        Recipe = new Recipe(string.Empty, new List<string>(), string.Empty, RecipeCategory.Unknown, DateTime.Now, DateTime.Now, owner);
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand PerformAddCommand { get; }
    public RelayCommand PerformCancelCommand { get; }
    public RelayCommand AddIngredientCommand { get; }
    public RelayCommand RemoveIngredientCommand { get; }

    private bool CanAdd(object? _) =>
        !string.IsNullOrWhiteSpace(Title) &&
        Ingredients.Count >= 1 &&
        !string.IsNullOrWhiteSpace(Instructions) &&
        Category != RecipeCategory.Unknown;

    private void AddIngredient() {
        var text = NewIngredientText?.Trim();
        if (string.IsNullOrEmpty(text) || Ingredients == null) return;

        Ingredients.Add(text);
        NewIngredientText = string.Empty;
        IsDirty = true;
        AddIngredientCommand.RaiseCanExecuteChanged();
        RemoveIngredientCommand.RaiseCanExecuteChanged();
    }

    private void RemoveIngredient() {
        if (SelectedIngredient == null || Ingredients == null) return;
        Ingredients.Remove(SelectedIngredient);
        SelectedIngredient = null;
        IsDirty = true;
        RemoveIngredientCommand.RaiseCanExecuteChanged();
        AddIngredientCommand.RaiseCanExecuteChanged();
    }

    private void PerformAdd() {
        if (_recipe == null || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Instructions)) {
            MessageBox.Show("Cannot add recipe: recipe data is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // convert ingredients collection back to List<string> for the model
        var ingredients = Ingredients?.ToList() ?? new List<string>();

        var owner = _userManager.CurrentUser;
        if (owner == null) {
            MessageBox.Show("No user is currently logged in. Cannot create a recipe without an owner.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            PerformCancel();
            return;
        }
        Recipe!.EditRecipe(Title, ingredients, Instructions, Category, owner);

        _recipeManager.AddRecipe(Recipe);

        IsDirty = false;
        RequestClose?.Invoke(true);
    }

    private void PerformCancel() {
        // Request the view to close and indicate cancellation
        RequestClose?.Invoke(false);
    }

    // 6) Bindable state (editable input)
    // editable copy exposed to the view
    private Recipe? _recipe;
    public Recipe? Recipe {
        get => _recipe;
        private set {
            _recipe = value;
            // populate Ingredients collection from the recipe
            Ingredients = new ObservableCollection<string>(Recipe?.Ingredients ?? new List<string>());
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Instructions));
            OnPropertyChanged(nameof(Ingredients));
            OnPropertyChanged(nameof(Category));
            // update dependent availability flags when recipe changes
            PerformAddCommand?.RaiseCanExecuteChanged();
            PerformCancelCommand?.RaiseCanExecuteChanged();
            AddIngredientCommand?.RaiseCanExecuteChanged();
            RemoveIngredientCommand?.RaiseCanExecuteChanged();
        }
    }

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

    // Ingredients exposed as ObservableCollection for live updates in the ListBox
    private ObservableCollection<string> _ingredients = new();
    public ObservableCollection<string> Ingredients {
        get => _ingredients;
        private set => Set(ref _ingredients, value);
    }

    private string? _selectedIngredient;
    public string? SelectedIngredient {
        get => _selectedIngredient;
        set {
            if (Set(ref _selectedIngredient, value)) {
                RemoveIngredientCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    private string _newIngredientText = string.Empty;
    public string NewIngredientText {
        get => _newIngredientText;
        set {
            if (Set(ref _newIngredientText, value)) {
                AddIngredientCommand?.RaiseCanExecuteChanged();
            }
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

    // 7) Validation and error/feedback properties

    // 8) Derived/computed properties

    // 9) Collections
    // Expose enum list for ComboBox
    public IEnumerable<RecipeCategory> Categories { get; } =
        Enum.GetValues(typeof(RecipeCategory)).Cast<RecipeCategory>();

    // 10) Private helpers/validation

    // 11) Nested types (none)
}
