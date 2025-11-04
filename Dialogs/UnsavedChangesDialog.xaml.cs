using System.Windows;
using CookMaster.ViewModels.Dialogs;

namespace CookMaster.Dialogs;

/// <summary>
/// Interaction logic for UnsavedChangesDialog.xaml
/// </summary>
public partial class UnsavedChangesDialog : Window {
    public enum DialogResultOption { Save, Discard, Cancel }

    public DialogResultOption Result { get; private set; } = DialogResultOption.Cancel;

    public UnsavedChangesDialog() {
        InitializeComponent();

        var vm = new UnsavedChangesDialogViewModel();
        vm.RequestClose += OnRequestClose;
        DataContext = vm;
    }

    private void OnRequestClose(UnsavedChangesDialogViewModel.Outcome outcome) {
        // Map VM outcome to the window's Result and DialogResult (preserve previous behavior)
        Result = outcome switch {
            UnsavedChangesDialogViewModel.Outcome.Save => DialogResultOption.Save,
            UnsavedChangesDialogViewModel.Outcome.Discard => DialogResultOption.Discard,
            _ => DialogResultOption.Cancel
        };

        // In original implementation Save/Discard set DialogResult = true, Cancel left it as "closed" (we set false).
        // Keep same signaling so existing callers that use ShowDialog() and check HasValue behave identically.
        switch (outcome) {
            case UnsavedChangesDialogViewModel.Outcome.Save:
            case UnsavedChangesDialogViewModel.Outcome.Discard:
                DialogResult = true;
                break;
            default:
                DialogResult = false;
                break;
        }
    }
}
