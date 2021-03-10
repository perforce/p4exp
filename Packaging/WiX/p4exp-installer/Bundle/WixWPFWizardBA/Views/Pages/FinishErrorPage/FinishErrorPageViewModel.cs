//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.FinishErrorPage
{
    using System.Windows.Input;
    using System.IO; // Path
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper; // LogLevel

    public class FinishErrorPageViewModel : PageViewModel
    {
        public FinishErrorPageViewModel(WizardViewModel wizardViewModel) : base(wizardViewModel)
        {
            P4VDownloadLink = Localisation.FinishErrorPage_P4VDownloadLink;
            P4EXPDownloadLink = Localisation.Wizard_P4EXPDownloadLink;
            ShowLogFilesChecked = false;
            this.NextButtonText = Localisation.FinishPage_ExitButtonText;
            this.NextPageCommand = new SimpleCommand(_ =>
                {
                    if (ShowLogFilesChecked)
                    {
                        OpenLogFiles();
                    }
                    this.Bootstrapper.Engine.Quit(wizardViewModel.Status);
                },
                _ => true);
            this.CanCancel = false;
            this.CanGoToPreviousPage = false;
            this.CanGoToNextPage = true;
        }

        public string ErrorTitle
        {
            get
            {
                switch ((uint) this.WizardViewModel.Status)
                {
                    case 0:
                        return ""; // A prerequisite error, so ErrorMessage was set, and don't need ErrorTitle.
                    case 0x80070642u:
                        return Localisation.FinishErrorPage_FinishErrorCanceled;
                    case 0x80072ee7u:
                        return Localisation.FinishErrorPage_FinishErrorConnectionError;
                    case 0x80072EFDu:
                        return Localisation.FinishErrorPage_FinishErrorCouldntDownloadInstallPackages;
                    case 0x80070002u:
                        return Localisation.FinishErrorPage_FinishErrorCouldntRetrieveInstallPackages;
                    //case 0x84BE0BC2u:
                    //    return Localisation.FinishErrorPage_FinishErrorSqlServerRebootPending;
                    default:
                        return string.Format(Localisation.FinishErrorPage_FinishErrorUnknown,
                            this.WizardViewModel.Status);
                }
            }
        }

        public bool ShowLogFilesChecked { get; set; }

        public override ICommand NextPageCommand { get; }

        public string P4VDownloadLink { get; set; }

        public string P4EXPDownloadLink { get; set; }

        // If there was an error, it's helpful for the user to view the log
        // files to locate the cause of the error, or save the log files
        // to a location/name to give to Perforce support.
        public void OpenLogFiles()
        {
            string setupLog = string.Empty;

            try
            {
                setupLog = this.Bootstrapper.Engine.StringVariables["P4EXPSetupLogFile"]; // Set in Bundle.wxs.
            }
            catch (System.Exception e)
            {
                this.Bootstrapper.Engine.Log(LogLevel.Standard, $"Info: caught exception: string P4EXPSetupLogFile doesn't exist: {e}");
            }

            string msiLog = string.Empty;

            try
            {
                msiLog = this.Bootstrapper.Engine.StringVariables["WixBundleLog_P4EXP"]; // Constructed from default name and MSI/@PackageID (?) in Bundle.wxs.
            }
            catch (System.Exception e)
            {
                this.Bootstrapper.Engine.Log(LogLevel.Standard, $"Info: caught exception: string WixBundleLog_P4EXP doesn't exist: {e}");
            }

            // Get app to open log files.

            string systemFolder = this.Bootstrapper.Engine.StringVariables["SystemFolder"];
            string notepadExe = Path.Combine(systemFolder, "notepad.exe");

            if (!string.IsNullOrEmpty(setupLog) && File.Exists(setupLog))
            {
                System.Diagnostics.Process processSetupLog = new System.Diagnostics.Process();
                processSetupLog.StartInfo.FileName = notepadExe;
                processSetupLog.StartInfo.Arguments = setupLog;
                try
                {
                    processSetupLog.Start();
                }
                catch (System.Exception e)
                {
                    this.Bootstrapper.Engine.Log(LogLevel.Standard, $"Error when attempting to open Setup log file: {e}");
                }
            }

            if (!string.IsNullOrEmpty(msiLog) && File.Exists(msiLog))
            {
                System.Diagnostics.Process processMsiLog = new System.Diagnostics.Process();
                processMsiLog.StartInfo.FileName = notepadExe;
                processMsiLog.StartInfo.Arguments = msiLog;
                try
                {
                    processMsiLog.Start();
                }
                catch (System.Exception e)
                {
                    this.Bootstrapper.Engine.Log(LogLevel.Standard, $"Error when attempting to open MSI log file: {e}");
                }
            }
        }
    }
}