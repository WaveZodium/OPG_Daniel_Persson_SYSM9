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

namespace CookMaster.Views {
    /// <summary>
    /// Interaction logic for UnsavedChangesDialog.xaml
    /// </summary>
    public partial class UnsavedChangesDialog : Window {
        public enum DialogResultOption { Save, Discard, Cancel }

        public DialogResultOption Result { get; private set; } = DialogResultOption.Cancel;

        public UnsavedChangesDialog() {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            Result = DialogResultOption.Save;
            DialogResult = true;
        }

        private void Discard_Click(object sender, RoutedEventArgs e) {
            Result = DialogResultOption.Discard;
            DialogResult = true;
        }
    }

}
