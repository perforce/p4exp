//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.UpgradePage
{
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

    public class UpgradePageViewModel : PageViewModel
    {
        public UpgradePageViewModel(WizardViewModel wizardViewModel)
            : base(wizardViewModel)
        {
            this.CanCancel = true;
            this.CanGoToPreviousPage = false;
            this.CanGoToNextPage = true;
            wizardViewModel.LaunchAction = LaunchAction.Install; // If don't do this, old install not removed.
        }
    }
}