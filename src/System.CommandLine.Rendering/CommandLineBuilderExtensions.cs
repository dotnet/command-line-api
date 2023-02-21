// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Rendering
{
    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder UseAnsiTerminalWhenAvailable(
            this CommandLineBuilder builder)
        {
            Directive enableVtDirective = new ("enable-vt");
            Directive outputDirective = new ("output");
            builder.Directives.Add(enableVtDirective);
            builder.Directives.Add(outputDirective);

            builder.AddMiddleware(context =>
            {
                var console = context.Console;

                var terminal = console.GetTerminal(
                    PreferVirtualTerminal(context.ParseResult, enableVtDirective),
                    OutputMode(context.ParseResult, outputDirective));

                context.Console = terminal ?? console;
            });

            return builder;
        }

        private static bool PreferVirtualTerminal(ParseResult parseResult, Directive enableVtDirective)
        {
            if (parseResult.FindResultFor(enableVtDirective) is DirectiveResult result)
            {
                string trueOrFalse = result.Value;

                if (bool.TryParse(trueOrFalse, out var pvt))
                {
                    return pvt;
                }
            }

            return true;
        }

        private static OutputMode OutputMode(ParseResult parseResult, Directive outputDirective)
        {
            if (parseResult.FindResultFor(outputDirective) is DirectiveResult result
                && Enum.TryParse<OutputMode>(result.Value, true, out var mode))
            {
                return mode;
            }

            return Rendering.OutputMode.Auto;
        }
    }
}
