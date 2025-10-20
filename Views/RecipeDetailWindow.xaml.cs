using CookMaster.ViewModels;
using System.Windows;

namespace CookMaster.Views;

/// <summary>
/// Interaction logic for RecipeDetailWindow.xaml
/// </summary>
public partial class RecipeDetailWindow : Window {
    public RecipeDetailWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public RecipeDetailWindow(RecipeDetailWindowViewModel vm) : this() {
        DataContext = vm;
    }
}
