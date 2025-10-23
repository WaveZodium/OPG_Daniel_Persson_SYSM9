using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.ViewModels;
using CookMaster.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CookMaster;

public partial class App : Application {
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e) {
        var services = new ServiceCollection();

        // Register managers as singletons
        services.AddSingleton<UserManager>();
        services.AddSingleton<RecipeManager>();

        // Register viewmodels and windows (transient)
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        services.AddTransient<RegisterWindowViewModel>();
        services.AddTransient<RegisterWindow>();

        services.AddTransient<RecipeListWindowViewModel>();
        services.AddTransient<RecipeListWindow>();

        services.AddTransient<RecipeDetailWindowViewModel>();
        services.AddTransient<RecipeDetailWindow>();

        services.AddTransient<AddRecipeWindowViewModel>();
        services.AddTransient<AddRecipeWindow>();

        services.AddTransient<UserListWindowViewModel>();
        services.AddTransient<UserListWindow>();

        services.AddTransient<UserDetailWindowViewModel>();
        services.AddTransient<UserDetailWindow>();

        // Factory: create a RecipeDetailWindowViewModel given a runtime Recipe.
        services.AddTransient<Func<Recipe, RecipeDetailWindowViewModel>>(sp => recipe
            => ActivatorUtilities.CreateInstance<RecipeDetailWindowViewModel>(sp, recipe));
        // Factory: create a RecipeDetailWindow given a runtime Recipe.
        // The factory creates the RecipeDetailWindowViewModel via
        // ActivatorUtilities so DI resolves other dependencies.
        services.AddTransient<Func<Recipe, RecipeDetailWindow>>(sp => recipe => {
            var vm = ActivatorUtilities.CreateInstance<RecipeDetailWindowViewModel>(sp, recipe);
            return new RecipeDetailWindow(vm);
        });

        Services = services.BuildServiceProvider();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }
}
