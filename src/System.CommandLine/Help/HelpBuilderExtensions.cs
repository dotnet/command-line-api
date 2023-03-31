// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine.Help
{
    public partial class HelpBuilder
    {
        /// <summary>
        /// Specifies custom help details for a specific symbol.
        /// </summary>
        /// <param name="symbol">The symbol to customize the help details for.</param>
        /// <param name="firstColumnText">A delegate to display the first help column (typically name and usage information).</param>
        /// <param name="secondColumnText">A delegate to display second help column (typically the description).</param>
        /// <param name="defaultValue">The displayed default value for the symbol.</param>
        public void CustomizeSymbol(
            CliSymbol symbol,
            string? firstColumnText = null,
            string? secondColumnText = null,
            string? defaultValue = null)
        {
            CustomizeSymbol(symbol, _ => firstColumnText, _ => secondColumnText, _ => defaultValue);
        }

        /// <summary>
        /// Writes help output for the specified command.
        /// </summary>
        public void Write(CliCommand command, TextWriter writer)
        {
            Write(new HelpContext(this, command, writer));
        }
    }
}