//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.WarningPage
{
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

    public class WarningPageViewModel : PageViewModel
    {
        public WarningPageViewModel(WizardViewModel wizardViewModel)
            : base(wizardViewModel)
        {
            P4EXPDownloadLink = Localisation.Wizard_P4EXPDownloadLink;
            this.CanCancel = true;
            this.CanGoToPreviousPage = false; // Warning page is first page.
            this.CanGoToNextPage = true;
            this.NextButtonText = Localisation.Wizard_NextButtonText;
        }
        public string P4EXPDownloadLink { get; set; }

    }
}