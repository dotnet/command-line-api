// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Completions
{
    /// <summary>
    /// Provides extension methods supporting command line tab completion.
    /// </summary>
    internal static class CompletionSource
    {
        private static readonly ConcurrentDictionary<Type, Func<CompletionContext, IEnumerable<CompletionItem>>> _completionSourcesByType = new();
        
        /// <summary>
        /// Gets a completion source that provides completions for a type (e.g. enum) with well-known values.
        /// </summary>
        internal static Func<CompletionContext, IEnumerable<CompletionItem>> ForType(Type type)
        {
            return _completionSourcesByType.GetOrAdd(type, t => GetCompletionSourceForType(t));
        }

        private static Func<CompletionContext, IEnumerable<CompletionItem>> GetCompletionSourceForType(Type type)
        {
            Type actualType = type.TryGetNullableType(out var nullableType) ? nullableType : type;

            if (actualType.IsEnum)
            {
                return _ => Enum.GetNames(actualType).Select(n => new CompletionItem(n));
            }
            else if (actualType == typeof(bool))
            {
                return static _ => new CompletionItem[]
                {
                    new(bool.TrueString),
                    new(bool.FalseString)
                };
            }

            return static _ => Array.Empty<CompletionItem>();
        }
    }
}