using CookMaster.ViewModels;
using System.Windows;

namespace CookMaster;

public partial class MainWindow : Window {
    // Keep parameterless ctor for designer and compatibility
    public MainWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public MainWindow(MainWindowViewModel vm) : this() {
        DataContext = vm;
    }
}