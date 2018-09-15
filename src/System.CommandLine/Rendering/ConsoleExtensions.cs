// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace System.CommandLine.Rendering
{
    public static class ConsoleExtensions
    {
        public static OutputMode DetectOutputMode(this IConsole console)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
            
            if (console.IsOutputRedirected)
            {
                return OutputMode.File;
            }

            var terminalName = System.Environment.GetEnvironmentVariable("TERM");
            return !string.IsNullOrEmpty(terminalName) 
                    && terminalName.StartsWith("xterm", StringComparison.OrdinalIgnoreCase) 
                        ? OutputMode.Ansi 
                        : OutputMode.NonAnsi;
        }
    }
}