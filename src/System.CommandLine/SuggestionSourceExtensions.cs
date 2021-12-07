// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Suggestions;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for working with suggestion sources.
    /// </summary>
    public static class SuggestionSourceExtensions
    {
        /// <summary>
        /// Adds a suggestion source using a delegate.
        /// </summary>
        /// <param name="suggestionSources">The list of suggestion sources to add to.</param>
        /// <param name="suggest">The delegate to be called when calculating suggestions.</param>
        public static void Add(
            this SuggestionSourceList suggestionSources,
            Func<CompletionContext, IEnumerable<string>> suggest)
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
        
        /// <summary>
        /// Adds a suggestion source using a delegate.
        /// </summary>
        /// <param name="suggestionSources">The list of suggestion sources to add to.</param>
        /// <param name="suggest">The delegate to be called when calculating suggestions.</param>
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

        /// <summary>
        /// Adds a suggestion source using a delegate.
        /// </summary>
        /// <param name="suggestionSources">The list of suggestion sources to add to.</param>
        /// <param name="suggestions">A list of strings to be suggested.</param>
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

            suggestionSources.Add(new AnonymousSuggestionSource(_ => suggestions.Select(s => new CompletionItem(s))));
        }
    }
}
