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

namespace WixWPFWizardBA.Dialogs.DefaultPortWarning
{
    /// <summary>
    /// Interaction logic for ServerHelpDialog.xaml
    /// </summary>
    public partial class DefaultPortWarningDialog : Window
    {
        private DefaultPortWarningDialogViewModel _viewModel;

        public DefaultPortWarningDialog()
        {
            InitializeComponent();
        }

        public DefaultPortWarningDialogViewModel ViewModel
        {
            get
            {
                return _viewModel = _viewModel ?? new DefaultPortWarningDialogViewModel();
            }
        }

        public string DialogTitle
        {
            get { return ViewModel.DialogTitle; }
            set { ViewModel.DialogTitle = value; }
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
