using System.Windows;

using CookMaster.ViewModels;

namespace CookMaster.Views;

public partial class RecipeListWindow : Window {
    public RecipeListWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public RecipeListWindow(RecipeListWindowViewModel vm) : this() {
        DataContext = vm;
    }
}
