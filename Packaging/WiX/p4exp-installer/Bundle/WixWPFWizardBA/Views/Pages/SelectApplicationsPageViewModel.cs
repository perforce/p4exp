//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.SelectApplicationsPage
{
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

    public class SelectApplicationsPageViewModel : PageViewModel
    {
        public SelectApplicationsPageViewModel(WizardViewModel wizardViewModel)
            : base(wizardViewModel)
        {
            this.CanCancel = true;
            this.CanGoToPreviousPage = false;
            this.CanGoToNextPage = true;
        }

        public SimpleCommand LaunchUninstallCommand { get; }

        public SimpleCommand LaunchRepairCommand { get; }

        public SimpleCommand LaunchModifyCommand { get; }

        public SimpleCommand LaunchLayoutCommand { get; }

        public SimpleCommand LaunchInstallCommand { get; }

        public SimpleCommand LaunchUpdateCommand { get; }
    }
}