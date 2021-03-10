//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.ReadyToInstallPage
{
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for ReadyToInstallPageView.xaml
    /// </summary>
    public partial class ReadyToInstallPageView : UserControl
    {
        public ReadyToInstallPageView(WizardViewModel wizardViewModel)
        {
            this.DataContext = new ReadyToInstallPageViewModel(wizardViewModel);
            this.InitializeComponent();
        }
    }
}