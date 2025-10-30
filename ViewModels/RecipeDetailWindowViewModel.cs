using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;
using CookMaster.Services;
using CookMaster.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace CookMaster.ViewModels;

public class RecipeDetailWindowViewModel : ViewModelBase {
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
            UpdateOwnerOrAdmin();
            PerformSaveCommand?.RaiseCanExecuteChanged();
            PerformDeleteCommand?.RaiseCanExecuteChanged();
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
                PerformSaveCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    // Whether current user is owner OR admin (affects Save/Delete availability)
    private bool _isOwnerOrAdmin;
    public bool IsOwnerOrAdmin {
        get => _isOwnerOrAdmin;
        private set {
            if (Set(ref _isOwnerOrAdmin, value)) {
                // update Save and Delete command availability when this changes
                PerformSaveCommand?.RaiseCanExecuteChanged();
                PerformDeleteCommand?.RaiseCanExecuteChanged();
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

    public RelayCommand PerformSaveCommand { get; }
    public RelayCommand PerformDeleteCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    public RelayCommand AddIngredientCommand { get; }
    public RelayCommand RemoveIngredientCommand { get; }

    public RecipeDetailWindowViewModel(
        RecipeManager recipeManager,
        UserManager userManager,
        IDialogService dialogService,
        Recipe? sourceRecipe) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _dialogService = dialogService;
        _sourceRecipe = sourceRecipe;

        Recipe = sourceRecipe?.CopyRecipe();
        Ingredients = new ObservableCollection<string>(Recipe!.Ingredients);

        UpdateOwnerOrAdmin();

        // Save allowed only when there are unsaved changes AND user is owner or admin
        PerformSaveCommand = new RelayCommand(_ => PerformSave(), _ => IsDirty && IsOwnerOrAdmin);

        PerformCloseCommand = new RelayCommand(_ => PerformClose());
        // CanExecute for delete depends on owner/admin
        PerformDeleteCommand = new RelayCommand(_ => PerformDelete(), _ => IsOwnerOrAdmin);

        AddIngredientCommand = new RelayCommand(_ => AddIngredient(), _ => !string.IsNullOrWhiteSpace(NewIngredientText));
        RemoveIngredientCommand = new RelayCommand(_ => RemoveIngredient(), _ => SelectedIngredient != null);
    }

    private void UpdateOwnerOrAdmin() {
        var current = _userManager.GetLoggedIn();
        if (Recipe == null || current == null) {
            IsOwnerOrAdmin = _userManager.IsAdmin; // reflect admin even if no detailed recipe/current user
            return;
        }

        IsOwnerOrAdmin = _userManager.IsAdmin || current.Id == Recipe.CreatedBy.Id;
    }

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

    private void PerformSave() {
        if (Recipe == null) return;
        if (_sourceRecipe != null) {
            Recipe.Title = Title;
            Recipe.Ingredients = Ingredients.ToList();
            Recipe.Instructions = Instructions;
            Recipe.Category = Category;

            // Edit existing recipe
            _sourceRecipe.EditRecipe(Recipe.Title, Recipe.Ingredients, Recipe.Instructions, Recipe.Category);
            IsDirty = false;
            RequestClose?.Invoke(true);
        }
        else {
            MessageBox.Show("Cannot save changes: source recipe is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PerformClose() {
        if (IsDirty && IsOwnerOrAdmin) {
            // Ask dialog service instead of constructing dialog here
            var owner = Application.Current?.Windows.OfType<RecipeDetailWindow>().FirstOrDefault() ?? Application.Current?.MainWindow;
            var result = _dialogService.ShowUnsavedChangesDialog(owner);

            if (!result.HasValue) return;

            switch (result.Value) {
                case DialogResultOption.Save:
                    PerformSaveCommand.Execute(null);
                    // PerformSave will invoke RequestClose(true) when done
                    break;

                case DialogResultOption.Discard:
                    Recipe = _sourceRecipe?.CopyRecipe();
                    IsDirty = false;
                    RequestClose?.Invoke(false);
                    break;

                case DialogResultOption.Cancel:
                default:
                    // stay open
                    break;
            }
        }
        else {
            RequestClose?.Invoke(false);
        }
    }

    private void PerformDelete() {
        if (!IsOwnerOrAdmin || _sourceRecipe == null) return;

        // Use dialog service instead of MessageBox
        var owner = Application.Current?.Windows.OfType<RecipeDetailWindow>().FirstOrDefault() ?? Application.Current?.MainWindow;
        var confirm = _dialogService.ShowDeleteConfirmationDialog(owner);

        // If dialog wasn't shown or closed unexpectedly, do nothing
        if (!confirm.HasValue) return;

        if (confirm.Value) {
            _recipeManager.RemoveRecipe(_sourceRecipe);
            RequestClose?.Invoke(true);
        }
    }
}
