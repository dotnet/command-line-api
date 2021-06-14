// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace System.CommandLine.Suggestions
{
    /// <summary>
    /// Defines the operations needed for a custom suggestion type of TSuggestion.
    /// </summary>
    public interface ISuggestionType<TSuggestion>
        : IComparable<TSuggestion>, IEquatable<TSuggestion>, IEqualityComparer<TSuggestion>
        where TSuggestion : new()
    {
        /// <summary>
        /// Build a custom suggestion object
        /// </summary>
        /// <param name="parseResult">Result provided by the parser</param>
        /// <param name="suggestion">Suggestion string</param>
        /// <returns>Custom suggestion object of type TSuggestion</returns>
        /// <remarks>This will be used to combine non-generic suggestions,
        /// such as from IIdentifierSymbol aliases or GetSuggestions, with generic suggestions,
        /// by creating TSuggestion objects from non-generic suggestions.</remarks>
        public TSuggestion Build(ParseResult? parseResult, string suggestion);

        /// <summary>
        /// Determines if the suggestion object matches input text
        /// </summary>
        /// <param name="textToMatch">Input text to match on</param>
        public bool DoesTextMatch(string textToMatch);

        /// <summary>
        /// Given input text that needs to be matched on, compare two TSuggestion objects
        /// </summary>
        /// <param name="other">The other TSuggestion object</param>
        /// <param name="textToMatch">Input text to match on</param>
        /// <returns>A negative, zero, positive integer when this object is less than, equal to, or greater than <c>other</c></returns>
        public int CompareToWithTextToMatch(TSuggestion other, string textToMatch);
    }
}
