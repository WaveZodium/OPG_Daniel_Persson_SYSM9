using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using CookMaster.ViewModels;

namespace CookMaster.Views;

/// <summary>
/// Interaction logic for ForgotPassword.xaml
/// </summary>
public partial class ForgotPasswordWindow : Window {

    public ForgotPasswordWindow() {
        InitializeComponent();
    }

    // DI constructor — service provider will use this when resolving ForgotPassword
    public ForgotPasswordWindow(ForgotPasswordViewModel vm) : this() {
        DataContext = vm;
        // Subscribe to the VM close request. Set DialogResult so ShowDialog() in the caller gets the result.
        vm.RequestClose += result => {
            // Only set DialogResult when opened modally; setting DialogResult when not modal throws.
            try {
                DialogResult = result;
            }
            catch {
                // ignore — window might have been opened non-modally in some flows
            }
            Close();
        };
    }
}
