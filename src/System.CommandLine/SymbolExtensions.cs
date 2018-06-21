// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public static class SymbolExtensions
    {
        public static object GetValueOrDefault(this Symbol symbol)
        {
            return symbol.GetValueOrDefault<object>();
        }

        public static T GetValueOrDefault<T>(this Symbol symbol)
        {
            if (symbol == null)
            {
                return default(T);
            }

            ArgumentParseResult result = symbol.Result;

            if (result is SuccessfulArgumentParseResult successfulResult)
            {
                if (!successfulResult.HasValue)
                {
                    return default(T);
                }

                object value = ((dynamic)symbol.Result).Value;

                switch (value)
                {
                    // the configuration specifies a type conversion
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

                    case null:
                        if (typeof(T) == typeof(bool))
                        {
                            // the presence of the parsed symbol is treated as true
                            return (dynamic)true;
                        }

                        return default(T);
                }

                if (result is SuccessfulArgumentParseResult success &&
                    success.HasValue)
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

            throw new InvalidOperationException(symbol.ValidationMessages.RequiredArgumentMissing(symbol));
        }

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
    }
}
