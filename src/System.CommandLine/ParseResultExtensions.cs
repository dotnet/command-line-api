// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.CommandLine
{
    public static class ParseResultExtensions
    {
        public static string TextToMatch(
            this ParseResult source,
            int? position = null)
        {
            var lastToken = source.Tokens.LastOrDefault();

            if (string.IsNullOrWhiteSpace(source.RawInput))
            {
                return source.UnmatchedTokens.LastOrDefault() ?? "";
            }

            if (position == null)
            {
                // assume the cursor is at the end of the input
                if (!source.RawInput.EndsWith(" "))
                {
                    return lastToken;
                }
                else
                {
                    return "";
                }
            }

            var textBeforeCursor = source.RawInput.Substring(0, position.Value);

            var textAfterCursor = source.RawInput.Substring(position.Value);

            return textBeforeCursor.Split(' ').LastOrDefault() +
                   textAfterCursor.Split(' ').FirstOrDefault();
        }

        internal static Symbol CurrentSymbol(this ParseResult result) =>
            result.Command
                  .AllSymbols()
                  .LastOrDefault();

        public static string Diagram(this ParseResult result)
        {
            var builder = new StringBuilder();

            builder.Diagram(result.RootCommand, result);

            if (result.UnmatchedTokens.Any())
            {
                builder.Append("   ???-->");

                foreach (var error in result.UnmatchedTokens)
                {
                    builder.Append(" ");
                    builder.Append(error);
                }
            }

            return builder.ToString();
        }

        private static void Diagram(
            this StringBuilder builder,
            Symbol symbol,
            ParseResult parseResult)
        {
            if (parseResult.Errors.Any(e => e.Symbol == symbol))
            {
                builder.Append("!");
            }
            
            if (symbol is Option option &&
                option.IsImplicit)
            {
                builder.Append("*");
            }

            builder.Append("[ ");

            builder.Append(symbol.Token);

            foreach (var child in symbol.Children)
            {
                builder.Append(" ");
                builder.Diagram(child, parseResult);
            }

            if (symbol.Arguments.Count > 0)
            {
                foreach (var arg in symbol.Arguments)
                {
                    builder.Append(" <");
                    builder.Append(arg);
                    builder.Append(">");
                }
            }
            else
            {
                var result = symbol.Result;
                if (result is SuccessfulArgumentParseResult _)
                {
                    var value = symbol.GetValueOrDefault();
                    
                    switch (value)
                    {
                        case null:
                        case IReadOnlyCollection<string> a when a.Count == 0:
                            break;
                        default:
                            builder.Append(" <");
                            builder.Append(value);
                            builder.Append(">");
                            break;
                    }
                }
            }

            builder.Append(" ]");
        }

        public static bool HasOption(
            this ParseResult parseResult,
            OptionDefinition optionDefinition)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.Command.Children.Any(s => s.SymbolDefinition == optionDefinition);
        }

        public static bool HasOption(
            this ParseResult parseResult,
            string alias)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.Command.Children.Contains(alias);
        }

        public static IEnumerable<string> Suggestions(
            this ParseResult parseResult,
            int? position = null)
        {
            var currentSymbolDefinition = parseResult?.CurrentSymbol().SymbolDefinition;

            var currentSymbolSuggestions = currentSymbolDefinition
                                               ?.Suggest(parseResult, position)
                                           ?? Array.Empty<string>();

            var parentSymbolDefinition = currentSymbolDefinition?.Parent;

            var parentSymbolSuggestions = parentSymbolDefinition?.Suggest(parseResult, position)
                                          ?? Array.Empty<string>();

            return parentSymbolSuggestions
                   .Concat(currentSymbolSuggestions)
                   .ToArray();
        }
    }
}
