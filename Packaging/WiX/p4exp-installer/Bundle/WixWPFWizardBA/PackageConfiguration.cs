//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA
{
    using System.Collections.Generic;
    using Common;

    public static class PackageConfiguration
    {
        public const string Netfx45RedistPackageId = "NetFx45FullRedistExe";
        public const string P4EXPInstallerPackageID = "P4EXPPackageID";
        public static IList<Package<PackageCombinationConfiguration>> PackageList { get; } =
            new List<Package<PackageCombinationConfiguration>>
            {
                new Package<PackageCombinationConfiguration>
                {
                    PackageId = P4EXPInstallerPackageID,
                    DisplayName = Localisation.PackageConfiguration_PackageList_P4EXP_msi,
                    Architectures = Architecture.X86 | Architecture.X64,
                },

                new Package<PackageCombinationConfiguration>
                {
                    PackageId = Netfx45RedistPackageId,
                    DisplayName = Localisation.Package_Netfx45Redist,
                    Architectures = Architecture.X86 | Architecture.X64,
                    IsRemovable = false
                }
            };
    }
}