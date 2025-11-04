using System.Windows;

using CookMaster.Dialogs;
using CookMaster.Services.Contracts;

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

    public bool? ShowDeleteConfirmationDialog(Window? owner = null) {
        var dlg = new ConfirmDeleteDialog();
        if (owner != null) dlg.Owner = owner;

        // ShowDialog returns nullable bool: true = Yes, false = No, null = closed unexpectedly
        var shown = dlg.ShowDialog();

        if (!shown.HasValue) return null;

        return shown.Value;
    }
}