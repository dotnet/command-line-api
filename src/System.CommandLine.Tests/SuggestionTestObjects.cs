// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Suggestions;
using System.CommandLine.Parsing;

namespace System.CommandLine.Tests
{
    internal class SuggestionTestObjectPlain
            : ISuggestionType<SuggestionTestObjectPlain>
    {
        public string Suggestion = "";
        public SuggestionTestObjectPlain() { }
        public SuggestionTestObjectPlain Build(ParseResult p, string s)
        {
            Suggestion = s;
            return this;
        }
        public int CompareTo(SuggestionTestObjectPlain other)
            => String.Compare(Suggestion, other.Suggestion, StringComparison.OrdinalIgnoreCase);
        public bool DoesTextMatch(string textToMatch) =>
            Suggestion.IndexOf(textToMatch, StringComparison.OrdinalIgnoreCase) >= 0;
        public int CompareToWithTextToMatch(SuggestionTestObjectPlain other, string textToMatch)
            => Suggestion.IndexOf(textToMatch, StringComparison.OrdinalIgnoreCase)
                .CompareTo(other.Suggestion.IndexOf(textToMatch, StringComparison.OrdinalIgnoreCase));
        public bool Equals(SuggestionTestObjectPlain other) => Equals(this, other);
        public bool Equals(SuggestionTestObjectPlain x, SuggestionTestObjectPlain y) => x.Suggestion.Equals(y.Suggestion);
        public int GetHashCode(SuggestionTestObjectPlain obj) => obj.Suggestion.GetHashCode();
    }

    internal class SuggestionTestObjectReverseOrder
            : ISuggestionType<SuggestionTestObjectReverseOrder>
    {
        public string Suggestion = "";
        public SuggestionTestObjectReverseOrder() { }
        public SuggestionTestObjectReverseOrder Build(ParseResult p, string s)
        {
            Suggestion = s;
            return this;
        }
        public int CompareTo(SuggestionTestObjectReverseOrder other)
            => -1 * String.Compare(Suggestion, other.Suggestion, StringComparison.OrdinalIgnoreCase);
        public bool DoesTextMatch(string textToMatch) =>
            Suggestion.IndexOf(textToMatch, StringComparison.OrdinalIgnoreCase) >= 0;
        public int CompareToWithTextToMatch(SuggestionTestObjectReverseOrder other, string textToMatch)
            => -1 * Suggestion.IndexOf(textToMatch, StringComparison.OrdinalIgnoreCase)
                .CompareTo(other.Suggestion.IndexOf(textToMatch, StringComparison.OrdinalIgnoreCase));
        public bool Equals(SuggestionTestObjectReverseOrder other) => Equals(this, other);
        public bool Equals(SuggestionTestObjectReverseOrder x, SuggestionTestObjectReverseOrder y) => x.Suggestion.Equals(y.Suggestion);
        public int GetHashCode(SuggestionTestObjectReverseOrder obj) => obj.Suggestion.GetHashCode();
    }
}
