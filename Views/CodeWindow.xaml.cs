using System;
using System.Windows;

using CookMaster.ViewModels;

namespace CookMaster.Views;

/// <summary>
/// Interaction logic for CodeWindow.xaml
/// </summary>
public partial class CodeWindow : Window {
    public CodeWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving ForgotPassword
    public CodeWindow(CodeWindowViewModel vm) : this() {
        DataContext = vm;

        vm.RequestClose += result => {
            try { DialogResult = result; } catch { /* non-modal */ }
            Close();
        };
    }
}
