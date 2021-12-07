// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Suggestions
{
    /// <summary>
    /// Supports command line completion operations.
    /// </summary>
    public class CompletionContext
    {
        internal CompletionContext(ParseResult parseResult)
        {
            ParseResult = parseResult;
            TextToMatch = parseResult.TextToMatch();
            RawInput = parseResult.RawInput;
        }

        internal CompletionContext(
            ParseResult parseResult,
            int position)
        {
            ParseResult = parseResult;
            TextToMatch = parseResult.TextToMatch(position);
            Position = position;
        }

        /// The text of the word to be completed, if any.
        public string? TextToMatch { get; }

        /// The parse result for which completions are being requested.
        public ParseResult ParseResult { get; }

        /// <summary>
        /// The complete text of the command line, if available.
        /// </summary>
        public string? RawInput { get; }

        /// <summary>
        /// The position of the cursor at which completions are requested.
        /// </summary>
        public int Position { get; }

        internal static CompletionContext Empty() => new(ParseResult.Empty());
    }
}