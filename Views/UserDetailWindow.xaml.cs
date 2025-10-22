using CookMaster.ViewModels;
using System.Windows;

namespace CookMaster.Views;

public partial class UserDetailWindow : Window {
    public UserDetailWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public UserDetailWindow(UserDetailWindowViewModel vm) : this() {
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