// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public static class SymbolResultExtensions
    {
        public static object GetValueOrDefault(this SymbolResult symbolResult)
        {
            return symbolResult.GetValueOrDefault<object>();
        }

        public static T GetValueOrDefault<T>(this SymbolResult symbolResult)
        {
            if (symbolResult == null)
            {
                return default;
            }

            var result = symbolResult.ArgumentResult;

            if (result is SuccessfulArgumentResult successful)
            {
                object value = successful.Value;

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

                        return default;
                }

                if (result is SuccessfulArgumentResult s)
                {
                    value = s.Value;
                }

                if (value is T t)
                {
                    return t;
                }
            }

            if (result is FailedArgumentResult failed)
            {
                throw new InvalidOperationException(failed.ErrorMessage);
            }

            throw new InvalidOperationException(symbolResult.ValidationMessages.RequiredArgumentMissing(symbolResult));
        }

        internal static IEnumerable<SymbolResult> AllSymbolResults(this SymbolResult symbolResult)
        {
            if (symbolResult == null)
            {
                throw new ArgumentNullException(nameof(symbolResult));
            }

            yield return symbolResult;

            foreach (var item in symbolResult
                                 .Children
                                 .FlattenBreadthFirst(o => o.Children))
            {
                yield return item;
            }
        }
    }
}
