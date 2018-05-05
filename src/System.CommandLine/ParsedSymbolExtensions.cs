// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public static class ParsedSymbolExtensions
    {
        internal static IEnumerable<ParsedSymbol> FlattenBreadthFirst(
            this IEnumerable<ParsedSymbol> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            foreach (var item in options.FlattenBreadthFirst(o => o.Children))
            {
                yield return item;
            }
        }

        public static object GetValueOrDefault(this ParsedSymbol parsedSymbol)
        {
            return parsedSymbol.GetValueOrDefault<object>();
        }

        public static T GetValueOrDefault<T>(this ParsedSymbol symbol)
        {
            if (symbol == null)
            {
                return default(T);
            }

            ArgumentParseResult result = symbol.Result;

            object value = null;

            if (result != null)
            {
                if (result.IsSuccessful)
                {
                    value = ((dynamic)symbol.Result).Value;

                    if (value is T)
                    {
                        return (dynamic)value;
                    }
                }
                else
                {
                    ThrowNoArgumentsException(symbol);
                }
            }
            else
            {
                ThrowNoArgumentsException(symbol);
            }

            return default(T);
        }

        private static void ThrowNoArgumentsException(ParsedSymbol symbol) =>
            // TODO: (GetValueOrDefault) localize
            throw new InvalidOperationException($"No valid argument was provided for option '{symbol.Token}' and it does not have a default value.");
    }
}
