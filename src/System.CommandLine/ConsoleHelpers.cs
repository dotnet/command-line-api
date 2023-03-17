// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal static class ConsoleHelpers
    {
        private static bool? _colorsAreSupported;

        private static bool ColorsAreSupported
        {
            get
            {
                if (_colorsAreSupported is null)
                {
                    try
                    {
                        _colorsAreSupported = !Console.IsOutputRedirected;
                    }

                    catch (PlatformNotSupportedException)
                    {
                        _colorsAreSupported = false;
                    }
                }

                return _colorsAreSupported.Value;
            }
        }

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