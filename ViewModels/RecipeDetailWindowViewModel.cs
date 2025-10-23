using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;
using System.Windows;

namespace CookMaster.ViewModels;

public class RecipeDetailWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

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

    public RelayCommand PerformSaveCommand { get; }
    public RelayCommand PerformDeleteCommand { get; }
    public RelayCommand PerformCloseCommand { get; }

    public RecipeDetailWindowViewModel(RecipeManager recipeManager, UserManager userManager, IServiceProvider services, Recipe? sourceRecipe) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _services = services;

        Recipe = sourceRecipe?.CopyRecipe();
        IsAdmin = _userManager.IsAdmin;
        _sourceRecipe = sourceRecipe;

        PerformSaveCommand = new RelayCommand(_ => PerformSave(), _ => IsDirty);
        PerformCloseCommand = new RelayCommand(_ => PerformClose());
        PerformDeleteCommand = new RelayCommand(_ => PerformDelete());
    }

    private void PerformSave() {
        if (Recipe == null) return;
        if (_sourceRecipe != null) {
            // Edit existing recipe
            _sourceRecipe.EditRecipe(Recipe.Title, Recipe.Ingredients, Recipe.Instructions, Recipe.Category);
            RequestClose?.Invoke(true);
        }
        else {
            MessageBox.Show("Cannot save changes: source recipe is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //RequestClose?.Invoke(false);
        }
    }

    private void PerformClose() {
        // Cancel: do nothing to the source, just close
        RequestClose?.Invoke(false);
    }

    private void PerformDelete() {
        if (_sourceRecipe != null) {
            _recipeManager.RemoveRecipe(_sourceRecipe);
        }
        RequestClose?.Invoke(true);
    }
}
