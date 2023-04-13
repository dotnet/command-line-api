// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Parses command line input.
    /// </summary>
    public static class CliParser
    {
        /// <summary>
        /// Parses a list of arguments.
        /// </summary>
        /// <param name="command">The command to use to parse the command line input.</param>
        /// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
        /// <param name="configuration">The configuration on which the parser's grammar and behaviors are based.</param>
        /// <returns>A <see cref="ParseResult"/> providing details about the parse operation.</returns>
        public static ParseResult Parse(CliCommand command, IReadOnlyList<string> args, CliConfiguration? configuration = null)
            => Parse(command, args, null, configuration);

        /// <summary>
        /// Parses a command line string.
        /// </summary>
        /// <param name="command">The command to use to parse the command line input.</param>
        /// <param name="commandLine">The complete command line input prior to splitting and tokenization. This input is not typically available when the parser is called from <c>Program.Main</c>. It is primarily used when calculating completions via the <c>dotnet-suggest</c> tool.</param>
        /// <param name="configuration">The configuration on which the parser's grammar and behaviors are based.</param>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <returns>A <see cref="ParseResult"/> providing details about the parse operation.</returns>
        public static ParseResult Parse(CliCommand command, string commandLine, CliConfiguration? configuration = null)
            => Parse(command, SplitCommandLine(commandLine).ToArray(), commandLine, configuration);

        /// <summary>
        /// Splits a string into a sequence of strings based on whitespace and quotation marks.
        /// </summary>
        /// <param name="commandLine">A command line input string.</param>
        /// <returns>A sequence of strings.</returns>
        public static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            var memory = commandLine.AsMemory();

            var startTokenIndex = 0;

            var pos = 0;

            var seeking = Boundary.TokenStart;
            var seekingQuote = Boundary.QuoteStart;

            while (pos < memory.Length)
            {
                var c = memory.Span[pos];

                if (char.IsWhiteSpace(c))
                {
                    if (seekingQuote == Boundary.QuoteStart)
                    {
                        switch (seeking)
                        {
                            case Boundary.WordEnd:
                                yield return CurrentToken();
                                startTokenIndex = pos;
                                seeking = Boundary.TokenStart;
                                break;

                            case Boundary.TokenStart:
                                startTokenIndex = pos;
                                break;
                        }
                    }
                }
                else if (c == '\"')
                {
                    if (seeking == Boundary.TokenStart)
                    {
                        switch (seekingQuote)
                        {
                            case Boundary.QuoteEnd:
                                yield return CurrentToken();
                                startTokenIndex = pos;
                                seekingQuote = Boundary.QuoteStart;
                                break;

                            case Boundary.QuoteStart:
                                startTokenIndex = pos + 1;
                                seekingQuote = Boundary.QuoteEnd;
                                break;
                        }
                    }
                    else
                    {
                        switch (seekingQuote)
                        {
                            case Boundary.QuoteEnd:
                                seekingQuote = Boundary.QuoteStart;
                                break;

                            case Boundary.QuoteStart:
                                seekingQuote = Boundary.QuoteEnd;
                                break;
                        }
                    }
                }
                else if (seeking == Boundary.TokenStart && seekingQuote == Boundary.QuoteStart)
                {
                    seeking = Boundary.WordEnd;
                    startTokenIndex = pos;
                }

                Advance();

                if (IsAtEndOfInput())
                {
                    switch (seeking)
                    {
                        case Boundary.TokenStart:
                            break;
                        default:
                            yield return CurrentToken();
                            break;
                    }
                }
            }

            void Advance() => pos++;

            string CurrentToken()
            {
                return memory.Slice(startTokenIndex, IndexOfEndOfToken()).ToString().Replace("\"", "");
            }

            int IndexOfEndOfToken() => pos - startTokenIndex;

            bool IsAtEndOfInput() => pos == memory.Length;
        }

        private static ParseResult Parse(
            CliCommand command,
            IReadOnlyList<string> arguments,
            string? rawInput,
            CliConfiguration? configuration)
        {
            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            configuration ??= new CliConfiguration(command);

            arguments.Tokenize(
                configuration,
                inferRootCommand: rawInput is not null,
                out List<CliToken> tokens,
                out List<string>? tokenizationErrors);

            var operation = new ParseOperation(
                tokens,
                configuration,
                tokenizationErrors,
                rawInput);

            return operation.Parse();
        }

        private enum Boundary
        {
            TokenStart,
            WordEnd,
            QuoteStart,
            QuoteEnd
        }
    }
}
