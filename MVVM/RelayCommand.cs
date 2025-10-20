using System.Windows.Input;

namespace CookMaster.MVVM;

public class RelayCommand : ICommand {
    // Field to hold references to methods that define what to do (Execute)
    private readonly Action<object?> _execute;

    // Checks if the command can be executed
    private readonly Func<object?, bool>? _canExecute;

    // Event that signals when the command's ability to _execute has changed
    public event EventHandler? CanExecuteChanged {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    // Constructor that takes methods for Execute and CanExecute
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    // Determines whether the command can be executed or not
    public bool CanExecute(object? parameter) {
        return _canExecute == null || _canExecute(parameter);
    }

    // Runs the logic assigned via the _execute method
    public void Execute(object? parameter) {
        _execute(parameter);
    }

    // Convenience to trigger re-evaluation of CanExecute from viewmodels
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
