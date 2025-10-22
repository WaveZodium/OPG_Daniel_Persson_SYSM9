using CookMaster.ViewModels;
using System.Windows;

namespace CookMaster.Views;

public partial class RegisterWindow : Window {
    public RegisterWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public RegisterWindow(RegisterWindowViewModel vm) : this() {
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
