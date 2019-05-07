// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine
{
    public static class SuggestionSource
    {
        private static readonly ConcurrentDictionary<Type, ISuggestionSource> _suggestionSourcesByType = new ConcurrentDictionary<Type, ISuggestionSource>();

        private static readonly string[] _trueAndFalse =
        {
            bool.FalseString,
            bool.TrueString
        };

        public static ISuggestionSource ForType(Type type)
        {
            return _suggestionSourcesByType.GetOrAdd(type ?? typeof(object), CreateForType);

            ISuggestionSource CreateForType(Type t)
            {
                if (t.IsNullable())
                {
                    t = t.GetGenericArguments().Single();
                }

                if (t.IsEnum)
                {
                    var names = Enum.GetNames(t);
                    return new AnonymousSuggestionSource(_ => names);
                }

                if (t == typeof(bool))
                {
                    return new AnonymousSuggestionSource(_ => _trueAndFalse);
                }

                return Empty;
            }
        }

        public static ISuggestionSource Empty { get; } = new AnonymousSuggestionSource(_ => Array.Empty<string>());
    }
}
