// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace System.CommandLine
{
    public static class OptionDefinitionExtensions
    {
        public static bool IsHidden(this SymbolDefinition symbolDefinition) =>
            string.IsNullOrWhiteSpace(symbolDefinition.Description);

        internal static IEnumerable<Symbol> AllSymbols(
            this Symbol symbol)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            yield return symbol;

            foreach (var item in symbol.Children.FlattenBreadthFirst(o => o.Children))
            {
                yield return item;
            }
        }

        public static OptionParseResult Parse(
            this OptionDefinition optionDefinition,
            string commandLine,
            IReadOnlyCollection<char> delimiters = null) =>
            new OptionParser(new ParserConfiguration(argumentDelimiters: delimiters, symbolDefinitions: new[] { optionDefinition })).Parse(commandLine);

        public static CommandParseResult Parse(
            this CommandDefinition commandDefinition,
            params string[] args) =>
            new CommandParser(commandDefinition).Parse(args);

        public static CommandParseResult Parse(
            this CommandDefinition commandDefinition,
            string commandLine,
            IReadOnlyCollection<char> delimiters = null) =>
            new CommandParser(commandDefinition).Parse(commandLine);
    }
}
