using System.Windows;

using CookMaster.ViewModels;
using CookMaster.ViewModels.Contracts;

namespace CookMaster.Views;

public partial class RecipeListWindow : Window {
    public RecipeListWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving MainWindow
    public RecipeListWindow(RecipeListWindowViewModel vm) : this() {
        DataContext = vm;
        vm.SessionEndRequested += OnSessionEndRequested;
    }

    private void OnSessionEndRequested(SessionEndReason reason) {
        // For modal use, you could map reason to a DialogResult if needed.
        Close();
    }

    protected override void OnClosed(EventArgs e) {
        if (DataContext is RecipeListWindowViewModel vm)
            vm.SessionEndRequested -= OnSessionEndRequested;
        base.OnClosed(e);
    }
}
