// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Suggestions;

namespace System.CommandLine
{
    public static class SuggestionSourceExtensions
    {
        public static void Add(
            this SuggestionSourceList suggestionSources,
            SuggestDelegate suggest)
        {
            if (suggestionSources is null)
            {
                throw new ArgumentNullException(nameof(suggestionSources));
            }

            if (suggest is null)
            {
                throw new ArgumentNullException(nameof(suggest));
            }

            suggestionSources.Add(new AnonymousSuggestionSource(suggest));
        }

        public static void Add(
            this SuggestionSourceList suggestionSources,
            params string[] suggestions)
        {
            if (suggestionSources is null)
            {
                throw new ArgumentNullException(nameof(suggestionSources));
            }

            if (suggestions is null)
            {
                throw new ArgumentNullException(nameof(suggestions));
            }

            suggestionSources.Add(new AnonymousSuggestionSource((_, __) => suggestions));
        }
    }
}
