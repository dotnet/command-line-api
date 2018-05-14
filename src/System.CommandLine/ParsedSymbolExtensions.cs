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

            if (result != null &&
                result.IsSuccessful)
            {
                object value = ((dynamic)symbol.Result).Value;

                switch (value)
                {
                    // the parser configuration specifies a type conversion 
                    case T alreadyConverted:
                        return alreadyConverted;

                    // try to parse the single string argument to the requested type
                    case string argument:
                        result = ArgumentConverter.Parse<T>(argument);

                        break;

                    // try to parse the multiple string arguments to the request type
                    case IReadOnlyCollection<string> arguments:
                        result = ArgumentConverter.ParseMany<T>(arguments);

                        break;
                }

                if (result.IsSuccessful)
                {
                    value = ((dynamic)result).Value;
                }

                if (value is T t)
                {
                    return t;
                }
            }

            if (result is FailedArgumentParseResult failed)
            {
                throw new InvalidOperationException(failed.ErrorMessage);
            }

            throw new InvalidOperationException(ValidationMessages.RequiredArgumentMissingForOption(symbol.Token));
        }
    }
}
