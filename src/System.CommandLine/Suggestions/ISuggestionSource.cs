// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace System.CommandLine.Suggestions
{
    /// <summary>
    /// Provides suggestions for tab completion and example values for help.
    /// </summary>
    public interface ISuggestionSource
    {
        /// <summary>
        /// Gets the suggested values for the given parse result and input text.
        /// </summary>
        /// <param name="parseResult">The result provided by the parser.</param>
        /// <param name="textToMatch">The input text to match on.</param>
        /// <returns>Suggestion strings that provide suggested values to the user.</returns>
        IEnumerable<string> GetSuggestions(ParseResult? parseResult = null, string? textToMatch = null);
    }


    /// <summary>
    /// Provides suggestions for tab completion and example values for help.
    /// </summary>
    public interface ISuggestionSource<TSuggestion>
        where TSuggestion : ISuggestionType<TSuggestion>, new()
    {
        /// <summary>
        /// Gets the suggested values for the given parse result and input text.
        /// </summary>
        /// <param name="parseResult">The result provided by the parser.</param>
        /// <param name="textToMatch">The input text to match on.</param>
        /// <returns>Suggestions of type TSuggestion that provide suggested values to the user.</returns>
        IEnumerable<TSuggestion> GetGenericSuggestions(ParseResult? parseResult = null, string? textToMatch = null);
    }
}
