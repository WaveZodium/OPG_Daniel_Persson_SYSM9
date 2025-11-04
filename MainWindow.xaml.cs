using System.Windows;

using CookMaster.ViewModels;

namespace CookMaster;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel vm) : this() {
        DataContext = vm;
    }
}