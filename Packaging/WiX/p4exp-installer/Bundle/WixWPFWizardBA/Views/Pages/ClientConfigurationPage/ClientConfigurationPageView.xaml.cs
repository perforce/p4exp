//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.ClientConfigurationPage
{
    /// <summary>
    ///     Interaction logic for ClientConfigurationPageView.xaml
    /// </summary>
    public partial class ClientConfigurationPageView : System.Windows.Controls.UserControl // To prevent ambiguous reference.
    {
        public ClientConfigurationPageView(WizardViewModel wizardViewModel)
        {
            this.DataContext = new ClientConfigurationPageViewModel(wizardViewModel);
            this.InitializeComponent();
        }
    }
}