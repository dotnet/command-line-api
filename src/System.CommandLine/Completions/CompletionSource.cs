// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Completions
{
    /// <summary>
    /// Provides extension methods supporting <see cref="ICompletionSource"/> and command line tab completion.
    /// </summary>
    internal static class CompletionSource
    {
        private static readonly ConcurrentDictionary<Type, ICompletionSource> _completionSourcesByType = new();
        
        /// <summary>
        /// Gets a completion source that provides completions for a type (e.g. enum) with well-known values.
        /// </summary>
        internal static ICompletionSource ForType(Type type)
        {
            return _completionSourcesByType.GetOrAdd(type, t => new CompletionSourceForType(t));
        }

        internal static ICompletionSource Empty { get; } = new AnonymousCompletionSource(static _ => Array.Empty<CompletionItem>());

        private class CompletionSourceForType : ICompletionSource
        {
            private readonly Type _type;
            private ICompletionSource? _innerCompletionSource;

            public CompletionSourceForType(Type type)
            {
                _type = type;
            }

            public IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
            {
                if (_innerCompletionSource is null)
                {
                    _innerCompletionSource = CreateForType(_type);
                }

                return _innerCompletionSource.GetCompletions(context);
            }

            private static ICompletionSource CreateForType(Type type)
            {
                if (type.TryGetNullableType(out var nullableType))
                {
                    return CreateForType(nullableType);
                }

                if (type.IsEnum)
                {
                    return new AnonymousCompletionSource(_ => GetEnumNames());

                    IEnumerable<CompletionItem> GetEnumNames() => Enum.GetNames(type).Select(n => new CompletionItem(n));
                }

                if (type == typeof(bool))
                {
                    return new AnonymousCompletionSource(static  _ => new CompletionItem[]
                    {
                        new(bool.TrueString),
                        new(bool.FalseString)
                    });
                }

                return Empty;
            }
        }
    }
}