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
    }
}