using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WixWPFWizardBA.Dialogs.ServerHelp
{
    /// <summary>
    /// Interaction logic for ServerHelpDialog.xaml
    /// </summary>
    public partial class ServerHelpDialog : Window
    {
        private ServerHelpDialogViewModel _viewModel;

        public ServerHelpDialog()
        {
            InitializeComponent();
        }

        public ServerHelpDialogViewModel ViewModel
        {
            get
            {
                return _viewModel = _viewModel ?? new ServerHelpDialogViewModel();
            }
        }

        public string DialogTitle
        {
            get { return ViewModel.DialogTitle; }
            set { ViewModel.DialogTitle = value; }
        }

        private void CloseClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
