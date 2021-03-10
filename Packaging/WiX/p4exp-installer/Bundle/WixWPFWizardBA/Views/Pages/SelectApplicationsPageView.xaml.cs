//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.SelectApplicationsPage
{
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for SelectApplicationsPageView.xaml
    /// </summary>
    public partial class SelectApplicationsPageView : UserControl
    {
        public SelectApplicationsPageView(WizardViewModel wizardViewModel)
        {
            this.DataContext = new SelectApplicationsPageViewModel(wizardViewModel);
            this.InitializeComponent();
        }
    }
}