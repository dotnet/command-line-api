// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace System.CommandLine
{
    public static class OptionExtensions
    {
        public static bool IsHidden(this Symbol symbol) =>
            string.IsNullOrWhiteSpace(symbol.Description);

        internal static IEnumerable<ParsedSymbol> AllSymbols(
            this ParsedSymbol symbol)
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
            this Option option,
            string commandLine,
            IReadOnlyCollection<char> delimiters = null) =>
            new OptionParser(new ParserConfiguration(argumentDelimiters: delimiters, definedSymbols: new[] { option })).Parse(commandLine);

        public static CommandParseResult Parse(
            this Command command,
            params string[] args) =>
            new CommandParser(command).Parse(args);

        public static CommandParseResult Parse(
            this Command command,
            string commandLine,
            IReadOnlyCollection<char> delimiters = null) =>
            new CommandParser(command).Parse(commandLine);
    }
}
