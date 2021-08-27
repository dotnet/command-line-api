﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Parsing
{
    public class CommandLineStringSplitter
    {
        public static readonly CommandLineStringSplitter Instance = new CommandLineStringSplitter();

        private enum Boundary
        {
            TokenStart,
            WordEnd,
            QuoteStart,
            QuoteEnd
        }

        public IEnumerable<string> Split(string commandLine)
        {
            var memory = commandLine.AsMemory();

            var startTokenIndex = 0;

            var pos = 0;

            var seeking = Boundary.TokenStart;
            var seekingQuote = Boundary.QuoteStart;
            StringBuilder sb = new StringBuilder();

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
                                AppendToken();
                                break;

                            case Boundary.QuoteStart:
                                seekingQuote = Boundary.QuoteEnd;
                                AppendToken();
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

            void AppendToken()
            {
                sb.Append(GetToken());
                startTokenIndex = pos + 1;
            }

            string CurrentToken()
            {
                sb.Append(GetToken());
                var result = sb.ToString();
                sb.Clear();
                return result;
            }

            string GetToken() => memory.Slice(startTokenIndex, IndexOfEndOfToken()).ToString();

            int IndexOfEndOfToken() => pos - startTokenIndex;

            bool IsAtEndOfInput() => pos == memory.Length;
        }
    }
}
