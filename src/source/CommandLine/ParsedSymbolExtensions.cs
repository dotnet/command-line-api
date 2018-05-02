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

        public static object GetValueOrDefault(this ParsedSymbol parsedSymbol) => parsedSymbol.GetValueOrDefault<object>();

        public static T GetValueOrDefault<T>(this ParsedSymbol symbol)
        {
            if (symbol == null)
            {
                return default(T);
            }

            if (symbol.Result != null &&
                symbol.Result.IsSuccessful)
            {
                return ((dynamic) symbol.Result).Value;
            }

            var parseResult = ArgumentConverter.Parse<T>(symbol.Symbol.ArgumentsRule.GetDefaultValue());

            if (parseResult is SuccessfulArgumentParseResult<T> successful)
            {
                return successful.Value;
            }

            return default(T);
        }
    }
}
