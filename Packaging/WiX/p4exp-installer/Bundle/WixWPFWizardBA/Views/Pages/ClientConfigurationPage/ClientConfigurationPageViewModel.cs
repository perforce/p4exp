//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.ClientConfigurationPage
{
    using System.IO; // Path
    using System.Windows.Forms; // OpenFileDialog
    using System.Windows.Input; // ICommand
    using WixWPFWizardBA.Utilities; // WixVariableHelper
    using WixWPFWizardBA.Dialogs.DefaultPortWarning;
    using WixWPFWizardBA.Dialogs.ServerHelp;
    public class ClientConfigurationPageViewModel : PageViewModel
    {
        public string _dialogTitle;
        private readonly WixVariableHelper _clientConfigurationBrowseHelper;

        public ClientConfigurationPageViewModel(WizardViewModel wizardViewModel)
            : base(wizardViewModel)
        {
            this.CanCancel = true;
            this.CanGoToPreviousPage = true;
            this.CanGoToNextPage = true;

            // The Next button is conditional on the user acknowledging
            // that the port is the default port. But don't just go to
            // next page after acknowledging so user has a chance to
            // change the port.
            this.NextPageCommand = new SimpleCommand(_ =>
            {
                // Remove any leading and trailing spaces from port and user.
                Bootstrapper.Engine.StringVariables["P4PORT"] = Bootstrapper.Engine.StringVariables["P4PORT"].TrimStart(' ');
                Bootstrapper.Engine.StringVariables["P4PORT"] = Bootstrapper.Engine.StringVariables["P4PORT"].TrimEnd(' ');
                Bootstrapper.Engine.StringVariables["P4USER"] = Bootstrapper.Engine.StringVariables["P4USER"].TrimStart(' ');
                Bootstrapper.Engine.StringVariables["P4USER"] = Bootstrapper.Engine.StringVariables["P4USER"].TrimEnd(' ');

                if (Bootstrapper.Engine.StringVariables["DefaultPortWarned"] == "false" &&
                    Bootstrapper.Engine.StringVariables["P4PORT"] == "perforce:1666")
                {
                    var defaultPortWarningDialog = new DefaultPortWarningDialog()
                    {
                        DialogTitle = this._dialogTitle
                    };
                    // If user clicks OK then Value is true, so we know to save the selected folder.
                    defaultPortWarningDialog.ShowDialog();
                    Bootstrapper.Engine.StringVariables["DefaultPortWarned"] = "true";
                }
                else
                {
                    this.BeginNextPhase();
                }
            },  _ => true);

            // TODO Try (again?) using Bootstrapper.Engine.StringVariables to set and get P4EDITOR.
            this._clientConfigurationBrowseHelper = new WixVariableHelper(wizardViewModel.Bootstrapper, "P4EDITOR"); // TODO Search registry for it, and set in bootstrapper.
            this.PathToEditor = Bootstrapper.Engine.StringVariables["P4EDITOR"];
            this._dialogTitle = string.Format(Localisation.Wizard_WindowTitle,
                Bootstrapper.Engine.StringVariables["WixBundleName"],
                Bootstrapper.Engine.StringVariables["TitleBarRelease"]);

            this.ClientConfigServerHelpCommand = new SimpleCommand(_ =>
            {
                var serverHelpDialog = new ServerHelpDialog
                {
                    DialogTitle = this._dialogTitle
                };
                // If user clicks OK then Value is true, so we know to save the selected folder.
                serverHelpDialog.ShowDialog();
            }, _ => true);

            this.ClientConfigBrowseCommand = new SimpleCommand(_ =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    string path = this.PathToEditor;
                    openFileDialog.InitialDirectory = Path.GetDirectoryName(path);
                    openFileDialog.FileName = Path.GetFileName(path);
                    openFileDialog.CheckPathExists = true;
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.Title = WixWPFWizardBA.Localisation.ClientConfigurationDialog_BrowseDlgTitle;
                    openFileDialog.Filter = WixWPFWizardBA.Localisation.ClientConfigurationDialog_BrowseDlgFilter; // "Applications (*.exe)|*.exe"
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;

                    DialogResult result = openFileDialog.ShowDialog(null); // If don't use null, Cancel button can't be clicked.

                    if (result == DialogResult.OK)
                    {
                        this.PathToEditor = openFileDialog.FileName;
                    }
                }
            }, _ => true);
        }
        public SimpleCommand ClientConfigServerHelpCommand { get; }

        public string PathToEditor
        {
            get => this._clientConfigurationBrowseHelper.Get();
            set
            {
                if (this._clientConfigurationBrowseHelper.Set(value))
                {
                    this.OnPropertyChanged(nameof(this.PathToEditor));
                }
            }
        }

        public SimpleCommand ClientConfigBrowseCommand { get; }

        public override ICommand NextPageCommand { get; }

    }
}
