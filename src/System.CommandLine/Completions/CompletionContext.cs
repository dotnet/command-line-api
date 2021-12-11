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

    /// <summary>
    /// Provides details for getting completions when the complete text of the original command line is not available.
    /// </summary>
    public class TokenCompletionContext : CompletionContext
    {
        internal TokenCompletionContext(ParseResult parseResult) : base(parseResult, GetTextToMatch(parseResult))
        {
        }
    }

    /// <summary>
    /// Provides details for calculating completions in the context of complete, unsplit command line text.
    /// </summary>
    public class TextCompletionContext : CompletionContext
    {
        private TextCompletionContext(
            ParseResult parseResult,
            string commandLineText,
            int cursorPosition) : base(parseResult, GetTextToMatch(parseResult, cursorPosition))
        {
            CommandLineText = commandLineText;
            CursorPosition = cursorPosition;
        }

        internal TextCompletionContext(
            ParseResult parseResult, 
            string commandLineText) : this(parseResult, commandLineText, commandLineText.Length)
        {
        }

        /// <summary>
        /// The position of the cursor within the command line. 
        /// </summary>
        public int CursorPosition { get; }

        /// <summary>
        /// The complete text of the command line prior to splitting, including any additional whitespace.
        /// </summary>
        public string CommandLineText { get; }

        /// <summary>
        /// Creates a new instance of <see cref="TextCompletionContext"/> at the specified cursor position.
        /// </summary>
        /// <param name="position">The cursor position at which completions are calculated.</param>
        public TextCompletionContext AtCursorPosition(int position) =>
            new(ParseResult, CommandLineText, position);
    }
}