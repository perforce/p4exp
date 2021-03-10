//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.ReadyToInstallPage
{
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

    public class ReadyToInstallPageViewModel : PageViewModel
    {
        public ReadyToInstallPageViewModel(WizardViewModel wizardViewModel)
            : base(wizardViewModel)
        {
            this.CanCancel = true;
            this.CanGoToPreviousPage = true;
            this.CanGoToNextPage = true;
            this.NextButtonText = Localisation.Wizard_InstallButtonText;
        }
    }
}