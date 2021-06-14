// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Suggestions;

namespace System.CommandLine
{
    public static class SymbolExtensions
    {
        internal static IReadOnlyList<IArgument> Arguments(this ISymbol symbol)
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

        public static IEnumerable<TSuggestion> GetGenericSuggestions<TSuggestion>(this ISymbol<TSuggestion> symbol, string textToMatch)
            where TSuggestion : ISuggestionType<TSuggestion>, new()
        {
            return symbol.GetGenericSuggestions(null, textToMatch);
        }
    }
}