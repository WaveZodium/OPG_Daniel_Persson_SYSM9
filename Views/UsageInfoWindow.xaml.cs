using System.Windows;

using CookMaster.ViewModels;

namespace CookMaster.Views;

/// <summary>
/// Interaction logic for UsageInfo.xaml
/// </summary>
public partial class UsageInfoWindow : Window {
    public UsageInfoWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving UsageInfoWindow
    public UsageInfoWindow(UsageInfoWindowViewModel vm) : this() {
        DataContext = vm;

        vm.RequestClose += result => {
            try {
                DialogResult = result;
            }
            catch {
                // ignore if not shown modally
            }
            Close();
        };
    }
}
