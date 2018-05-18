// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    internal class ArgumentSuggestionSource : ISuggestionSource
    {
        private readonly List<string> _suggestions = new List<string>();
        private readonly List<Suggest> _suggestionSources = new List<Suggest>();

        public IEnumerable<string> Suggest(
            ParseResult parseResult,
            int? position = null)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            var fixedSuggestions = _suggestions;

            var dynamicSuggestions = _suggestionSources
                .SelectMany(source => source(parseResult, position));

            return fixedSuggestions
                   .Concat(dynamicSuggestions)
                   .Distinct()
                   .OrderBy(c => c)
                   .Containing(parseResult.TextToMatch());
        }

        public void AddSuggestions(IReadOnlyCollection<string> suggestions)
        {
            if (suggestions == null)
            {
                throw new ArgumentNullException(nameof(suggestions));
            }

            _suggestions.AddRange(suggestions);
        }

        public void AddSuggestionSource(Suggest suggest)
        {
            if (suggest == null)
            {
                throw new ArgumentNullException(nameof(suggest));
            }

            _suggestionSources.Add(suggest);
        }
    }
}
