// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Xunit;

namespace System.CommandLine.Tests.Utility
{
    public class LinuxOnlyTheory : TheoryAttribute
    {
        public LinuxOnlyTheory()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Skip = "This test requires Linux to run";
            }
        }
    }
}