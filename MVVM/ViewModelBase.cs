using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CookMaster.MVVM;

public class ViewModelBase : INotifyPropertyChanged {
    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler? PropertyChanged;

    // Method to raise the PropertyChanged event
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Convenience setter that raises PropertyChanged only when value changes
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
