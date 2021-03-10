//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.FinishPage
{
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper; // LogLevel
    using System; // Exception
    using System.Collections.Generic; // List
    using System.Diagnostics; // Process
    using System.Threading; // Sleep
    using System.Windows; // MessageBox
    using System.Windows.Input;
    // Note: to get SHDocVw, add a reference to the project to the COM Microsoft Internet Controls.
	// ALSO need to build using .Net 4.5 (to get Embed Interop Types property for SHDocVw) and
	// add a reference to System.Xaml so don't get IComponentConnector errors.

    public class FinishPageViewModel : PageViewModel
    {
        public FinishPageViewModel(WizardViewModel wizardViewModel) : base(wizardViewModel)
        {
            // Product URLs have a product version in the URL. It's set
            // at build time so we can have the correct version.
            string urlVersion = Bootstrapper.Engine.StringVariables["URLVersion"];
            ReleaseNotesURL = string.Format(Localisation.FinishPage_ReleaseNotesURL, urlVersion);
            TechnicalDocumentationURL = string.Format(Localisation.FinishPage_TechnicalDocumentationURL, urlVersion);
            LicenseURL = string.Format(Localisation.FinishPage_LicenseURL, urlVersion);
            bool restartRequired = wizardViewModel.RestartRequired;
            bool needExplorerRestart = wizardViewModel.LaunchAction != LaunchAction.Uninstall;

            // Don't have a cancel button. Instead use "Exit".
            this.CanCancel = false;

            // Back button is always "Exit".
            this.BackButtonText = Localisation.FinishPage_ExitButtonText;
            this.PreviousPageCommand = new SimpleCommand(
                _ => {
                    if (restartRequired)
                    {
                        MessageBox.Show(Localisation.FinishPage_RememberToRestartSystem);
                    }
                    else if (needExplorerRestart) // Else since don't show if reminding to restart system.
                    {
                        MessageBox.Show(Localisation.FinishPage_RememberToRestartExplorer);
                    }
                    this.Bootstrapper.Engine.Quit(wizardViewModel.Status); }, _ => true);
            this.CanGoToPreviousPage = true;

            this.CanGoToNextPage = false; // Assume only an Exit button.

            // Next button is either "Reboot now" if restart required,
            // or "Restart Explorer" if need an explorer restart.
            if (restartRequired)
			{
                this.CanGoToNextPage = true;
	            this.NextButtonText = Localisation.FinishPage_RestartButtonText;
	            this.NextPageCommand = new SimpleCommand(_ =>
	            {
	                wizardViewModel.RestartConfirmed = true;
	                this.Bootstrapper.Engine.Quit(wizardViewModel.Status);
	            }, _ => true);
			}
			else
			{
                if (needExplorerRestart)
                {
                    this.CanGoToNextPage = true;
                    this.NextButtonText = Localisation.FinishPage_RestartExplorerText;
                    this.NextPageCommand = new SimpleCommand(_ =>
                    {
                        RestartFileExplorer();
                        this.Bootstrapper.Engine.Quit(wizardViewModel.Status);
                    }, _ => true);
                }
            }
        }

        /// <summary>
        /// For P4EXP, when installing, upgrading, or repairing, we need to restart
        /// File Explorer in order for configuration to be complete (mostly the icons).
        /// </summary>
        void RestartFileExplorer()
        {
            // Get the path for each open Explorer window.

            List<string> explorerPaths = new List<string>();
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();

            string filename;

            foreach (SHDocVw.InternetExplorer ie in shellWindows)
            {
                filename = System.IO.Path.GetFileNameWithoutExtension(ie.FullName).ToLower();

                if (filename.Equals("explorer"))
                {
                    explorerPaths.Add(ie.LocationURL);
                }
            }

            // End all the Explorer processes.

            var explorers = Process.GetProcessesByName("explorer");
            foreach (var p in explorers)
            {
                p.Kill();
            }

            // Open the paths for Explorer windows that were closed.

            // To use one explorer process to open multiple paths, pass path to Start.
            // Big advantage to this is that the opened windows won't be highlighted in the
            // Taskbar. A hightlight would make user think they need to view each window.
            foreach (string path in explorerPaths)
            {
                try
                {
                    Process.Start("\"" + path + "\"");
                    Thread.Sleep(1000); // Hopefully this will prevent exception "The remote procedure call failed and did not execute".
                }
                catch (Exception e)
                {
                    this.Bootstrapper.Engine.Log(LogLevel.Standard, "Info (will try again): opening " + path + ". Exception: " + e.Message);
                    try // again!
                    {
                        Process.Start("\"" + path + "\"");
                        Thread.Sleep(1000); // Hopefully this will prevent exception "The remote procedure call failed and did not execute".
                    }
                    catch (Exception ex)
                    {
                        string logMsg = String.Format(Localisation.BootstrapperLog_ExceptionOpeningExplorerWindow,
                            $"{path}", $"{ex.Message}");
                        this.Bootstrapper.Engine.Log(LogLevel.Standard, logMsg);
                    }
                }
            }
        }

        public override ICommand NextPageCommand { get; }
        public override ICommand PreviousPageCommand { get; }

        public string ReleaseNotesURL { get; set; }

        public string TechnicalDocumentationURL { get; set; }

        public string LicenseURL { get; set; }

        public string PromotionURL { get; set; }
    }
}