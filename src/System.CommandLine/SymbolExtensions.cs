// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public static class SymbolExtensions
    {
        internal static IEnumerable<string> ChildSymbolAliases(this ISymbol symbol) =>
            symbol.Children
                  .Where(s => !s.IsHidden)
                  .SelectMany(s => s.RawAliases);

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
    }
}