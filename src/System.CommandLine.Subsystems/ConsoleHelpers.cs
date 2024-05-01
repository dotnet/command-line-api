// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace System.CommandLine
{
    internal static class ConsoleHelpers
    {
        private static readonly bool ColorsAreSupported = GetColorsAreSupported();

        private static bool GetColorsAreSupported()
#if NET7_0_OR_GREATER
            => !(OperatingSystem.IsBrowser() || OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || OperatingSystem.IsTvOS())
#else
            => !(RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"))
                    || RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"))
                    || RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"))
                    || RuntimeInformation.IsOSPlatform(OSPlatform.Create("TVOS")))
#endif
            && !Console.IsOutputRedirected;

        internal static void SetTerminalForegroundRed()
        {
            if (ColorsAreSupported)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
        }

        internal static void ResetTerminalForegroundColor()
        {
            if (ColorsAreSupported)
            {
                Console.ResetColor();
            }
        }
    }
}