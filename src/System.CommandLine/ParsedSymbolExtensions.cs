// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
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

            object value;

            if (result != null)
            {
                if (result.IsSuccessful)
                {
                    value = ((dynamic) symbol.Result).Value;

                    if (value is T)
                    {
                        return (dynamic) value;
                    }
                }
                else
                {
                    value = symbol.Symbol.ArgumentsRule.GetDefaultValue();
                }
            }
            else
            {
                value = symbol.Symbol.ArgumentsRule.GetDefaultValue();
            }

            result = ArgumentConverter.Parse<T>(value?.ToString());

            if (result.IsSuccessful)
            {
                return ((dynamic) result).Value;
            }

            return default(T);
        }
    }
}
