using CookMaster.Managers;
using CookMaster.MVVM;
using CookMaster.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CookMaster.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    private readonly UserManager _userManager;
    private readonly IServiceProvider _services;

    public RelayCommand OpenRecipeListWindowCommand { get; }
    public RelayCommand OpenRegisterWindowCommand { get; }

    // Inject UserManager so MainWindowViewModel can access user state/operations
    public MainWindowViewModel(UserManager userManager, IServiceProvider services) {
        _userManager = userManager;
        _services = services;

        OpenRecipeListWindowCommand = new RelayCommand(_ => OpenRecipeListWindow());
        OpenRegisterWindowCommand = new RelayCommand(_ => OpenRegisterWindow());
    }

    private void OpenRecipeListWindow() {
        // Create a scope so the new window and its dependencies get disposed when closed
        var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<RecipeListWindow>();

        // Dispose the scope when the window closes (non-modal)
        window.Closed += (_, __) => scope.Dispose();

        // Show the recipe list and close the main window (login flow)
        window.Show();

        // Close/hide MainWindow (the VM uses Application.Current to find it)
        var main = Application.Current?.Windows.OfType<MainWindow>().FirstOrDefault();
        main?.Close();
    }
    private void OpenRegisterWindow() {
        // Use a scope and open RegisterWindow as a modal dialog; dispose immediately after
        using var scope = _services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<RegisterWindow>();

        // ShowDialog blocks until closed; scope is disposed when leaving using block
        window.ShowDialog();
    }
}
