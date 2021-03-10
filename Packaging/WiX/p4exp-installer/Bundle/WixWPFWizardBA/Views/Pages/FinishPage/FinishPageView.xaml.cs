//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.FinishPage
{
    using System.Windows.Controls; // UserControl

    /// <summary>
    ///     Interaction logic for FinishPageView.xaml
    /// </summary>
    public partial class FinishPageView : UserControl
    {
        public FinishPageView(WizardViewModel wizardViewModel)
        {
            this.DataContext = new FinishPageViewModel(wizardViewModel);
            this.InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender,
                                       System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }
    }
}