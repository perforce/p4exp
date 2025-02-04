//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//

namespace WixWPFWizardBA.Views.Pages.SelectApplicationsPage
{
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
    using System.IO; // File, Path
    using System.Reflection; // Assembly
    using WixWPFWizardBA.Utilities; // WixVariableHelper
    using WixWPFWizardBA.Dialogs.FolderBrowser; // FolderBrowserDialog
    public class SelectApplicationsPageViewModel : PageViewModel
    {
        private readonly WixVariableHelper _selectAppsFolderHelper;
        private string _dialogTitle;


        public SelectApplicationsPageViewModel(WizardViewModel wizardViewModel)
            : base(wizardViewModel)
        {
            this.CanCancel = true;
            this.CanGoToPreviousPage = false; // TODO Allow if doing modify.
            this.CanGoToNextPage = true;

            // TODO: From WelcomePage in original, which Install button
            // clicked. Correct to do it this way?
            wizardViewModel.LaunchAction = LaunchAction.Install;
            // TODO Need this? Or put in ReadyToInstall? this.BeginNextPhase();

            GetVersionOfApps(this);

            this._selectAppsFolderHelper = new WixVariableHelper(wizardViewModel.Bootstrapper, "InstallDir");
            this._dialogTitle = string.Format(Localisation.Wizard_WindowTitle,
                Bootstrapper.Engine.StringVariables["WixBundleName"],
                Bootstrapper.Engine.StringVariables["TitleBarRelease"]);

            this.SelectAppsFolderBrowseCommand = new SimpleCommand(_ =>
            {
                var folderBrowserDialog = new FolderBrowserDialog
                {
                    DialogTitle = this._dialogTitle,
                    SelectedFolder = ApplicationsFolder
                };
                // If user clicks OK then Value is true, so we know to save the selected folder.
                if (folderBrowserDialog.ShowDialog().Value)
                {
                    this.ApplicationsFolder = folderBrowserDialog.SelectedFolder;
                }
            }, _ => true);
        }

        /// <summary>
        /// Read version of each app from versions.txt file (which is cached with bootstrapper).
        /// Assume this doesn't take long to do so can do it each time the dialog opens.
        /// </summary>
        /// <param name="pageViewModel">We need this for access to version variables. TODO: Not best practice?</param>
        private static void GetVersionOfApps(SelectApplicationsPageViewModel pageViewModel)
        {
            string versionFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "versions.txt");

            foreach (string line in File.ReadAllLines(versionFile))
            {
                if (line.Contains("P4EXP"))
                {
                    pageViewModel.p4expVersion = line.Substring(line.LastIndexOf(' ') + 1);
                }
            }
        }

        // This string is our access to the bundle's InstallDir variable.
        // The Get may return the initial path from Bundle.wxs,
        // [ProgramFiles64Folder]Perforce\, so format it to get the path.
        public string ApplicationsFolder
        {
            get => Bootstrapper.Engine.FormatString(this._selectAppsFolderHelper.Get());
            set
            {
                if (this._selectAppsFolderHelper.Set(value))
                {
                    this.OnPropertyChanged(nameof(this.ApplicationsFolder));
                }
            }
        }

        public string p4expVersion { get; set; }
        public SimpleCommand SelectAppsFolderBrowseCommand { get; }
    }
}