using System.Windows;
using System.Windows.Controls;

namespace CookMaster.Behaviors;

// Helper class that enables two-way MVVM binding to a PasswordBox's Password.
// PasswordBox.Password is not a DependencyProperty, so we expose an attached
// DependencyProperty that ViewModels can bind to. The assistant synchronizes
// the attached property and the PasswordBox.Password while avoiding recursion.
public static class PasswordBoxAssistant {
    // The property exposed to XAML: BoundPassword.
    // Example XAML usage:
    //   behaviors:PasswordBoxAssistant.BindPassword="True"
    //   behaviors:PasswordBoxAssistant.BoundPassword="{Binding Password, Mode=TwoWay}"
    //
    // This registered attached DP holds the string value that ViewModels bind to.
    // When the DP changes (e.g., ViewModel updated), OnBoundPasswordChanged runs
    // and writes the new value into the PasswordBox.Password (unless currently updating).
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

    // Optional toggle used in XAML to enable/disable the password binding behavior.
    // When true, the assistant hooks the PasswordChanged event; when false, it unhooks.
    // This is a convenience to avoid attaching handlers unless explicitly requested.
    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BindPassword",
            typeof(bool),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(false, OnBindPasswordChanged));

    // Internal recursion-guard attached property.
    // We set this flag while programmatically writing PasswordBox.Password so that
    // the PasswordChanged handler can detect the programmatic update and not write
    // back into the BoundPassword DP (avoids infinite loops).
    private static readonly DependencyProperty UpdatingPasswordProperty =
        DependencyProperty.RegisterAttached(
            "UpdatingPassword",
            typeof(bool),
            typeof(PasswordBoxAssistant),
            new PropertyMetadata(false));

    // Getter / setter helpers for the public BoundPassword DP.
    // These are used by XAML infrastructure and by our code to get/set the value.
    public static string GetBoundPassword(DependencyObject dp) => (string)dp.GetValue(BoundPasswordProperty);
    public static void SetBoundPassword(DependencyObject dp, string value) => dp.SetValue(BoundPasswordProperty, value);

    // Getter / setter helpers for the BindPassword toggle.
    public static bool GetBindPassword(DependencyObject dp) => (bool)dp.GetValue(BindPasswordProperty);
    public static void SetBindPassword(DependencyObject dp, bool value) => dp.SetValue(BindPasswordProperty, value);

    // Getter / setter helpers for the internal UpdatingPassword flag.
    private static bool GetUpdatingPassword(DependencyObject dp) => (bool)dp.GetValue(UpdatingPasswordProperty);
    private static void SetUpdatingPassword(DependencyObject dp, bool value) => dp.SetValue(UpdatingPasswordProperty, value);

    // Called when the BoundPassword attached DP changes (e.g., VM updated the bound property).
    // We must write the new value into the PasswordBox.Password, but only if the
    // change wasn't initiated by the PasswordBox itself (guarded by UpdatingPassword).
    private static void OnBoundPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e) {
        // Ensure we are attached to a PasswordBox; otherwise do nothing.
        if (dp is not PasswordBox passwordBox) return;

        // Defensive unsubscribe: prevents multiple subscriptions if this method is called
        // multiple times for the same control (avoids duplicate event handlers).
        passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

        // If we are not currently updating the PasswordBox (i.e., the change came
        // from the ViewModel), write the new password into the control.
        //
        // Note: writing to Password programmatically will raise PasswordChanged; that's
        // why we set/unset the UpdatingPassword flag in the handler to avoid a loop.
        if (!GetUpdatingPassword(passwordBox)) {
            // e.NewValue may be null — ensure we set an empty string in that case.
            passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
        }

        // (Re)attach the handler so the control will notify us when the user types.
        passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
    }

    // Called when the BindPassword property changes.
    // This toggles whether we listen to PasswordChanged on the PasswordBox.
    // Using a toggle avoids attaching handlers globally by default.
    private static void OnBindPasswordChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e) {
        if (dp is not PasswordBox passwordBox) return;

        // If the previous value was true, remove our handler (cleanup).
        if ((bool)e.OldValue) {
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
        }

        // If the new value is true, attach the handler so we can sync user input to the DP.
        if ((bool)e.NewValue) {
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }
    }

    // This handler runs whenever the PasswordBox.Password changes (user typed or programmatic).
    // We copy the control's Password into the attached BoundPassword DP, which propagates
    // back to any ViewModel property bound to BoundPassword (two-way).
    //
    // The UpdatingPassword flag is used to prevent OnBoundPasswordChanged from writing back
    // into the control while we are copying — that prevents recursion.
    private static void PasswordBox_PasswordChanged(object? sender, RoutedEventArgs e) {
        if (sender is not PasswordBox passwordBox) return;

        // Indicate we are programmatically updating the DP so the DP->control sync ignores it.
        SetUpdatingPassword(passwordBox, true);

        // Copy the current (typed) password into the BoundPassword DP. The DP system will
        // notify any bound ViewModel property (TwoWay binding) so the VM receives the text.
        SetBoundPassword(passwordBox, passwordBox.Password);

        // Clear the flag so subsequent VM-driven changes will propagate to the control.
        SetUpdatingPassword(passwordBox, false);
    }
}
