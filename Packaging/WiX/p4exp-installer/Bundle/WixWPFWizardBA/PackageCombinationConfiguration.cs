//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
    using Microsoft.Win32;
    using Utilities;

    public class PackageCombinationConfiguration : INotifyPropertyChanged
    {
        private readonly BootstrapperApplication _bootstrapper;

        public PackageCombinationConfiguration(WixBootstrapper bootstrapper)
        {
            this._bootstrapper = bootstrapper;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}