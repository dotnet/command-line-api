// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Completions
{
    /// <summary>
    /// Supports command line completion operations.
    /// </summary>
    public abstract class CompletionContext
    {
        internal CompletionContext(ParseResult parseResult, string textToMatch)
        {
            ParseResult = parseResult;
            TextToMatch = textToMatch;
        }

        /// The text of the word to be completed, if any.
        public string TextToMatch { get; }

        /// The parse result for which completions are being requested.
        public ParseResult ParseResult { get; }

        internal static CompletionContext Empty() => new TokenCompletionContext(ParseResult.Empty());

        /// <summary>
        /// Gets the text to be matched for completion, which can be used to filter a list of completions.
        /// </summary>
        /// <param name="parseResult">A parse result.</param>
        /// <param name="position">The position within the raw input, if available, at which to provide completions.</param>
        /// <returns>A string containing the user-entered text to be matched for completions.</returns>
        internal static string GetTextToMatch(
            ParseResult parseResult,
            int? position = null)
        {
            Token? lastToken = parseResult.Tokens.LastOrDefault(t => t.Type != TokenType.Directive);

            string? textToMatch = null;
            string? rawInput = parseResult.CommandLineText;

            if (rawInput is not null)
            {
                if (position is not null)
                {
                    if (position > rawInput.Length)
                    {
                        rawInput += ' ';
                        position = Math.Min(rawInput.Length, position.Value);
                    }
                }
                else
                {
                    position = rawInput.Length;
                }
            }
            else if (lastToken?.Value is not null)
            {
                position = null;
                textToMatch = lastToken.Value;
            }

            if (string.IsNullOrWhiteSpace(rawInput))
            {
                if (parseResult.UnmatchedTokens.Count > 0 ||
                    lastToken?.Type == TokenType.Argument)
                {
                    return textToMatch ?? "";
                }
            }
            else
            {
                var textBeforeCursor = rawInput!.Substring(0, position!.Value);

                var textAfterCursor = rawInput.Substring(position.Value);

                return textBeforeCursor.Split(' ').LastOrDefault() +
                       textAfterCursor.Split(' ').FirstOrDefault();
            }

            return "";
        }
    }

    // FIX: (CompletionContext) these need better names

    public class TokenCompletionContext : CompletionContext
    {
        internal TokenCompletionContext(ParseResult parseResult) : base(parseResult, GetTextToMatch(parseResult))
        {
        }
    }

    public class TextCompletionContext : CompletionContext
    {
        private TextCompletionContext(
            ParseResult parseResult,
            string commandLineText,
            int position) : base(parseResult, GetTextToMatch(parseResult, position))
        {
            CommandLineText = commandLineText;
            Position = position;
        }

        internal TextCompletionContext(ParseResult parseResult, string commandLineText) : this(parseResult, commandLineText, commandLineText.Length)
        {
        }

        public int Position { get; }

        public string CommandLineText { get; }

        public TextCompletionContext AtPosition(int position) =>
            new(ParseResult, CommandLineText, position);
    }
}