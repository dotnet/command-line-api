// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Runtime.InteropServices;

namespace System.CommandLine.Suggest
{
    public static class DotnetProfileDirectory
    {
        private const string DotnetHomeVariableName = "DOTNET_CLI_HOME";
        private const string DotnetProfileDirectoryName = ".dotnet";

        private static string PlatformHomeVariableName =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "USERPROFILE" : "HOME";

        public static bool TryGet(out string dotnetProfileDirectory)
        {
            dotnetProfileDirectory = null;
            var home = Environment.GetEnvironmentVariable(DotnetHomeVariableName);
            if (string.IsNullOrEmpty(home))
            {
                home = Environment.GetEnvironmentVariable(PlatformHomeVariableName);
                if (string.IsNullOrEmpty(home))
                {
                    return false;
                }
            }

            dotnetProfileDirectory = Path.Combine(home, DotnetProfileDirectoryName);
            return true;
        }
    }
}
