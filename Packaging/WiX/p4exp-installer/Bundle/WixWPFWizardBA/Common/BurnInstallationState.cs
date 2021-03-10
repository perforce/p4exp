﻿//  
// Copyright (c) Nick Guletskii and Arseniy Aseev. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the solution root for full license information.  
//
namespace WixWPFWizardBA.Common
{
    public enum BurnInstallationState
    {
        Initializing,
        Detected,
        Applying,
        Applied,
        Failed,
        DetectedNewer
    }
}