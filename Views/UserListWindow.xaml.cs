using CookMaster.ViewModels;
using System.Windows;

namespace CookMaster.Views;

/// <summary>
/// Interaction logic for UserListWindow.xaml
/// </summary>
public partial class UserListWindow : Window {
    public UserListWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public UserListWindow(UserListWindowViewModel vm) : this() {
        DataContext = vm;
    }
}
