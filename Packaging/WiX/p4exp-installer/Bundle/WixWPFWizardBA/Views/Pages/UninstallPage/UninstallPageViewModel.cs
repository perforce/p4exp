//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.UninstallPage
{
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
    using System.Windows; // RoutedEventArgs

    public class UninstallPageViewModel : PageViewModel
    {
        public UninstallPageViewModel(WizardViewModel wizardViewModel)
            : base(wizardViewModel)
        {
            this.CanCancel = true;
            this.CanGoToPreviousPage = true;
            this.CanGoToNextPage = true;

        }

        public bool DeleteSettingsChecked
        {
            get => (Bootstrapper.Engine.StringVariables["DeleteSettings"] == "1");
            set
            {
                if (value)
                {
                    if (Bootstrapper.Engine.StringVariables["DeleteSettings"] != "1")
                    {
                        Bootstrapper.Engine.StringVariables["DeleteSettings"] = "1";
                        this.OnPropertyChanged(nameof(this.DeleteSettingsChecked));
                    }
                }
                else
                {
                    if (Bootstrapper.Engine.StringVariables["DeleteSettings"] != "0")
                    {
                        Bootstrapper.Engine.StringVariables["DeleteSettings"] = "0";
                        this.OnPropertyChanged(nameof(this.DeleteSettingsChecked));
                    }
                }
            }
        }
    }
}