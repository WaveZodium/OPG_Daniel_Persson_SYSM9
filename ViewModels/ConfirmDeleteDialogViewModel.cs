using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class ConfirmDeleteDialogViewModel {
    public RelayCommand YesCommand { get; }
    public RelayCommand NoCommand { get; }

    /// <summary>
    /// Raised when the VM wants the dialog closed. Parameter is true for Yes, false for No.
    /// </summary>
    public event Action<bool>? RequestClose;

    public ConfirmDeleteDialogViewModel() {
        YesCommand = new RelayCommand(_ => RequestClose?.Invoke(true));
        NoCommand = new RelayCommand(_ => RequestClose?.Invoke(false));
    }
}