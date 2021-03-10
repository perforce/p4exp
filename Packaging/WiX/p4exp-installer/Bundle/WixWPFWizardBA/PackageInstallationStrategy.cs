//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA
{
    using Common;
    using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

    public class PackageInstallationStrategy : PackageInstallationStrategyBase<PackageCombinationConfiguration>
    {
        public PackageInstallationStrategy(PackageCombinationConfiguration packageCombinationConfiguration) : base(
            PackageConfiguration.PackageList, packageCombinationConfiguration)
        {
            this.PackageCombinationConfiguration = packageCombinationConfiguration;
        }

        public PackageCombinationConfiguration PackageCombinationConfiguration { get; }

        public override FeatureState? PlanMsiFeature(LaunchAction launchAction, string packageId, string featureId)
        {
            if (packageId != PackageConfiguration.P4EXPInstallerPackageID)
            {
                return null;
            }
            else
            {
                return FeatureState.Absent;
            }
        }
    }
}