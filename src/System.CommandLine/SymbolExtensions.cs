// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine
{
    public static class SymbolExtensions
    {
        internal static IEnumerable<IArgument> Arguments(this ISymbol symbol)
        {
            switch (symbol)
            {
                case IOption option:
                    return new[]
                    {
                        option.Argument
                    };
                case ICommand command:
                    return command.Arguments;
                case IArgument argument:
                    return new[]
                    {
                        argument
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        public static IEnumerable<string?> GetSuggestions(this ISymbol symbol, string? textToMatch = null)
        {
            return symbol.GetSuggestions(null, textToMatch);
        }

        public static ParseResult Parse(this ISymbol symbol, string commandLine) =>
            symbol switch
            {
                Argument argument => argument.Parse(commandLine),
                Command command => command.Parse(commandLine),
                Option option => option.Parse(commandLine),
                _ => throw new ArgumentOutOfRangeException(nameof(symbol))
            };
    }
}