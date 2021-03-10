//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.UninstallPage
{
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for UninstallPageView.xaml
    /// </summary>
    public partial class UninstallPageView : UserControl
    {
        public UninstallPageView(WizardViewModel wizardViewModel)
        {
            this.DataContext = new UninstallPageViewModel(wizardViewModel);
            this.InitializeComponent();
        }
    }
}