// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    public class CommandLineStringSplitter
    {
        public static readonly CommandLineStringSplitter Instance = new CommandLineStringSplitter();

        private enum Boundary
        {
            TokenStart,
            WordEnd,
            QuoteEnd
        }

        public IEnumerable<string> Split(string commandLine)
        {
            var memory = commandLine.AsMemory();

            var startTokenIndex = 0;

            var pos = 0;

            var seeking = Boundary.TokenStart;
            int? skipQuoteAtIndex = null;

            while (pos < memory.Length)
            {
                var c = memory.Span[pos];

                if (char.IsWhiteSpace(c))
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

                        case Boundary.QuoteEnd:
                            break;
                    }
                }
                else if (c == '\"')
                {
                    switch (seeking)
                    {
                        case Boundary.QuoteEnd:
                            yield return CurrentToken();
                            startTokenIndex = pos;
                            seeking = Boundary.TokenStart;
                            break;

                        case Boundary.TokenStart:
                            startTokenIndex = pos + 1;
                            seeking = Boundary.QuoteEnd;
                            break;

                        case Boundary.WordEnd:
                            seeking = Boundary.QuoteEnd;
                            skipQuoteAtIndex = pos;
                            break;
                    }
                }
                else if (seeking == Boundary.TokenStart)
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
                if (skipQuoteAtIndex is null)
                {
                    return memory.Slice(
                                     startTokenIndex,
                                     IndexOfEndOfToken())
                                 .ToString();
                }
                else
                {
                    var beforeQuote = memory.Slice(
                        startTokenIndex,
                        skipQuoteAtIndex.Value - startTokenIndex);

                    var indexOfCharAfterQuote = skipQuoteAtIndex.Value + 1;

                    var afterQuote = memory.Slice(
                        indexOfCharAfterQuote,
                        pos - skipQuoteAtIndex.Value - 1);

                    skipQuoteAtIndex = null;

                    return $"{beforeQuote}{afterQuote}";
                }
            }

            int IndexOfEndOfToken() => pos - startTokenIndex;

            bool IsAtEndOfInput() => pos == memory.Length;
        }
    }
}
