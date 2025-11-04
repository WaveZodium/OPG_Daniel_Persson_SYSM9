using System.Windows;

using CookMaster.Services.Contracts;

namespace CookMaster.Services;

/// <summary>
/// Small dialog service abstraction for showing UI dialogs from viewmodels.
/// Keep it minimal for the assignment; extend with more methods as needed.
/// </summary>
public interface IDialogService {
    /// <summary>
    /// Shows the "unsaved changes" dialog and returns the user's choice.
    /// The optional owner parameter is used to parent the dialog window.
    /// Returns null if dialog was not shown or closed unexpectedly.
    /// </summary>
    DialogResultOption? ShowUnsavedChangesDialog(Window? owner = null);

    /// <summary>
    /// Shows a simple Yes/No confirmation dialog asking the user whether they
    /// want to delete the recipe. Returns true for Yes, false for No, and null
    /// if the dialog was not shown or closed unexpectedly.
    /// </summary>
    bool? ShowDeleteConfirmationDialog(Window? owner = null);
}