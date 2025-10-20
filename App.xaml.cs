using CookMaster.Managers;
using CookMaster.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CookMaster;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {
    public IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e) {
        var services = new ServiceCollection();

        // register managers as singletons
        services.AddSingleton<UserManager>();
        services.AddSingleton<RecipeManager>();

        // register viewmodels and windows
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainWindow>();

        Services = services.BuildServiceProvider();

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }
}
