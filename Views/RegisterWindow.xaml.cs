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
    }
}
