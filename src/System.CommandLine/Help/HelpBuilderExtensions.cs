// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.IO;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Provides extension methods for the help builder.
    /// </summary>
    public static class HelpBuilderExtensions
    {
        /// <param name="builder">The help builder to write with.</param>
        /// <param name="symbol">The symbol to customize the help details for.</param>
        /// <param name="firstColumnText">A delegate to display the first help column (typically name and usage information).</param>
        /// <param name="secondColumnText">A delegate to display second help column (typically the description).</param>
        /// <param name="defaultValue">The displayed default value for the symbol.</param>
        public static void Customize(
            this HelpBuilder builder,
            ISymbol symbol,
            string? firstColumnText = null,
            string? secondColumnText = null,
            string? defaultValue = null)
        {
            builder.Customize(symbol, _ => firstColumnText, _ => secondColumnText, _ => defaultValue);
        }

        /// <param name="symbol">The symbol to customize the help details for.</param>
        /// <param name="firstColumnText">A delegate to display the first help column (typically name and usage information).</param>
        /// <param name="secondColumnText">A delegate to display second help column (typically the description).</param>
        /// <param name="defaultValue">A delegate to display the default value for the symbol.</param>
        /// /// <param name="builder">The help builder to write with.</param>
        public static void Customize(
            this HelpBuilder builder,
            ISymbol symbol,
            Func<ParseResult?, string?>? firstColumnText = null,
            Func<ParseResult?, string?>? secondColumnText = null,
            Func<ParseResult?, string?>? defaultValue = null)
        {
            builder.Customize(symbol, firstColumnText, secondColumnText, defaultValue);
        }

        /// <summary>
        /// Writes help output for the specified command.
        /// </summary>
        /// <param name="builder">The help builder to write with.</param>
        /// <param name="command">The command for which to write help output.</param>
        /// <param name="writer">The writer to write output to.</param>
        public static void Write(
            this IHelpBuilder builder,
            ICommand command,
            TextWriter writer) =>
            builder.Write(command, writer, ParseResult.Empty());

        public static string FormatHelp(this IHelpBuilder helpBuilder, ICommand command, ParseResult parseResult)
        {
            using var output = new StringWriter();
            helpBuilder.Write(command, output, parseResult);
            return output.ToString();
        }
    }
}