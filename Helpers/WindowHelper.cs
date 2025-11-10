using System.Windows;

namespace CookMaster.Helpers;

public static class WindowHelper {
    /// <summary>
    /// Returns the currently active window, falling back to any visible/enabled window, then MainWindow.
    /// Safe to call from background threads (will marshal to UI dispatcher if needed).
    /// </summary>
    public static Window? GetActiveWindow() {
        var app = Application.Current;
        if (app == null) return null;

        // Marshal to UI thread if caller isn't on it
        if (!app.Dispatcher.CheckAccess())
            return app.Dispatcher.Invoke(() => GetActiveWindowInternal(app));

        return GetActiveWindowInternal(app);
    }

    private static Window? GetActiveWindowInternal(Application app) {
        return app.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            ?? app.Windows.OfType<Window>().FirstOrDefault(w => w.IsVisible && w.IsEnabled)
            ?? app.MainWindow;
    }
}