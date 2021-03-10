//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.MaintenancePage
{
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for MaintenancePageView.xaml
    /// </summary>
    public partial class MaintenancePageView : UserControl
    {
        public MaintenancePageView(WizardViewModel wizardViewModel)
        {
            this.DataContext = new MaintenancePageViewModel(wizardViewModel);
            this.InitializeComponent();
        }
    }
}