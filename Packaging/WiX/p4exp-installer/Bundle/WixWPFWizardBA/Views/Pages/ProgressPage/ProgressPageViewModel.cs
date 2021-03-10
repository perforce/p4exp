//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Views.Pages.ProgressPage
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Threading;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
    using Microsoft.Win32; // RegistryKey (.Net 4.5 to get OpenBaseKey)

    public class ProgressPageViewModel : PageViewModel
    {
        private int _progress;

        public ProgressPageViewModel(WizardViewModel wizardViewModel)
            : base(wizardViewModel)
        {
            this.InitEvents();

            this.CanCancel = true;
            this.CanGoToPreviousPage = false;
            this.CanGoToNextPage = false;
        }

        public int Progress
        {
            get => this._progress;
            set
            {
                if (this._progress != value)
                {
                    this._progress = value;
                    this.OnPropertyChanged(nameof(this.Progress));
                }
            }
        }

        public ObservableCollection<ProgressEntry> ProgressParts { get; } = new ObservableCollection<ProgressEntry>();


        private void InitEvents()
        {
            this.Bootstrapper.CacheAcquireProgress += this.Bootstrapper_CacheAcquireProgress;
            this.Bootstrapper.CacheAcquireBegin += this.Bootstrapper_CacheAcquireBegin;
            this.Bootstrapper.CacheAcquireComplete += this.Bootstrapper_CacheAcquireComplete;
            this.Bootstrapper.ExecutePackageBegin += this.Bootstrapper_ExecuteBegin;
            this.Bootstrapper.ExecutePackageComplete += this.Bootstrapper_ExecuteComplete;
            this.Bootstrapper.ExecuteMsiMessage += this.Bootstrapper_ExecuteMsiMessage;
            this.Bootstrapper.ExecuteProgress += this.Bootstrapper_ExecuteProgress;
            this.Bootstrapper.Progress += this.Bootstrapper_Progress;
        }

        private void Bootstrapper_ExecuteComplete(object sender, ExecutePackageCompleteEventArgs e)
        {
            WixBootstrapper.BootstrapperDispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    var toRemove = this.GetProgressEntryWithoutAdding(e.PackageId, ActionType.Execute);
                    if (toRemove == null)
                        return;
                    this.ProgressParts.Remove(toRemove);
                }));
            this.HandleCancellation(e);
        }

        private void Bootstrapper_Progress(object sender, ProgressEventArgs e)
        {
            this.Progress = e.OverallPercentage;
            this.HandleCancellation(e);
        }

        private void Bootstrapper_ExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            if (this.WizardViewModel.IsVisible)
            {
                WixBootstrapper.BootstrapperDispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {
                        var entry = this.GetProgressEntryWithoutAdding(e.PackageId, ActionType.Execute);
                        if (entry != null)
                        {
                            entry.Progress = e.OverallPercentage;
                        }
                    }));
            }
            this.HandleCancellation(e);
        }

        private void Bootstrapper_ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            /* TODO Find way to display MSI progress messages.

                        var progressEntry =
                            this.GetProgressEntryWithoutAdding("P4EXP", ActionType.Execute);
                        progressEntry.Description = e.Data.ToList().ToString();
                        this.ProgressParts.Add(progressEntry);

            // This didn't work: e.Data.ToList().ToString()

            this.Bootstrapper.Engine.Log(LogLevel.Standard, "ExecuteMsiMessage e.Message: " + e.Message);
            */
            this.HandleCancellation(e);
        }

        private void Bootstrapper_ExecuteBegin(object sender, ExecutePackageBeginEventArgs e)
        {
            this.Bootstrapper.Engine.Log(LogLevel.Standard, $"Entering {nameof(this.Bootstrapper_ExecuteBegin)}");
            if (this.WizardViewModel.IsVisible)
            {
                WixBootstrapper.BootstrapperDispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() => { this.AddProgressEntry(e.PackageId, ActionType.Execute); }));
            }
            this.HandleCancellation(e);
        }

        private void HandleCancellation(ResultEventArgs e)
        {
            if (!this.WizardViewModel.ShouldCancel)
                return;
            e.Result = Result.Cancel;
        }


        private ProgressEntry AddProgressEntry(string packageId, ActionType actionType)
        {
            var progressEntry =
                this.GetProgressEntryWithoutAdding(packageId, actionType);

            string description;
            var packageName = this.GetNameFromId(packageId);
            switch (actionType)
            {
                case ActionType.Execute:
                    description = string.Format(Localisation.ProgressPage_ProgressExecuteEntryText,
                        packageName);
                    break;
                case ActionType.Copy:
                    description = string.Format(Localisation.ProgressPage_ProgressCopyEntryText,
                        packageName);
                    break;
                case ActionType.Download:
                    description = string.Format(Localisation.ProgressPage_ProgressDownloadEntryText,
                        packageName);
                    break;
                case ActionType.Extract:
                    description = string.Format(Localisation.ProgressPage_ProgressExtractEntryText,
                        packageName);
                    break;
                case ActionType.Caching:
                    // Should never happen, but we'll provide a value just in case.
                    description = string.Format(Localisation.ProgressPage_ProgressCachingEntryText,
                        packageName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }

            //
            // Add the operation (Installing, Updating, etc.) to the description
            // so it's on the same line.
            //

            if ((this.WizardViewModel.LaunchAction == LaunchAction.Install) &&
                ((this.WizardViewModel.VersionStatus == Common.VersionStatus.None) ||
                 (this.WizardViewModel.VersionStatus == Common.VersionStatus.NewerAlreadyInstalled) ||
                 (this.WizardViewModel.VersionStatus == Common.VersionStatus.Current)))
            {
                description = string.Format(Localisation.ProgressPage_InstallingTextMain, description);
            }

            if (this.WizardViewModel.LaunchAction == LaunchAction.Modify)
            {
                description = string.Format(Localisation.ProgressPage_InstallingComponentsTextMain, description);
            }

            if (this.WizardViewModel.LaunchAction == LaunchAction.Repair)
            {
                description = string.Format(Localisation.ProgressPage_RepairingTextMain, description);
            }

            if (this.WizardViewModel.LaunchAction == LaunchAction.Uninstall)
            {
                description = string.Format(Localisation.ProgressPage_UninstallingTextMain, description);
            }

            if ((this.WizardViewModel.LaunchAction == LaunchAction.Install) &&
                (this.WizardViewModel.VersionStatus == Common.VersionStatus.OlderInstalled))
            {
                description = string.Format(Localisation.ProgressPage_UpgradingTextMain, description);
            }

            // Set progress entry.

            if (progressEntry == null)
            {
                progressEntry = new ProgressEntry
                {
                    Description = description,
                    PackageId = packageId,
                    Progress = 0,
                    ActionType = actionType
                };
                this.Bootstrapper.Engine.Log(LogLevel.Debug, $"Adding progress entry {packageId} {actionType}");
                this.ProgressParts.Add(progressEntry);
            }
            progressEntry.ActionType = actionType;
            progressEntry.Description = description;
            return progressEntry;
        }

        private ProgressEntry GetProgressEntryWithoutAdding(string packageId, ActionType actionType)
        {
            return this.ProgressParts.SingleOrDefault(x => x.PackageId == packageId
                                                           && (x.ActionType & actionType) != 0);
        }

        private void Bootstrapper_CacheAcquireComplete(object sender, CacheAcquireCompleteEventArgs e)
        {
            this.Bootstrapper.Engine.Log(LogLevel.Debug, $"CacheAcquire status {e.Status:x8}");
            this.Bootstrapper.Engine.Log(LogLevel.Debug, $"Removing progress entry {e.PackageOrContainerId} (cache)");
            WixBootstrapper.BootstrapperDispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(
                    () =>
                    {
                        var toRemove = this.GetProgressEntryWithoutAdding(e.PackageOrContainerId, ActionType.Caching);
                        if (toRemove != null)
                        {
                            this.ProgressParts.Remove(toRemove);
                        }
                    }));
            this.HandleCancellation(e);
        }

        private void Bootstrapper_CacheAcquireBegin(object sender, CacheAcquireBeginEventArgs e)
        {
            if (this.WizardViewModel.IsVisible)
            {
                WixBootstrapper.BootstrapperDispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() =>
                    {
                        this.AddProgressEntry(e.PackageOrContainerId, ActionTypeUtils.ToActionType(e.Operation));
                    }));
            }
            this.HandleCancellation(e);
        }
        
        private string GetNameFromId(string id)
        {
            if (id[0] == '{')
            {
                // It's a GUID, so use it to find bundle name (most likely a bundle).
                // This happens during upgrade where the bootstrapper gets the ID for
                // the bundle, but doesn't get the name. Also, the ID isn't in the
                // list of packages.
                string regKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + id;

                try
                {
                    RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                    string displayName = root.OpenSubKey(regKey).GetValue("DisplayName").ToString();
                    if (displayName != string.Empty)
                    {
                        return displayName;
                    }
                }
                catch
                {
                    // Ignore error. Try getting name below.
                }
            }
            return this.WizardViewModel.PackageInstallationStrategy.GetPackageNameFromId(id);
        }

        private void Bootstrapper_CacheAcquireProgress(object sender,
            CacheAcquireProgressEventArgs e)
        {
            if (this.WizardViewModel.IsVisible)
            {
                WixBootstrapper.BootstrapperDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    var entry = this.GetProgressEntryWithoutAdding(e.PackageOrContainerId, ActionType.Caching);
                    if (entry != null)
                    {
                        entry.Progress = (int) (100m * e.Progress / e.Total);
                    }
                }));
            }
            this.HandleCancellation(e);
        }
    }
}