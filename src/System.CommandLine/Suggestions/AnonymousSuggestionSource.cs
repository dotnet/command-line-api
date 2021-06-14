// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace System.CommandLine.Suggestions
{
    internal class AnonymousSuggestionSource : ISuggestionSource
    {
        private readonly SuggestDelegate _suggest;
        private bool _enforceTextMatch;

        public bool EnforceTextMatch => _enforceTextMatch;

        public AnonymousSuggestionSource(SuggestDelegate suggest, bool enforceTextMatch = true)
        {
            _enforceTextMatch = enforceTextMatch;
            _suggest = suggest ?? throw new ArgumentNullException(nameof(suggest));
        }

        public IEnumerable<string> GetSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
        {
            return _suggest(parseResult, textToMatch);
        }
    }

    internal class AnonymousSuggestionSource<TSuggestion> : ISuggestionSource<TSuggestion>
        where TSuggestion : ISuggestionType<TSuggestion>, new()
    {
        private readonly SuggestDelegate<TSuggestion> _suggest;
        private bool _enforceTextMatch;

        public bool EnforceTextMatch => _enforceTextMatch;

        public AnonymousSuggestionSource(SuggestDelegate<TSuggestion> suggest, bool enforceTextMatch = true)
        {
            _enforceTextMatch = enforceTextMatch;
            _suggest = suggest ?? throw new ArgumentNullException(nameof(suggest));
        }

        public IEnumerable<TSuggestion> GetGenericSuggestions(ParseResult? parseResult = null, string? textToMatch = null)
        {
            return _suggest(parseResult, textToMatch);
        }
    }
}
