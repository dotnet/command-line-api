// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder UseAnsiTerminalWhenAvailable(
            this CommandLineBuilder builder)
        {
            builder.ConfigureConsole(context =>
            {
                var outputMode = OutputMode.Auto;
                var preferVirtualTerminal = true;

                if (context.ParseResult.Directives.TryGetValues("output", out var value))
                {
                    if (Enum.TryParse<OutputMode>(
                        value.FirstOrDefault(),
                        true,
                        out var mode))
                    {
                        outputMode = mode;
                    }
                }

                var console = context.Console;

                var terminal = console.GetTerminal(
                    preferVirtualTerminal: preferVirtualTerminal,
                    outputMode: outputMode);

                return terminal ?? console;
            });

            return builder;
        }
    }
}
