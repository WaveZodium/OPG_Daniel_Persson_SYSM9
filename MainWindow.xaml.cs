using CookMaster.ViewModels;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Controls;

namespace CookMaster;

public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel vm) : this() {
        DataContext = vm;
    }
}