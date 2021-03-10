//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.MaintenancePage
{
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

    public class MaintenancePageViewModel : PageViewModel
    {
        public MaintenancePageViewModel(WizardViewModel wizardViewModel)
            : base(wizardViewModel)
        {
            this.CanCancel = true;
            this.CanGoToPreviousPage = false;
            this.CanGoToNextPage = false;

            this.LaunchRepairCommand = new SimpleCommand(_ =>
            {
                wizardViewModel.LaunchAction = LaunchAction.Repair;
                this.BeginNextPhase();
            },
                _ => true);

            this.LaunchUninstallCommand = new SimpleCommand(_ =>
            {
                wizardViewModel.LaunchAction = LaunchAction.Uninstall;
                this.BeginNextPhase();
            },
                _ => true);
        }

        public SimpleCommand LaunchRepairCommand { get;  }
        public SimpleCommand LaunchUninstallCommand { get; }
    }
}