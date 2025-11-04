using System.Windows;

using CookMaster.ViewModels;

namespace CookMaster.Views;

public partial class AddRecipeWindow : Window {
    public AddRecipeWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving AddRecipeWindow
    public AddRecipeWindow(AddRecipeWindowViewModel vm) : this() {
        DataContext = vm;

        vm.RequestClose += result => {
            try {
                DialogResult = result;
            }
            catch {
                // ignore if not opened modally
            }
            Close();
        };
    }
}
