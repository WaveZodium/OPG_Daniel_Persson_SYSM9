using System;
using System.Linq;
using System.Collections.Generic;
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
    // 1) Constants / static

    // 2) Dependencies (injected services/managers)
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;
    private readonly IDialogService _dialogService;

    // Unfiltered source list; filters are applied on top of this
    private List<Recipe> _allRecipes = new();

    // 3) Events
    public event Action<bool>? RequestClose;

    // 4) Constructors
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

        UpdateRecipesList();

        // initialize owner/admin state for current selection (if any)
        UpdateOwnerOrAdmin();

        OpenMainWindowCommand = new RelayCommand(_ => OpenMainWindow());
        OpenAddRecipeWindowCommand = new RelayCommand(_ => OpenAddRecipeWindow());
        OpenCopyRecipeWindowCommand = new RelayCommand(_ => OpenCopyRecipeWindow(), _ => SelectedRecipe != null);

        // canExecute checks selection
        OpenRecipeDetailWindowCommand = new RelayCommand(_ => OpenRecipeDetailWindow(), _ => SelectedRecipe != null);

        // CanExecute for delete depends on selection AND owner/admin
        PerformDeleteCommand = new RelayCommand(_ => PerformDelete(), _ => SelectedRecipe != null && IsOwnerOrAdmin);

        OpenUserListWindowCommand = new RelayCommand(_ => OpenUserListWindow());
        TryLogoutCommand = new RelayCommand(_ => {
            _userManager.SignOut();
            OpenMainWindow();
        });

        OpenUserDetailsCommand = new RelayCommand(_ => {
            /*var owner = GetActiveWindow();
            using var scope = _services.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<UserDetailWindow>();
            if (owner != null)
                window.Owner = owner;
            window.ShowDialog();*/

            // open user detail window and pass the selected user to its VM
            using var scope = _services.CreateScope();
            var window = scope.ServiceProvider.GetRequiredService<Views.UserDetailWindow>();

            // Prefer the window's DataContext if it already is the expected VM instance,
            // otherwise resolve a VM from the scope and assign it as the DataContext.
            var vm = window.DataContext as UserDetailWindowViewModel
                     ?? scope.ServiceProvider.GetRequiredService<UserDetailWindowViewModel>();

            // Provide the selected user to the detail VM
            vm.LoadUser(_userManager.CurrentUser);

            // Ensure window is using the VM instance we just prepared
            window.DataContext = vm;

            var result = window.ShowDialog();

            if (result == true) {
                // After the dialog closes, refresh the user list to reflect any changes
                var current = _userManager.GetLoggedIn();
                LoggedInUserName = current != null ? current.Username : "(not signed in)";
            }
        });

        OpenUsageInfoWindowCommand = new RelayCommand(_ => OpenUsageInfoWindow()); // init

        ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand TryLogoutCommand { get; }
    public RelayCommand OpenMainWindowCommand { get; }
    public RelayCommand OpenAddRecipeWindowCommand { get; }
    public RelayCommand OpenCopyRecipeWindowCommand { get; }
    public RelayCommand OpenRecipeDetailWindowCommand { get; }
    public RelayCommand PerformDeleteCommand { get; }
    public RelayCommand OpenUserListWindowCommand { get; }
    public RelayCommand OpenUserDetailsCommand { get; }
    public RelayCommand OpenUsageInfoWindowCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }

    private void OpenMainWindow() {
        // Signal: true => go back to login (logout flow)
        RequestClose?.Invoke(true);
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
            UpdateRecipesList();
            // selection may have changed -> recompute
            UpdateOwnerOrAdmin();
        }
    }

    private void OpenCopyRecipeWindow() {
        if (SelectedRecipe == null) return;

        var owner = Application.Current?.Windows.OfType<RecipeListWindow>().FirstOrDefault();

        using var scope = _services.CreateScope();

        // Create a VM seeded from the selected recipe and pass it to the window
        var vm = ActivatorUtilities.CreateInstance<AddRecipeWindowViewModel>(scope.ServiceProvider, SelectedRecipe);
        var window = ActivatorUtilities.CreateInstance<AddRecipeWindow>(scope.ServiceProvider, vm);

        if (owner != null) window.Owner = owner;

        var dialogResult = window.ShowDialog();

        if (dialogResult == true) {
            UpdateRecipesList();
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
            UpdateRecipesList();
            UpdateOwnerOrAdmin();
        }
    }

    private void OpenUserListWindow() {
        // Use the current RecipeListWindow as owner
        var owner = Application.Current?.Windows.OfType<RecipeListWindow>().FirstOrDefault();

        // Open the UserListWindow as a modal dialog from a DI scope
        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<UserListWindow>();

        if (owner != null)
            window.Owner = owner;

        window.ShowDialog();
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
            UpdateRecipesList();
        }
    }

    private void OpenUsageInfoWindow() {
        var owner = Application.Current?.Windows.OfType<RecipeListWindow>().FirstOrDefault();

        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<UsageInfoWindow>();

        if (owner != null)
            window.Owner = owner;

        window.ShowDialog();
    }

    private void ClearFilters() {
        SelectedCategory = null;
        SelectedDate = null;
        SearchText = string.Empty;
    }

    // 6) Bindable state (editable input)
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
                OpenCopyRecipeWindowCommand?.RaiseCanExecuteChanged();
            }
        }
    }

    // Search/filter inputs
    private string _searchText = string.Empty;
    public string SearchText {
        get => _searchText;
        set {
            if (Set(ref _searchText, value)) {
                ApplyFilters();
            }
        }
    }

    private RecipeCategory? _selectedCategory;
    public RecipeCategory? SelectedCategory {
        get => _selectedCategory;
        set {
            if (Set(ref _selectedCategory, value)) {
                ApplyFilters();
            }
        }
    }

    private DateTime? _selectedDate;
    public DateTime? SelectedDate {
        get => _selectedDate;
        set {
            if (Set(ref _selectedDate, value)) {
                ApplyFilters();
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

    // 7) Validation and error/feedback properties

    // 8) Derived/computed properties

    // 9) Collections
    // Expose enum list for other uses (kept)
    public IEnumerable<RecipeCategory> Categories { get; } =
        Enum.GetValues(typeof(RecipeCategory)).Cast<RecipeCategory>();

    // New: list with a leading null to allow clearing the filter
    public IEnumerable<RecipeCategory?> CategoryFilterOptions { get; } =
        new RecipeCategory?[] { null }.Concat(Enum.GetValues(typeof(RecipeCategory))
                                                .Cast<RecipeCategory>()
                                                .Select(c => (RecipeCategory?)c));

    // 10) Helpers/validation
    private void UpdateOwnerOrAdmin() {
        var current = _userManager.GetLoggedIn();
        IsOwnerOrAdmin = _userManager.IsAdmin || (current != null && SelectedRecipe?.Owner?.Id == current.Id);
    }

    private void UpdateRecipesList() {
        IEnumerable<Recipe> baseList;

        if (IsAdmin) {
            baseList = _recipeManager.GetAllRecipes();
        }
        else {
            baseList = _userManager.CurrentUser != null
                ? _recipeManager.GetByOwner(_userManager.CurrentUser)
                : Enumerable.Empty<Recipe>();
        }

        _allRecipes = baseList.ToList();
        ApplyFilters();
    }

    public void RefreshRecipes() {
        UpdateRecipesList();
        UpdateOwnerOrAdmin();
    }

    private void ApplyFilters() {
        IEnumerable<Recipe> query = _allRecipes;

        // Text filter: match title, category name, or owner username (case-insensitive)
        if (!string.IsNullOrWhiteSpace(_searchText)) {
            var s = _searchText.Trim();
            query = query.Where(r =>
                ContainsIgnoreCase(r.Title, s) ||
                ContainsIgnoreCase(r.Category.ToString(), s) ||
                ContainsIgnoreCase(r.Owner?.Username, s));
        }

        // Category filter (only when a category is selected)
        if (_selectedCategory.HasValue) {
            var cat = _selectedCategory.Value;
            query = query.Where(r => r.Category == cat);
        }

        // Date filter: match recipes created on the selected calendar day (ignore time)
        if (_selectedDate.HasValue) {
            var day = _selectedDate.Value.Date; // local date from DatePicker
            query = query.Where(r => r.Created.ToLocalTime().Date == day);
            // Alternative (equivalent): range match for the day
            // var nextDay = day.AddDays(1);
            // query = query.Where(r => {
            //     var lc = r.Created.ToLocalTime();
            //     return lc >= day && lc < nextDay;
            // });
        }

        Recipes = new ObservableCollection<Recipe>(query.ToList());
    }

    private static bool ContainsIgnoreCase(string? source, string value) {
        if (string.IsNullOrWhiteSpace(source)) return false;
        return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    // 11) Nested types (none)
}
