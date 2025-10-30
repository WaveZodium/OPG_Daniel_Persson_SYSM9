using System.Windows;
using System.Windows.Input;

namespace CookMaster.Behaviors;

public static class EnterKeyBehavior {
    public static readonly DependencyProperty EnterKeyCommandProperty =
        DependencyProperty.RegisterAttached(
            "EnterKeyCommand",
            typeof(ICommand),
            typeof(EnterKeyBehavior),
            new PropertyMetadata(null, OnEnterKeyCommandChanged));

    public static ICommand? GetEnterKeyCommand(DependencyObject obj) =>
        (ICommand?)obj.GetValue(EnterKeyCommandProperty);

    public static void SetEnterKeyCommand(DependencyObject obj, ICommand? value) =>
        obj.SetValue(EnterKeyCommandProperty, value);

    public static readonly DependencyProperty EnterKeyCommandParameterProperty =
        DependencyProperty.RegisterAttached(
            "EnterKeyCommandParameter",
            typeof(object),
            typeof(EnterKeyBehavior),
            new PropertyMetadata(null));

    public static object? GetEnterKeyCommandParameter(DependencyObject obj) =>
        obj.GetValue(EnterKeyCommandParameterProperty);

    public static void SetEnterKeyCommandParameter(DependencyObject obj, object? value) =>
        obj.SetValue(EnterKeyCommandParameterProperty, value);

    private static void OnEnterKeyCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is UIElement uiElement) {
            // remove existing handler to avoid duplicates
            uiElement.KeyDown -= UiElement_KeyDown;

            if (e.NewValue is ICommand) {
                uiElement.KeyDown += UiElement_KeyDown;
            }
        }
    }

    private static void UiElement_KeyDown(object? sender, KeyEventArgs e) {
        if (e == null || e.Key != Key.Enter || sender is not DependencyObject d) return;

        var command = GetEnterKeyCommand(d);
        if (command == null) return;

        var parameter = GetEnterKeyCommandParameter(d) ?? (d as FrameworkElement)?.DataContext;

        if (command.CanExecute(parameter)) {
            command.Execute(parameter);
            e.Handled = true; // prevent system beep or further handling
        }
    }
}
