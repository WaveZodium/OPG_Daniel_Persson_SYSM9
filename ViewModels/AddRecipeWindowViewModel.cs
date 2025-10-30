using System;
using System.Windows;
using CookMaster.MVVM;
using CookMaster.Managers;
using CookMaster.Models;

namespace CookMaster.ViewModels;

public class AddRecipeWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    // Close event for the view (parameter indicates success)
    public event Action<bool>? RequestClose;

    public RelayCommand PerformAddCommand { get; }
    public RelayCommand PerformCancelCommand { get; }

    private Recipe? _recipe;
    public Recipe? Recipe {
        get => _recipe;
        private set {
            _recipe = value;
            //OnPropertyChanged(nameof(Title));
            //OnPropertyChanged(nameof(Instructions));
            //OnPropertyChanged(nameof(Ingredients));
            //OnPropertyChanged(nameof(Category));

            //UpdateOwnerOrAdmin();
            //PerformSaveCommand?.RaiseCanExecuteChanged();
            //PerformDeleteCommand?.RaiseCanExecuteChanged();
            PerformAddCommand?.RaiseCanExecuteChanged();
        }
    }

    public AddRecipeWindowViewModel(RecipeManager recipeManager, UserManager userManager, IServiceProvider services) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _services = services;

        PerformAddCommand = new RelayCommand(_ => PerformAdd());
        PerformCancelCommand = new RelayCommand(_ => PerformCancel());

        // Initialize a new empty recipe
        Recipe = new Recipe(string.Empty, new List<string>(), string.Empty, RecipeCategory.Unknown, userManager.CurrentUser);
    }

    private void PerformAdd() {
        /*if (_recipe == null) {
            MessageBox.Show("Cannot add recipe: recipe data is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }*/
        //if (_sourceRecipe != null) {
        // Edit existing recipe
        //_recipe.EditRecipe(_recipe.Title, _recipe.Ingredients, _recipe.Instructions, _recipe.Category);


        List<string> ingredients = new List<string> {
            "1 cup flour",
            "2 eggs",
            "1/2 cup sugar"
        };

        Recipe.EditRecipe("Titel på receptet", ingredients, "Instruktioner här...", RecipeCategory.Dessert);

        _recipeManager.AddRecipe(Recipe);

        //IsDirty = false;
        RequestClose?.Invoke(true);
    }

    private void PerformCancel() {
        // Request the view to close and indicate cancellation
        RequestClose?.Invoke(false);
    }
}
