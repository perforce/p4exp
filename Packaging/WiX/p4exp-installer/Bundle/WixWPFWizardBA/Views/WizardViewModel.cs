//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views
{
    using System;
    using System.Collections.Generic; // IEnumerable
    using System.ComponentModel;
    using System.Runtime.InteropServices; // DllImport

    using System.Windows;
    using System.Windows.Threading;
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
    using Microsoft.Deployment.WindowsInstaller; // ProductInstallation
    using Pages.FinishErrorPage;
    using Pages.FinishPage;
    using Pages.PlanPage;
    using Pages.ProgressPage;
    using Pages.SelectApplicationsPage;
    using Pages.ClientConfigurationPage;
    using Pages.ReadyToInstallPage;
    using Pages.MaintenancePage;
    using Pages.UninstallPage;
    using Pages.UpgradePage;
    using Pages.WarningPage;
    using System.IO; // Directory
    using System.Diagnostics; // Process

    public sealed class WizardViewModel : BootstrapperManager, INotifyPropertyChanged
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        public bool g_bMSI64bit = false;

        // TODO .exe should return this if 64-bit app trying to install on 32-bit OS.
        const int ERROR_INSTALL_PLATFORM_UNSUPPORTED = 1633; // Windows error code defined rather than import.

        private PageType _currentPageType = PageType.None;

        private UIElement _currentPageView;

        const string p4expUpgradeCode = "{A91EE5E2-5D91-483B-9147-278F50EB7B87}";

        enum Platform
        {
            x86,
            x64,
            none
        };

        Platform installedP4EXPPlatform = Platform.none;

        const string p4vUpgradeCode = "{70A9FDC7-885B-4D6D-BAFD-CB2D27AB2963}";

        // New minimum version: 2019.2.188.3366
        // Web site download page would show version in format 2019.2/1883366.
        // Apps and Features shows version in format 192.188.3366.
        Version p4vMinimumVersion = new Version("192.188.3366"); // Same version in format yyyy.a/bbbbbbb must be in error string FinishErrorPage_ErrorP4VVersion.

        public WizardViewModel(WixBootstrapper bootstrapper) : base(bootstrapper)
        {
            this.PackageCombinationConfiguration = new PackageCombinationConfiguration(bootstrapper);
            this.PackageInstallationStrategy = new PackageInstallationStrategy(this.PackageCombinationConfiguration);
        }

        public PackageCombinationConfiguration PackageCombinationConfiguration { get; }

        public PageType CurrentPageType
        {
            get => this._currentPageType;
            set
            {
                this._currentPageType = value;
                this.OnPropertyChanged(nameof(this.CurrentPageType));
            }
        }

        public PageType NextPageType
        {
            get
            {
                if (this.GetNextPageOrDefault(this.CurrentPageType) != null)
                {
                    return (PageType) this.GetNextPageOrDefault(this.CurrentPageType);
                }

                return PageType.None;
            }
        }

        public PageType PreviousPageType
        {
            get
            {
                if (this.GetPreviousPageOrDefault(this.CurrentPageType) != null)
                {
                    return (PageType) this.GetPreviousPageOrDefault(this.CurrentPageType);
                }

                return PageType.None;
            }
        }

        public UIElement CurrentPageView
        {
            get => this._currentPageView;
            set
            {
                this._currentPageView = value;
                this.OnPropertyChanged(nameof(this.CurrentPageView));
            }
        }


        public override IPackageInstallationStrategy PackageInstallationStrategy { get; }

        protected override void BeginUpdate()
        {
            this.GoToPage(PageType.BootstrapperUpdateCheckPage);
        }

        protected override void OnBootstrapperShouldGoToFirstPage()
        {
            this.GoToFirstPage();
        }

        protected override void ApplyBegin()
        {
            this.CurrentPageType = PageType.ProgressPage;

            this.Log(LogLevel.Standard, $"Building progress page view with {this.LaunchAction}");

            FrameworkElement view = null;
            view = new ProgressPageView(this);
            this.CurrentPageView = view;
        }

        protected override void PlanBegin()
        {
            this.GoToPage(PageType.PlanPage);
        }

        private PageType? GetNextPageOrDefault(PageType currentPageType)
        {
            switch (currentPageType)
            {
                case PageType.MaintenancePage:
                    switch (this.LaunchAction)
                    {
                        case LaunchAction.Uninstall:
                            return PageType.UninstallPage;
                        case LaunchAction.Repair:
                            return PageType.PlanPage;
                        case LaunchAction.Unknown:
                            return null;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(this.LaunchAction), this.LaunchAction, null);
                    }
                case PageType.SelectApplicationsPage:
                    return PageType.ClientConfigurationPage;
                case PageType.ClientConfigurationPage:
                    return PageType.ReadyToInstallPage;
                case PageType.ReadyToInstallPage:
                    return PageType.PlanPage;
                case PageType.UninstallPage:
                    return PageType.PlanPage;
                case PageType.UpgradePage:
                    return PageType.PlanPage;
                case PageType.WarningPage:
	                // The WarningPage is a special first page,
                    // so determine which page is "really" first
					// after the warning.
                    return GetFirstPageType();

                default:
                    return null;
            }
        }

        private PageType? GetPreviousPageOrDefault(PageType currentPageType)
        {
            switch (currentPageType)
            {
                case PageType.ClientConfigurationPage:
                    return PageType.SelectApplicationsPage;
                case PageType.ReadyToInstallPage:
                    return PageType.ClientConfigurationPage;
                case PageType.UninstallPage:
                    return PageType.MaintenancePage;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentPageType), currentPageType, null);
            }
        }

        public void GoToFirstPage()
        {
            // With this code, when download the installer with a browser, and Windows
            // Defender SmartScreen is enabled for browsers, and choose
            // "Run anyway," the installer window is not in foreground
            // BUT the icon is blinking in the Taskbar. Haven't been able to
            // find a way to make the window foreground.
            Process currentProcess = Process.GetCurrentProcess();
            IntPtr hWnd = currentProcess.MainWindowHandle;
            if (hWnd != IntPtr.Zero)
            {
                SetForegroundWindow(hWnd);
                SetFocus(hWnd);
            }

            //
            // First check prerequisites.
            //

            // C# preprocessor can't check for the value of the symbol, but only whether or not the
            // symbol is defined. We use the MSIPLATFORM64 environment variable which
            // has the *value* MSIPLATFORM64 or it's not defined. We access the environment
            // variable in the project properties > Build > General > Conditional compilation symbols
            // by just having $(MSIPLATFORM64). Note that the .csproj file will have that syntax, but
            // the properties for the project will only show MSIPLATFORM64 (the value of the environment variable).
#if MSIPLATFORM64
            g_bMSI64bit = true;
#endif
            // First check if user is trying to upgrade an x64 install with x86.

            installedP4EXPPlatform = GetInstalledP4EXPPlatform(); // May also be none.

            if (installedP4EXPPlatform == Platform.x64 &&
                !g_bMSI64bit)
            {
                this.ErrorMessage = "Message Flag: ErrorUpgradex64Withx86";
                this.GoToPage(PageType.FinishErrorPage);
                return;
            }

            // Check platform prerequisites.

            if (IsOS64bit() && !g_bMSI64bit)
            {
                this.GoToPage(PageType.WarningPage);
                return;
            }

            if (!IsOS64bit() && g_bMSI64bit)
            {
                this.ErrorMessage = "Message Flag: ErrorOSx86MSIx64"; // TODO .exe should return ERROR_INSTALL_PLATFORM_UNSUPPORTED.
                this.GoToPage(PageType.FinishErrorPage);
                return;
            }

            // Check if user is trying to upgrade an x86 with x64.
            // We check here so we give an error above if
            // installing x64 on x86 OS.

            if (installedP4EXPPlatform == Platform.x86 &&
                g_bMSI64bit)
            {
                this.ErrorMessage = "Message Flag: ErrorUpgradex86Withx64";
                this.GoToPage(PageType.FinishErrorPage);
                return;
            }

            // Now check that P4V we need is installed.
            // No need to check if P4EXP already installed (we're uninstalling or modifying).
            // In the case of upgrade, we want to check if P4V is what new installer needs.
            if (this.IsInstalled != true && !P4VVersionGood())
            {
                // Flag to show text block with error message that has hyperlink.
                this.ErrorMessage = "Message Flag: ErrorP4VVersion";
                this.GoToPage(PageType.FinishErrorPage);
                return;
            }
            this.GoToPage(GetFirstPageType());
        }

        //
        // Determine which page should be first.
        //
        PageType GetFirstPageType()
        {
            if (this.VersionStatus == VersionStatus.OlderInstalled || this.IsOldMsi == true)
            {
                return PageType.UpgradePage;
            }
            else
            {
                if (this.IsInstalled == true) // Repair (or uninstall)
                {
                    return PageType.MaintenancePage;
                }
                else // Fresh install.
                {
                    return PageType.SelectApplicationsPage;
                }
            }
        }

        public void GoToPage(PageType pageType)
        {
            if (pageType == this.CurrentPageType)
                return;

            this.Log(LogLevel.Standard, $"Switching to {pageType} from {this.CurrentPageType}");
            this.CurrentPageType = pageType;

//            System.Diagnostics.Debugger.Launch(); // DEBUG

            var view = this.CreateNewView(pageType);
            this.CurrentPageView = view;
        }

        private UIElement CreateNewView(PageType pageType)
        {
            switch (pageType)
            {
                case PageType.SelectApplicationsPage:
                    return new SelectApplicationsPageView(this);
                case PageType.ClientConfigurationPage:
                    return new ClientConfigurationPageView(this);
                case PageType.ReadyToInstallPage:
                    return new ReadyToInstallPageView(this);
                case PageType.PlanPage:
                    return new PlanPageView(this);
                case PageType.FinishPage:
                    return new FinishPageView(this);
                case PageType.FinishErrorPage:
                    return new FinishErrorPageView(this);
                case PageType.MaintenancePage:
                    return new MaintenancePageView(this);
                case PageType.UninstallPage:
                    return new UninstallPageView(this);
                case PageType.UpgradePage:
                    return new UpgradePageView(this);
                case PageType.WarningPage:
                    return new WarningPageView(this);
            }
            throw new ArgumentOutOfRangeException(nameof(pageType));
        }

        protected override void TransitionToFinishPhase()
        {
            this.GoToPage(this.Status < 0 ? PageType.FinishErrorPage : PageType.FinishPage);
        }

        public void GoToNextPage()
        {
            this.GoToPage(this.NextPageType);
        }

        protected override MessageBoxResult ShowCancelDialog()
        {
            if (this.IsInteractive)
            {
                var result = MessageBoxResult.No;
                WixBootstrapper.BootstrapperDispatcher.Invoke(DispatcherPriority.Background,
                    new Action(() =>
                    {
                        result = MessageBox.Show(WixBootstrapper.RootView,
                            string.Format(Localisation.CancelDialogBody, this.Bootstrapper.BundleName),
                            string.Format(Localisation.CancelDialogTitle, this.Bootstrapper.BundleName),
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                    }));
                return result;
            }
            return MessageBoxResult.Yes;
        }

        // TODO: Is there an easier way to do this?
        // $(env.TitleBarRelease) -> TitleBarRelease (variable in Bundle.wxs) -> this property
        public string TitleBarReleaseStr
        {
            get => Bootstrapper.Engine.StringVariables["TitleBarRelease"];
        }


        /// <summary>
        /// Check if OS is 64-bit.
        /// </summary>
        /// <returns></returns>
        public static bool IsOS64bit()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }

        public bool P4VVersionGood()
        {
            IEnumerable<ProductInstallation> installedP4Vs = ProductInstallation.GetRelatedProducts(p4vUpgradeCode);

            foreach (ProductInstallation p4v in installedP4Vs)
            {
                if (p4v.ProductVersion >= p4vMinimumVersion)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If P4EXP is installed, find out if it's x64 or x86.
        /// If not installed then return "none".
        /// </summary>
        /// <param name="upgradeCode"></param>
        /// <returns>InstalledEXPPlatform</returns>
        private static Platform GetInstalledP4EXPPlatform()
        {
            string installLocation = string.Empty;

            IEnumerable<ProductInstallation> installedP4EXP = ProductInstallation.GetRelatedProducts(p4expUpgradeCode);

            foreach (ProductInstallation p4exp in installedP4EXP)
            {
                installLocation = p4exp.InstallLocation;
            }

            if (installLocation == string.Empty)
            {
                return Platform.none;
            }
            else
            {
                if (Directory.Exists(installLocation + "P4EXP"))
                {
                    return Platform.x64;
                }
                else
                {
                    return Platform.x86;
                }
            }
        }
   }
}