// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Microsoft.DotNet.PlatformAbstractions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class LinuxOnlyTheory : TheoryAttribute
    {
        public LinuxOnlyTheory()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                this.Skip = "This test requires Linux to run";
            }
        }
    }
}