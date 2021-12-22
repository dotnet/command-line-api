// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Completions;

/// <summary>
/// Provides details for calculating completions in the context of complete, unsplit command line text.
/// </summary>
public class TextCompletionContext : CompletionContext
{
    private TextCompletionContext(
        ParseResult parseResult,
        string commandLineText,
        int cursorPosition) : base(parseResult, GetWordToComplete(parseResult, cursorPosition))
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