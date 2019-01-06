// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace System.CommandLine.Rendering
{
    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder UseAnsiTerminalWhenAvailable(
            this CommandLineBuilder builder)
        {
            builder.ConfigureConsole(context => GetTerminal(context.Console, true));

            return builder;
        }

        public static ITerminal GetTerminal(
            this IConsole console,
            bool preferVirtualTerminal = true,
            OutputMode outputMode = OutputMode.Auto)
        {
            if (console == null)
            {
                throw new ArgumentNullException(nameof(console));
            }

            if (console is ITerminal terminal)
            {
                return terminal;
            }
            else if (preferVirtualTerminal &&
                     VirtualTerminalMode.TryEnable() is VirtualTerminalMode virtualTerminalMode &&
                     virtualTerminalMode.IsEnabled)
            {
                return new VirtualTerminal(
                    console,
                    virtualTerminalMode)
                       {
                           OutputMode = outputMode
                       };
            }
            else
            {
                return new SystemConsoleTerminal(console);
            }
        }
    }
}
