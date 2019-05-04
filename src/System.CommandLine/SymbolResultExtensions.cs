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

        public static bool TryGetValueOrDefault(
            this SymbolResult symbolResult,
            Type type, 
            out object value)
        {
            if (symbolResult == null)
            {
                value = default;
                return false;
            }

            if (type == null)
            {
                type = typeof(object);
            }

            var result = symbolResult.ArgumentResult;

            if (result is SuccessfulArgumentResult successful)
            {
                value = successful.Value;

                if (type.IsInstanceOfType(value))
                {
                    return true;
                }

                switch (value)
                {
                    // try to parse the single string argument to the requested type
                    case string argument:
                        result = ArgumentConverter.Parse(type, argument);

                        break;

                    // try to parse the multiple string arguments to the request type
                    case IReadOnlyCollection<string> arguments:
                        result = ArgumentConverter.ParseMany(type, arguments);

                        break;

                    case null:
                        if (type == typeof(bool))
                        {
                            // the presence of the parsed symbol is treated as true
                            value = true;
                            return true;
                        }

                        value = default;
                        return false;
                }

                if (result is SuccessfulArgumentResult s)
                {
                    value = s.Value;
                }

                if (type.IsInstanceOfType(value))
                {
                    return true;
                }
            }

            if (result is FailedArgumentResult failed)
            {
                throw new InvalidOperationException(failed.ErrorMessage);
            }

            value = default;
            return false;
        }

        public static T GetValueOrDefault<T>(this SymbolResult symbolResult)
        {
            if (symbolResult.TryGetValueOrDefault(typeof(T), out var value))
            {
                return (T)value;
            }

            return default;

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
