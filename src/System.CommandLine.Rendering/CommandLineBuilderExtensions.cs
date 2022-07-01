// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder UseAnsiTerminalWhenAvailable(
            this CommandLineBuilder builder)
        {
            builder.AddMiddleware(context =>
            {
                var console = context.Console;

                var terminal = console.GetTerminal(
                    PreferVirtualTerminal(context.BindingContext),
                    OutputMode(context.BindingContext));

                context.Console = terminal ?? console;
            });

            return builder;
        }

        internal static bool PreferVirtualTerminal(
            this BindingContext context)
        {
            if (context.ParseResult.Directives.TryGetValues(
                "enable-vt",
                out var trueOrFalse))
            {
                if (bool.TryParse(
                    trueOrFalse.FirstOrDefault(),
                    out var pvt))
                {
                    return pvt;
                }
            }

            return true;
        }

        public static OutputMode OutputMode(this BindingContext context)
        {
            if (context.ParseResult.Directives.TryGetValues(
                    "output",
                    out var modeString) &&
                Enum.TryParse<OutputMode>(
                    modeString.FirstOrDefault(),
                    true,
                    out var mode))
            {
                return mode;
            }

            return Rendering.OutputMode.Auto;
        }
    }
}
