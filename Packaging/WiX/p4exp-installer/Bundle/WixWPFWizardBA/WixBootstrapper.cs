//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA
{
    using System;
    using System.Diagnostics; // ProcessWindowStyle
    using System.Globalization;
    using System.IO; // Path
    using System.Runtime.InteropServices; // DllImport, SetLastError, CallingConvention, MarshallAs, UnmanagedType, In, Out
    using System.Threading;
    using System.Windows; // MessageBox
    using System.Windows.Markup;
    using System.Windows.Threading;
    using Microsoft.Win32; // RegistryKey, 
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper; // LogLevel
    using Views;

    public class WixBootstrapper : BootstrapperApplication
    {
        const string appToGetP4Vars = "GetP4Vars.exe";

        public static Dispatcher BootstrapperDispatcher { get; private set; }

        public static WizardWindow RootView { get; set; }

        public string BundleName => this.Engine.StringVariables["WixBundleName"];

        protected override void Run()
        {
            // On an English system, the user can change the language to Japanese, so
            // detect the user language, and not the system.
            var code = int.Parse(this.Engine.FormatString("[UserUILanguageID]"));
            var cultureInfo = CultureInfo.GetCultureInfo(code);

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
            Localisation.Culture = cultureInfo;

            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag)));
            try
            {
                this.Engine.CloseSplashScreen();

                InitializeP4Variables();

                FormatVersionForPage();

                var rebootPending = this.Engine.StringVariables["RebootPending"];
                if (!string.IsNullOrEmpty(rebootPending) && rebootPending != "0")
                {
                    if (this.Command.Display == Display.Full)
                    {
                        MessageBox.Show(
                            string.Format(Localisation.WixBootstrapper_RestartPendingDialogBody, this.BundleName),
                            string.Format(Localisation.WixBootstrapper_RestartPendingDialogTitle, this.BundleName),
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    this.Engine.Quit(3010);
                }

                this.Engine.Log(LogLevel.Verbose, "Launching Burn frontend");
                BootstrapperDispatcher = Dispatcher.CurrentDispatcher;
                AppDomain.CurrentDomain.UnhandledException +=
                    (sender, args) =>
                    {
                        this.Engine.Log(LogLevel.Error, $"Critical bootstrapper exception: {args.ExceptionObject}");
                    };
                BootstrapperDispatcher.UnhandledException +=
                    (sender, args) =>
                    {
                        this.Engine.Log(LogLevel.Error, $"Critical bootstrapper exception: {args.Exception}");
                    };

                RootView = new WizardWindow(this);
                RootView.Closed += (sender, args) => BootstrapperDispatcher.InvokeShutdown();

                this.Engine.Detect();

                //System.Diagnostics.Debugger.Launch();
                if (this.Command.Display == Display.Passive || this.Command.Display == Display.Full)
                {
                    RootView.Show();
                    Dispatcher.Run();
                }

                this.Engine.Quit(RootView.ViewModel.Status);
            }
            catch (Exception e)
            {
                this.Engine.Log(LogLevel.Error, $"Critical bootstrapper exception: {e}");
                throw e;
            }
        }

        /// <summary>
        /// Get values of P4INSTROOT, P4PORT, P4USER, and P4EDITOR from registry or use default values.
        /// Also initialize DefaultPortWarned.
        /// </summary>
        public void InitializeP4Variables()
        {
            // Path to where we are is path to GetP4Vars.exe since in Bundle.wxs we include
			// GetP4Vars.exe.
            string dirWeAreIn = AppDomain.CurrentDomain.BaseDirectory;

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; // Even if we have CreateNoWindow = true, a window briefly appears, so hide.
            process.StartInfo.FileName = Path.Combine(dirWeAreIn, appToGetP4Vars);

            try
            {
                process.Start();
                process.WaitForExit(30000); // 30000 = 30 seconds
            }
            catch (Exception e)
            {
                this.Engine.Log(LogLevel.Standard, $"Error when attempting to get Perforce variables (perhaps user cancel): {e}");
            }

            if (process.ExitCode != 0 &&
                process.ExitCode != 2) // ERROR_FILE_NOT_FOUND if value name not found, which we can ignore.
            {
                MessageBox.Show("Error reading 32-bit part of registry. ExitCode: " + process.ExitCode.ToString(), "DEBUG");
                process.StartInfo.Verb = "runas"; // Try running as administrator.
                // TODO Make above a loop.
                try
                {
                    process.Start();
                    process.WaitForExit(30000); // 30000 = 30 seconds
                }
                catch (Exception e)
                {
                    this.Engine.Log(LogLevel.Standard, $"Error when attempting to get Perforce variables (perhaps user cancel): {e}");
                }
            }

            // Make sure the strings exists. InstallDir may not be set in Bundle.wxs.
            try
            {
                // Just reference the variable to see if it exists.
                if (this.Engine.StringVariables["InstallDir"] != "")
                {
                }
            }
            catch // InstallDir doesn't exist (exception of element not found).
            {
                this.Engine.StringVariables["InstallDir"] = "";
            }
            this.Engine.StringVariables["P4EDITOR"] = "";
            this.Engine.StringVariables["P4PORT"] = "";
            this.Engine.StringVariables["P4USER"] = "";

            //MessageBox.Show("Before reading values from registry."); // DEBUG

            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Perforce");

            if (key != null) // Registry path exists.
            {
                key.Close();
                // Open as writeable so we can delete the temporary value.
                key = Registry.CurrentUser.OpenSubKey("Software\\Perforce", true);

                string p4values = key.GetValue("P4ValuesForSetup", ",,,").ToString();

                string[] values = p4values.Split(',');

                if (values[0] != "")
                {
                    this.Engine.StringVariables["InstallDir"] = values[0];
                }
                this.Engine.StringVariables["P4EDITOR"] = values[1];
                this.Engine.StringVariables["P4PORT"] = values[2];
                this.Engine.StringVariables["P4USER"] = values[3];

                if (p4values != ",,,")
                {
                    key.DeleteValue("P4ValuesForSetup");
                    string deletePerforceKey = key.GetValue("DeletePerforceKey", "default").ToString();
                    if (deletePerforceKey != "default")
                    {
                        key.DeleteValue("DeletePerforceKey");
                        key.Close();
                        key = Registry.CurrentUser.OpenSubKey("Software", true);
                        key.DeleteSubKey("Perforce");
                    }
                }
                key.Close();
            }
            else
            {
                // P4V may not be installed, so don't show this message.
                // MessageBox.Show("Couldn't open CurrentUser\\Software\\Perforce.", "DEBUG");
            }

            // Set default values if values not set.

            // InstallDir

            if (this.Engine.StringVariables["InstallDir"] == "")
            {
                this.Engine.StringVariables["InstallDir"] = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramW6432%"), "Perforce");
            }

            // P4EDITOR

            if (this.Engine.StringVariables["P4EDITOR"] == "")
            {
                string editor = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "Notepad.exe");
                if (!File.Exists(editor))
                {
                    editor = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "Write.exe");
                    if (!File.Exists(editor))
                    {
                        editor = string.Empty; // Let user browse to a program.
                    }
                }

                this.Engine.StringVariables["P4EDITOR"] = editor;
            }

            // P4PORT

            if (this.Engine.StringVariables["P4PORT"] == "")
            {
                this.Engine.StringVariables["P4PORT"] = "perforce:1666";
            }

            // P4USER

            if (this.Engine.StringVariables["P4USER"] == "")
            {
                this.Engine.StringVariables["P4USER"] = Environment.UserName;
            }
        }

        /// <summary>
        /// Convert the product version to a format to display on Select Destination page.
        /// Example:
        /// ProductVersion: 192.189.0303
        /// Formatted: 2019.2/189.0303
        /// Note: If ProductVersion starts with 255, then it's a main or dev build, so
        /// the version starts with 9999.9.
        /// </summary>
        public void FormatVersionForPage()
        {
            string formatted = String.Empty;
            string bundleVersion = this.Engine.VersionVariables["WixBundleVersion"].ToString(); // VersionVariables["BurnBundleVersionVariable"].ToString();
            string[] versionNumbers = bundleVersion.Split('.');
            if (versionNumbers[0] == "255")
            {
                formatted = "9999.9"; // Indicate it's a main/dev build.
            }
            else
            {
                formatted = "20" + versionNumbers[0].Substring(0, 2); // Example: 2019
                formatted += "." + versionNumbers[0].Substring(2); // Example: 2019.1
            }
            formatted += "/" + versionNumbers[1] + versionNumbers[2]; // Example: 2019.1/1890303
            this.Engine.StringVariables["VersionForPage"] = formatted;
        }
    }
}