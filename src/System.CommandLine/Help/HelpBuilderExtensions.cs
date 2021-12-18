// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine.Help
{
    /// <summary>
    /// Provides extension methods for the help builder.
    /// </summary>
    public static class HelpBuilderExtensions
    {
        /// <summary>
        /// Specifies custom help details for a specific symbol.
        /// </summary>
        /// <param name="builder">The help builder to write with.</param>
        /// <param name="symbol">The symbol to customize the help details for.</param>
        /// <param name="firstColumnText">A delegate to display the first help column (typically name and usage information).</param>
        /// <param name="secondColumnText">A delegate to display second help column (typically the description).</param>
        /// <param name="defaultValue">The displayed default value for the symbol.</param>
        public static void CustomizeSymbol(
            this HelpBuilder builder,
            Symbol symbol,
            string? firstColumnText = null,
            string? secondColumnText = null,
            string? defaultValue = null)
        {
            builder.CustomizeSymbol(symbol, _ => firstColumnText, _ => secondColumnText, _ => defaultValue);
        }

        /// <summary>
        /// Writes help output for the specified command.
        /// </summary>
        public static void Write(
            this HelpBuilder helpBuilder,
            Command command,
            TextWriter writer)
        {
            helpBuilder.Write(new HelpContext(helpBuilder, command, writer));
        }
    }
}