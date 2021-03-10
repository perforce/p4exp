//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA
{
    public enum PageType
    {
        SelectApplicationsPage,
        ClientConfigurationPage,
        ReadyToInstallPage,
        PlanPage,
        ProgressPage,
        FinishPage,
        FinishErrorPage,
        MaintenancePage,
        UninstallPage,
        UpgradePage,
        WarningPage,
        None,
        BootstrapperUpdateCheckPage, // We keep these for now since more work to remove.
        BootstrapperUpdateAvailablePage
    }
}