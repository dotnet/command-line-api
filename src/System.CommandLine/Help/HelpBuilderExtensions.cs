﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
        /// <param name="descriptor">The name and invocation details, typically in the first help column.</param>
        /// <param name="defaultValue">The displayed default value for the symbol.</param>
        /// <param name="description">The description for the symbol.</param>
        public static void Customize(
            this HelpBuilder builder,
            ISymbol symbol,
            string? descriptor = null,
            string? defaultValue = null,
            string? description = null)
        {
            builder.Customize(symbol, _ => descriptor, _ => defaultValue, _ => description);
        }

        /// <param name="symbol">The symbol to customize the help details for.</param>
        /// <param name="descriptor">A delegate to display the name and invocation details, typically in the first help column.</param>
        /// <param name="defaultValue">A delegate to display the default value for the symbol.</param>
        /// <param name="description">A delegate to display the description for the symbol.</param>
        /// /// <param name="builder">The help builder to write with.</param>
        public static void Customize(
            this HelpBuilder builder,
            ISymbol symbol,
            Func<ParseResult?, string?>? descriptor = null,
            Func<ParseResult?, string?>? defaultValue = null,
            Func<ParseResult?, string?>? description = null)
        {
            builder.Customize(symbol, descriptor, defaultValue, description);
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
    }
}