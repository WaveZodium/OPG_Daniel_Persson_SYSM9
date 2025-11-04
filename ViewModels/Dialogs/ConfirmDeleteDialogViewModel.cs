using CookMaster.MVVM;

namespace CookMaster.ViewModels.Dialogs;

public class ConfirmDeleteDialogViewModel {
    // 1) Constants / static

    // 2) Dependencies (injected services/managers)

    // 3) Events
    /// <summary>
    /// Raised when the VM wants the dialog closed. Parameter is true for Yes, false for No.
    /// </summary>
    public event Action<bool>? RequestClose;

    // 4) Constructors
    public ConfirmDeleteDialogViewModel() {
        YesCommand = new RelayCommand(_ => RequestClose?.Invoke(true));
        NoCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
    }

    // 5) Commands + Execute/CanExecute
    public RelayCommand YesCommand { get; }
    public RelayCommand NoCommand { get; }

    // 6) Bindable state (editable input)

    // 7) Validation and error/feedback properties

    // 8) Derived/computed properties

    // 9) Collections

    // 10) Private helpers/validation

    // 11) Nested types (none)
}