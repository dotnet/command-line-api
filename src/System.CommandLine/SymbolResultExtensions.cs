// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public static class SymbolResultExtensions
    {
        public static ArgumentResult GetValueAs(
            this SymbolResult symbolResult,
            Type type)
        {
            if (symbolResult == null)
            {
                return ArgumentResult.None;
            }

            if (type == null)
            {
                type = typeof(object);
            }

            var result = symbolResult.ArgumentResult;

            if (!(result is SuccessfulArgumentResult successful))
            {
                return result;
            }

            if (type.IsInstanceOfType(successful.Value))
            {
                return result;
            }

            return ArgumentConverter.Parse(type, successful.Value);
        }

        public static object GetValueOrDefault(this SymbolResult symbolResult)
        {
            return symbolResult.GetValueOrDefault<object>();
        }

        public static T GetValueOrDefault<T>(this SymbolResult symbolResult)
        {
            ArgumentResult result = symbolResult.GetValueAs(typeof(T));

            switch (result)
            {
                case SuccessfulArgumentResult successful:
                    return (T)successful.Value;
                case FailedArgumentResult failed:
                    throw new InvalidOperationException(failed.ErrorMessage);
                case NoArgumentResult _:
                default:
                    return default;
            }
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
