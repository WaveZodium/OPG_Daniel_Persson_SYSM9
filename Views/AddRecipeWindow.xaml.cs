using CookMaster.ViewModels;
using System.Windows;

namespace CookMaster.Views;

/// <summary>
/// Interaction logic for AddRecipeWindow.xaml
/// </summary>
public partial class AddRecipeWindow : Window {
    public AddRecipeWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public AddRecipeWindow(AddRecipeWindowViewModel vm) : this() {
        DataContext = vm;
    }
}
