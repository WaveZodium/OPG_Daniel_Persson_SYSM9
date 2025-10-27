using System;
using CookMaster.MVVM;

namespace CookMaster.ViewModels;

public class UnsavedChangesDialogViewModel {
    public enum Outcome { Save, Discard, Cancel }

    public RelayCommand SaveCommand { get; }
    public RelayCommand DiscardCommand { get; }
    public RelayCommand CancelCommand { get; }

    /// <summary>
    /// Raised when the VM wants the dialog closed. Parameter is the chosen outcome.
    /// The view/window will map this to DialogResult/Result.
    /// </summary>
    public event Action<Outcome>? RequestClose;

    public UnsavedChangesDialogViewModel() {
        SaveCommand = new RelayCommand(_ => RequestClose?.Invoke(Outcome.Save));
        DiscardCommand = new RelayCommand(_ => RequestClose?.Invoke(Outcome.Discard));
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(Outcome.Cancel));
    }
}