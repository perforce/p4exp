//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.WarningPage
{
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for WarningPageView.xaml
    /// </summary>
    public partial class WarningPageView : UserControl
    {
        public WarningPageView(WizardViewModel wizardViewModel)
        {
            this.DataContext = new WarningPageViewModel(wizardViewModel);
            this.InitializeComponent();
        }
        private void Hyperlink_RequestNavigate(object sender,
                              System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }
    }
}