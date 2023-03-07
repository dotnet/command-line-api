// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.IO
{
    internal static class ConsoleExtensions
    {
        private static bool? _isConsoleRedirectionCheckSupported;

        private static bool IsConsoleRedirectionCheckSupported
        {
            get
            {
                if (_isConsoleRedirectionCheckSupported is null)
                {
                    try
                    {
                        var check = Console.IsOutputRedirected;
                        _isConsoleRedirectionCheckSupported = true;
                    }

                    catch (PlatformNotSupportedException)
                    {
                        _isConsoleRedirectionCheckSupported = false;
                    }
                }

                return _isConsoleRedirectionCheckSupported.Value;
            }
        }
        
        internal static void SetTerminalForegroundRed(this IConsole console)
        {
            if (IsConsoleRedirectionCheckSupported &&
                !Console.IsOutputRedirected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (IsConsoleRedirectionCheckSupported)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
        }

        internal static void ResetTerminalForegroundColor(this IConsole console)
        {
            if (IsConsoleRedirectionCheckSupported &&
                !Console.IsOutputRedirected)
            {
                Console.ResetColor();
            }
            else if (IsConsoleRedirectionCheckSupported)
            {
                Console.ResetColor();
            }
        }
    }
}