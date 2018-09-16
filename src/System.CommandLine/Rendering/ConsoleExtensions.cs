// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace System.CommandLine.Rendering
{
    public static class ConsoleExtensions
    {
        public static OutputMode DetectOutputMode(this IConsole console)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (console.IsOutputRedirected)
            {
                return OutputMode.File;
            }

            return console.IsVirtualTerminal()
                       ? OutputMode.Ansi
                       : OutputMode.NonAnsi;
        }

        public static void Clear(this IConsole console)
        {
            if (console.IsVirtualTerminal())
            {
                console.Out.WriteLine(Ansi.Clear.EntireScreen);
            }
            else
            {
                Console.Clear();
            }
        }
    }
}
