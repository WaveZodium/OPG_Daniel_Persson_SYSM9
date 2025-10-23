using System.Linq;
using System.Windows;
using CookMaster.Views;

namespace CookMaster.Services;

/// <summary>
/// Concrete dialog service that shows views/dialogs from the Views folder.
/// Keeps viewmodels free of direct window creation.
/// </summary>
public class DialogService : IDialogService {
    public DialogResultOption? ShowUnsavedChangesDialog(Window? owner = null) {
        var dlg = new UnsavedChangesDialog();
        if (owner != null) dlg.Owner = owner;

        // ShowDialog returns bool? but the dialog type exposes its Result enum
        var shown = dlg.ShowDialog();

        // If dialog was never shown or closed abnormally, return null
        if (!shown.HasValue) return null;

        // Map the dialog's own enum to the service enum
        return dlg.Result switch {
            UnsavedChangesDialog.DialogResultOption.Save => DialogResultOption.Save,
            UnsavedChangesDialog.DialogResultOption.Discard => DialogResultOption.Discard,
            UnsavedChangesDialog.DialogResultOption.Cancel => DialogResultOption.Cancel,
            _ => null
        };
    }
}