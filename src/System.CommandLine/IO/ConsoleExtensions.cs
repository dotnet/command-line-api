// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.IO
{
    internal static class ConsoleExtensions
    {
        internal static void SetTerminalForegroundRed(this IConsole console)
        {
            if (Platform.IsConsoleRedirectionCheckSupported &&
                !Console.IsOutputRedirected)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (Platform.IsConsoleRedirectionCheckSupported)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
        }

        internal static void ResetTerminalForegroundColor(this IConsole console)
        {
            if (Platform.IsConsoleRedirectionCheckSupported &&
                !Console.IsOutputRedirected)
            {
                Console.ResetColor();
            }
            else if (Platform.IsConsoleRedirectionCheckSupported)
            {
                Console.ResetColor();
            }
        }
    }
}