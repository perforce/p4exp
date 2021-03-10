using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WixWPFWizardBA.Dialogs.FolderBrowser
{
    /// <summary>
    /// Interaction logic for FolderBrowserDialog.xaml
    /// </summary>
    public partial class FolderBrowserDialog : Window
    {
        private BrowserViewModel _viewModel;

        public BrowserViewModel ViewModel
        {
            get
            { 
                return _viewModel = _viewModel ?? new BrowserViewModel();
            }
        }

        public string DialogTitle
        {
            get { return ViewModel.DialogTitle; }
            set { ViewModel.DialogTitle = value; }
        }
        public string Description
        {
            set { ViewModel.Description = value; }
        }

        public string SelectedFolder
        {
            get { return ViewModel.SelectedFolder; }
            set { ViewModel.SelectedFolder = value; }
        }

        public FolderBrowserDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // TODO Do more checks on the path in IsPathValid.
            if (Utilities.SystemInformationUtilities.IsPathValid(ViewModel.SelectedFolder))
            {
                DialogResult = true;
            }
            else
            {
                System.Windows.MessageBox.Show(Localisation.Wizard_BadPath, DialogTitle, MessageBoxButton.OK);
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                // close the dialog on a double-click of a folder
                DialogResult = true;
            }
        }
    }
}
