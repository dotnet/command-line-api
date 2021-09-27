// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace System.CommandLine.Suggestions
{
    /// <summary>
    /// Provides suggestions for command line completion.
    /// </summary>
    /// <param name="parseResult">The parse result for which completions are being requested.</param>
    /// <param name="textToMatch">The text of the word to be completed, if any.</param>
    /// <returns>A list of suggestions.</returns>
    public delegate IEnumerable<string> SuggestDelegate(ParseResult? parseResult, string? textToMatch);
}
