using System.Windows;

using CookMaster.ViewModels;

namespace CookMaster.Views {
    /// <summary>
    /// Interaction logic for ConfirmDeleteDialog.xaml
    /// </summary>
    public partial class ConfirmDeleteDialog : Window {
        public ConfirmDeleteDialog() {
            InitializeComponent();

            var vm = new ConfirmDeleteDialogViewModel();
            vm.RequestClose += OnRequestClose;
            DataContext = vm;
        }

        private void OnRequestClose(bool yes) {
            // yes -> DialogResult = true, no -> DialogResult = false
            DialogResult = yes;
        }
    }
}
