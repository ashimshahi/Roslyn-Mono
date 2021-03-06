﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Options;

namespace Microsoft.CodeAnalysis.Host
{
    public static class PersistentStorageOptions
    {
        public const string OptionName = "FeatureManager/Persistence";

        [ExportOption]
        public static readonly Option<bool> Enabled = new Option<bool>(OptionName, "Enabled", defaultValue: true);
    }
}
