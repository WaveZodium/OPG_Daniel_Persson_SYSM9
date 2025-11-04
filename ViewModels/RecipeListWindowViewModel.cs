using System.Collections.ObjectModel;
using System.Windows;

using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.MVVM;
using CookMaster.Services;
using CookMaster.Views;

using Microsoft.Extensions.DependencyInjection;

namespace CookMaster.ViewModels;

public class RecipeListWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogService;

    private ObservableCollection<Recipe> _recipes = new();
    public ObservableCollection<Recipe> Recipes {
        get => _recipes;
        set => Set(ref _recipes, value);
    }

    private Recipe? _selectedRecipe;
    public Recipe? SelectedRecipe {
        get => _selectedRecipe;
        set {
            if (Set(ref _selectedRecipe, value)) {
                // Recompute owner/admin whenever selection changes
                UpdateOwnerOrAdmin();

                // Notify that command availability may have changed
                OpenRecipeDetailWindowCommand?.RaiseCanExecuteChanged();
                PerformDeleteCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    // New bindable property for the logged-in user display
    private string _loggedInUserName = string.Empty;
    public string LoggedInUserName {
        get => _loggedInUserName;
        set => Set(ref _loggedInUserName, value);
    }

    // in the VM, add a bindable IsLoggedIn property and initialize it
    private bool _isLoggedIn;
    public bool IsLoggedIn {
        get => _isLoggedIn;
        private set => Set(ref _isLoggedIn, value);
    }

    // Added IsAdmin backing field and property so XAML can bind visibility
    private bool _isAdmin;
    public bool IsAdmin {
        get => _isAdmin;
        private set => Set(ref _isAdmin, value);
    }

    // Whether current user is owner OR admin (affects Save/Delete availability)
    private bool _isOwnerOrAdmin;
    public bool IsOwnerOrAdmin {
        get => _isOwnerOrAdmin;
        private set {
            if (Set(ref _isOwnerOrAdmin, value)) {
                // update Delete command availability when this changes
                PerformDeleteCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand TryLogoutCommand { get; }
    public RelayCommand OpenMainWindowCommand { get; }
    public RelayCommand OpenAddRecipeWindowCommand { get; }
    public RelayCommand OpenRecipeDetailWindowCommand { get; }
    public RelayCommand PerformDeleteCommand { get; }
    public RelayCommand OpenUserListWindowCommand { get; }

    public RecipeListWindowViewModel(RecipeManager recipeManager, UserManager userManager, IDialogService dialogService, IServiceProvider services) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _services = services;
        _dialogService = dialogService; // FIX: assign dialog service

        // initialize logged-in user display from UserManager
        var logged = _userManager.GetLoggedIn();
        LoggedInUserName = logged != null ? logged.Username : "(not signed in)";
        IsLoggedIn = _userManager.IsLoggedIn;
        IsAdmin = _userManager.IsAdmin;

        if (IsAdmin) { Recipes = new ObservableCollection<Recipe>(_recipeManager.GetAllRecipes()); }
        else { Recipes = new ObservableCollection<Recipe>(_recipeManager.GetByOwner(_userManager.CurrentUser)); }

        // initialize owner/admin state for current selection (if any)
        UpdateOwnerOrAdmin();

        OpenMainWindowCommand = new RelayCommand(_ => OpenMainWindow());
        OpenAddRecipeWindowCommand = new RelayCommand(_ => OpenAddRecipeWindow());

        // canExecute checks selection
        OpenRecipeDetailWindowCommand = new RelayCommand(_ => OpenRecipeDetailWindow(), _ => SelectedRecipe != null);

        // CanExecute for delete depends on selection AND owner/admin
        PerformDeleteCommand = new RelayCommand(_ => PerformDelete(), _ => SelectedRecipe != null && IsOwnerOrAdmin);

        OpenUserListWindowCommand = new RelayCommand(_ => OpenUserListWindow());
        TryLogoutCommand = new RelayCommand(_ => {
            _userManager.SignOut();
            OpenMainWindow();
        });
    }

    private void UpdateOwnerOrAdmin() {
        var current = _userManager.GetLoggedIn();
        IsOwnerOrAdmin = _userManager.IsAdmin || (current != null && SelectedRecipe?.Owner?.Id == current.Id);
    }

    private void OpenMainWindow() {
        // Create a scope so the new window and its dependencies get disposed when closed
        var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<MainWindow>();

        // Dispose the scope when the window closes (non-modal)
        window.Closed += (_, __) => scope.Dispose();

        // Show the recipe list and close the main window (login flow)
        window.Show();

        // Close/hide MainWindow (the VM uses Application.Current to find it)
        var main = Application.Current?.Windows.OfType<RecipeListWindow>().FirstOrDefault();
        main?.Close();
    }

    private void OpenAddRecipeWindow() {
        // Open AddRecipeWindow as a modal dialog using a DI scope
        var owner = Application.Current?.Windows.OfType<RecipeListWindow>().FirstOrDefault();

        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<AddRecipeWindow>();

        if (owner != null) window.Owner = owner;

        // ShowDialog blocks; refresh when the dialog returns true (saved)
        var dialogResult = window.ShowDialog();

        if (dialogResult == true) {
            if (IsAdmin) { Recipes = new ObservableCollection<Recipe>(_recipeManager.GetAllRecipes()); }
            else { Recipes = new ObservableCollection<Recipe>(_recipeManager.GetByOwner(_userManager.CurrentUser)); }
            // selection may have changed -> recompute
            UpdateOwnerOrAdmin();
        }
    }

    private void OpenRecipeDetailWindow() {
        if (SelectedRecipe == null) return;

        var owner = Application.Current?.Windows.OfType<RecipeListWindow>().FirstOrDefault();

        using var scope = _services.CreateScope();

        // Resolve VM factory and create the VM with runtime Recipe
        var vmFactory = scope.ServiceProvider.GetRequiredService<Func<Recipe, RecipeDetailWindowViewModel>>();
        var vm = vmFactory(SelectedRecipe);

        // Create the window with that VM (DI resolves other ctor args if any)
        var window = ActivatorUtilities.CreateInstance<RecipeDetailWindow>(scope.ServiceProvider, vm);

        if (owner != null) window.Owner = owner;

        // ShowDialog returns the DialogResult (true = saved/confirmed, false/null = cancelled/closed)
        var dialogResult = window.ShowDialog();

        // Refresh list only when dialog indicated success (true)
        if (dialogResult == true) {
            if (IsAdmin) { Recipes = new ObservableCollection<Recipe>(_recipeManager.GetAllRecipes()); }
            else { Recipes = new ObservableCollection<Recipe>(_recipeManager.GetByOwner(_userManager.CurrentUser)); }
            UpdateOwnerOrAdmin();
        }
    }

    private void OpenUserListWindow() {
        // Find the currently open RecipeListWindow instance (the one that triggered this command)
        var current = Application.Current?.Windows.OfType<RecipeListWindow>().FirstOrDefault();

        // Hide the recipe list window so it disappears behind the modal dialog
        current?.Hide();

        try {
            // Open the UserListWindow as a modal dialog from a DI scope
            using var scope = _services.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<UserListWindow>();

            // Set the owner to keep proper window parenting (optional, but recommended)
            if (current != null)
                window.Owner = current;

            // Show dialog; this blocks until the userlist window closes (either via X or Close button)
            window.ShowDialog();
        }
        finally {
            // When the dialog closes, restore the same RecipeListWindow instance
            if (current != null) {
                current.Show();
                current.Activate();
            }
        }
    }

    private void PerformDelete() {
        if (!IsOwnerOrAdmin || _selectedRecipe == null) return;

        // Use dialog service instead of MessageBox
        var owner = Application.Current?.Windows.OfType<RecipeDetailWindow>().FirstOrDefault() ?? Application.Current?.MainWindow;
        var confirm = _dialogService.ShowDeleteConfirmationDialog(owner);

        // If dialog wasn't shown or closed unexpectedly, do nothing
        if (!confirm.HasValue) return;

        if (confirm.Value) {
            _recipeManager.RemoveRecipe(_selectedRecipe);
            _selectedRecipe = null;
            if (IsAdmin) { Recipes = new ObservableCollection<Recipe>(_recipeManager.GetAllRecipes()); }
            else { Recipes = new ObservableCollection<Recipe>(_recipeManager.GetByOwner(_userManager.CurrentUser)); }
        }
    }
}
