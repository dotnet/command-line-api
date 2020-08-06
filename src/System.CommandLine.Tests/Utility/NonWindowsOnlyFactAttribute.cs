// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.PlatformAbstractions;
using Xunit;

namespace System.CommandLine.Tests.Utility
{
    public class NonWindowsOnlyFactAttribute : FactAttribute
    {
        public NonWindowsOnlyFactAttribute()
        {
            if (RuntimeEnvironment.OperatingSystemPlatform == Platform.Windows)
            {
                Skip = "This test requires non-Windows to run";
            }
        }
    }
}