// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Suggestions
{
    public static class SuggestionSource
    {
        private static readonly Dictionary<Type, ISuggestionSource> _suggestionSourcesByType = new Dictionary<Type, ISuggestionSource>();

        private static readonly string[] _trueAndFalse =
        {
            bool.FalseString,
            bool.TrueString
        };

        public static ISuggestionSource ForType(Type type)
        {
            return _suggestionSourcesByType.GetOrAdd(type, t => new SuggestionSourceForType(t));
        }

        public static ISuggestionSource Empty { get; } = new AnonymousSuggestionSource((_, __) => Array.Empty<string>());

        private class SuggestionSourceForType : ISuggestionSource
        {
            private readonly Type _type;
            private ISuggestionSource? _innerSuggestionSource;

            public SuggestionSourceForType(Type type)
            {
                _type = type;
            }

            public IEnumerable<string> GetSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
            {
                if (_innerSuggestionSource is null)
                {
                    _innerSuggestionSource = CreateForType(_type);
                }

                return _innerSuggestionSource.GetSuggestions(parseResult, textToMatch);
            }

            private static ISuggestionSource CreateForType(Type t)
            {
                if (t.IsNullable())
                {
                    t = t.GetGenericArguments().Single();
                }

                if (t.IsEnum)
                {
                    var names = Enum.GetNames(t);
                    return new AnonymousSuggestionSource((_, __) => names);
                }

                if (t == typeof(bool))
                {
                    return new AnonymousSuggestionSource((_, __) => _trueAndFalse);
                }

                return Empty;
            }
        }
    }
}