// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Suggestions
{
    /// <summary>
    /// Provides extension methods supporting <see cref="ISuggestionSource"/> and command line tab completion.
    /// </summary>
    internal static class SuggestionSource
    {
        private static readonly ConcurrentDictionary<Type, ISuggestionSource> _suggestionSourcesByType = new();
        
        /// <summary>
        /// Gets a suggestion source that provides completions for a type (e.g. enum) with well-known values.
        /// </summary>
        internal static ISuggestionSource ForType(Type type)
        {
            return _suggestionSourcesByType.GetOrAdd(type, t => new SuggestionSourceForType(t));
        }

        internal static ISuggestionSource Empty { get; } = new AnonymousSuggestionSource(static _ => Array.Empty<CompletionItem>());

        private class SuggestionSourceForType : ISuggestionSource
        {
            private readonly Type _type;
            private ISuggestionSource? _innerSuggestionSource;

            public SuggestionSourceForType(Type type)
            {
                _type = type;
            }

            public IEnumerable<CompletionItem> GetSuggestions(CompletionContext context)
            {
                if (_innerSuggestionSource is null)
                {
                    _innerSuggestionSource = CreateForType(_type);
                }

                return _innerSuggestionSource.GetSuggestions(context);
            }

            private static ISuggestionSource CreateForType(Type t)
            {
                if (t.IsNullable())
                {
                    t = t.GetGenericArguments().Single();
                }

                if (t.IsEnum)
                {
                    return new AnonymousSuggestionSource(_ => GetEnumNames());

                    IEnumerable<CompletionItem> GetEnumNames() => Enum.GetNames(t).Select(n => new CompletionItem(n));
                }

                if (t == typeof(bool))
                {
                    return new AnonymousSuggestionSource(static  _ => new CompletionItem[]
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