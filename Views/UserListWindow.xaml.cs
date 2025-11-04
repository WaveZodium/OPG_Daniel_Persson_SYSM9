using System.Windows;

using CookMaster.ViewModels;

namespace CookMaster.Views;

public partial class UserListWindow : Window {
    public UserListWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public UserListWindow(UserListWindowViewModel vm) : this() {
        DataContext = vm;

        // Subscribe to RequestClose. Only set DialogResult when VM explicitly requests success (true).
        vm.RequestClose += result => {
            if (result.HasValue && result.Value) {
                // VM requested success -> set DialogResult so ShowDialog() caller receives 'true'
                try {
                    DialogResult = true;
                }
                catch {
                    // ignore if not opened modally
                }
            }
            // For null or false we just close — this mirrors clicking the X (which returns null)
            Close();
        };
    }
}
