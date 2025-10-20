using System.Windows.Input;

namespace CookMaster.MVVM;

class RelayCommand : ICommand {
    // Field to hold references to methods that define what to do (Execute)
    private Action<object> execute;

    // Checks if the command can be executed
    private Func<object, bool> canExecute;

    // Event that signals when the command's ability to execute has changed
    public event EventHandler? CanExecuteChanged {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    // Constructor that takes methods for Execute and CanExecute
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null) {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    // Determines whether the command can be executed or not
    public bool CanExecute(object? parameter) {
        return canExecute == null || canExecute(parameter);
    }

    // Runs the logic assigned via the execute method
    public void Execute(object? parameter) {
        execute(parameter);
    }
}
