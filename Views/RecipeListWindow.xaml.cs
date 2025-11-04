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

        // Subscribe to the VM close request. Set DialogResult so ShowDialog() in the caller gets the result.
        vm.RequestClose += result => {
            // Only set DialogResult when opened modally; setting DialogResult when not modal throws.
            try {
                DialogResult = result;
            }
            catch {
                // ignore — window might have been opened non-modally in some flows
            }
            Close();
        };
    }
}
