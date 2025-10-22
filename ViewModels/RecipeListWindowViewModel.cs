using CookMaster.Managers;
using CookMaster.MVVM;
using CookMaster.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CookMaster.ViewModels;

public class RecipeListWindowViewModel : ViewModelBase {
    private readonly RecipeManager _recipeManager;
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    public RelayCommand OpenMainWindowCommand { get; }
    public RelayCommand OpenAddRecipeWindowCommand { get; }
    public RelayCommand OpenRecipeDetailWindowCommand { get; }
    public RelayCommand OpenUserListWindowCommand { get; }

    public RecipeListWindowViewModel(RecipeManager recipeManager, UserManager userManager, IServiceProvider services) {
        _recipeManager = recipeManager;
        _userManager = userManager;
        _services = services;

        OpenMainWindowCommand = new RelayCommand(_ => OpenMainWindow());
        OpenAddRecipeWindowCommand = new RelayCommand(_ => OpenAddRecipeWindow());
        OpenRecipeDetailWindowCommand = new RelayCommand(_ => OpenRecipeDetailWindow());
        OpenUserListWindowCommand = new RelayCommand(_ => OpenUserListWindow());
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
        // Use a scope and open AddRecipeWindow as a modal dialog; dispose immediately after
        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<AddRecipeWindow>();

        // ShowDialog blocks until closed; scope is disposed when leaving using block
        window.ShowDialog();
    }

    private void OpenRecipeDetailWindow() {
        // Use a scope and open RecipeDetailWindow as a modal dialog; dispose immediately after
        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<RecipeDetailWindow>();

        // ShowDialog blocks until closed; scope is disposed when leaving using block
        window.ShowDialog();
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
}
