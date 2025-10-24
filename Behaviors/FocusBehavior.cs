using System.Windows;
using System.Windows.Input;

namespace CookMaster.Behaviors;

public static class FocusBehavior {
    public static readonly DependencyProperty IsFocusedProperty =
        DependencyProperty.RegisterAttached(
            "IsFocused",
            typeof(bool),
            typeof(FocusBehavior),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsFocusedChanged));

    public static bool GetIsFocused(DependencyObject obj) => (bool)obj.GetValue(IsFocusedProperty);
    public static void SetIsFocused(DependencyObject obj, bool value) => obj.SetValue(IsFocusedProperty, value);

    private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is not FrameworkElement element) return;

        // detach old handlers (safe even if not attached)
        element.GotFocus -= Element_GotFocus;
        element.LostFocus -= Element_LostFocus;

        // attach handlers to update bound property when focus changes
        element.GotFocus += Element_GotFocus;
        element.LostFocus += Element_LostFocus;

        if ((bool)e.NewValue) {
            // If element isn't loaded/visible yet, wait for Loaded
            if (!element.IsVisible) {
                RoutedEventHandler? loaded = null;
                loaded = (_, __) => {
                    element.Loaded -= loaded;
                    element.Dispatcher.BeginInvoke(() => {
                        element.Focus();
                        Keyboard.Focus(element);
                    });
                };
                element.Loaded += loaded;
            }
            else {
                element.Dispatcher.BeginInvoke(() => {
                    element.Focus();
                    Keyboard.Focus(element);
                });
            }
        }
    }

    private static void Element_GotFocus(object? sender, RoutedEventArgs e) {
        if (sender is DependencyObject d) {
            // update attached prop so bound VM property receives true
            d.SetValue(IsFocusedProperty, true);
        }
    }

    private static void Element_LostFocus(object? sender, RoutedEventArgs e) {
        if (sender is DependencyObject d) {
            // update attached prop so bound VM property receives false
            d.SetValue(IsFocusedProperty, false);
        }
    }
}
