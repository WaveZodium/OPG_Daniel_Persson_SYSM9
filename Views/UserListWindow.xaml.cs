using CookMaster.ViewModels;
using System.Windows;

namespace CookMaster.Views;

public partial class UserListWindow : Window {
    public UserListWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public UserListWindow(UserListWindowViewModel vm) : this() {
        DataContext = vm;
    }
}
