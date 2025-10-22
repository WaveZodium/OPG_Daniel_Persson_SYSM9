using CookMaster.Managers;
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

        Services = services.BuildServiceProvider();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }
}
