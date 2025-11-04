using CookMaster.MVVM;

namespace CookMaster.ViewModels.Dialogs;

public class UnsavedChangesDialogViewModel {
    // 1) Constants / static

    // 2) Dependencies (injected services/managers)

    // 3) Events
    /// <summary>
    /// Raised when the VM wants the dialog closed. Parameter is the chosen outcome.
    /// The view/window will map this to DialogResult/Result.
    /// </summary>
    public event Action<Outcome>? RequestClose;

    // 4) Constructors
    public UnsavedChangesDialogViewModel() {
        SaveCommand = new RelayCommand(_ => RequestClose?.Invoke(Outcome.Save));
        DiscardCommand = new RelayCommand(_ => RequestClose?.Invoke(Outcome.Discard));
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(Outcome.Cancel));
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand SaveCommand { get; }
    public RelayCommand DiscardCommand { get; }
    public RelayCommand CancelCommand { get; }

    // 6) Bindable state (editable input)

    // 7) Validation and error/feedback properties

    // 8) Derived/computed properties

    // 9) Collections

    // 10) Private helpers/validation

    // 11) Nested types
    public enum Outcome { Save, Discard, Cancel }
}