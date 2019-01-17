// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

            if (console is ITerminal terminal &&
                !terminal.IsOutputRedirected)
            {
                if (terminal is IRenderable renderable &&
                    renderable.OutputMode != OutputMode.Auto)
                {
                    return renderable.OutputMode;
                }

                return terminal is VirtualTerminal
                           ? OutputMode.Ansi
                           : OutputMode.NonAnsi;
            }
            else
            {
                return OutputMode.PlainText;
            }
        }
    }
}
