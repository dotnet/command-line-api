// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public static class TerminalExtensions
    {
        public static void Clear(this ITerminal terminal)
        {
            if (terminal.IsVirtualTerminal)
            {
                terminal.Out.WriteLine(Ansi.Clear.EntireScreen.EscapeSequence);
            }
            else
            {
                Console.Clear();
            }
        }
    }
}
