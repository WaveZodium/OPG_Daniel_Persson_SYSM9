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
