using System.Windows;

namespace CookMaster.Services;

/// <summary>
/// Result options for dialogs that offer Save / Discard / Cancel choices.
/// </summary>
public enum DialogResultOption
{
    Save,
    Discard,
    Cancel
}

/// <summary>
/// Small dialog service abstraction for showing UI dialogs from viewmodels.
/// Keep it minimal for the assignment; extend with more methods as needed.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows the "unsaved changes" dialog and returns the user's choice.
    /// The optional owner parameter is used to parent the dialog window.
    /// Returns null if dialog was not shown or closed unexpectedly.
    /// </summary>
    DialogResultOption? ShowUnsavedChangesDialog(Window? owner = null);
}