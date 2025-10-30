using CookMaster.Managers;
using CookMaster.Models;
using CookMaster.Services;
using CookMaster.ViewModels;
using CookMaster.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Policy;
using System.Windows;
using System.Windows.Shapes;

namespace CookMaster;

public partial class App : Application {
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e) {
        // Ensure App.xaml resources are loaded
        InitializeComponent();

        // Fallback: ensure an implicit Window style exists in App resources so WPF will apply it reliably.
        if (!Resources.Contains(typeof(Window))) {
            var winStyle = new Style(typeof(Window));
            var brush = TryFindResource("WindowBackgroundBrush") as System.Windows.Media.Brush
                        ?? System.Windows.SystemColors.WindowBrush;
            winStyle.Setters.Add(new Setter(Window.BackgroundProperty, brush));
            Resources[typeof(Window)] = winStyle;
        }

        if (Resources.Contains(typeof(System.Windows.Window))) {
            var s = Resources[typeof(System.Windows.Window)] as System.Windows.Style;
            System.Diagnostics.Debug.WriteLine($"Window style found. Setters count: {s?.Setters.Count}");
            foreach (var setter in s?.Setters.OfType<System.Windows.Setter>() ?? Enumerable.Empty<System.Windows.Setter>()) {
                System.Diagnostics.Debug.WriteLine($" Setter: {setter.Property} = {setter.Value}");
            }
        }
        System.Diagnostics.Debug.WriteLine($"Has WindowBackgroundBrush: {Resources.Contains("WindowBackgroundBrush")}");

        // configure DI / get service provider...
        var provider = ConfigureServices().BuildServiceProvider();

        // resolve and show main window after resources are initialized
        var main = provider.GetRequiredService<MainWindow>();
        main.Show();

        base.OnStartup(e);
    }

    // stub for your DI setup
    private IServiceCollection ConfigureServices() {
        var services = new ServiceCollection();

        // Register managers as singletons
        services.AddSingleton<UserManager>();
        services.AddSingleton<RecipeManager>();

        // Register dialog service
        services.AddSingleton<IDialogService, DialogService>();

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

        return services;
    }
}
