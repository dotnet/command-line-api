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

        public static Command Command(this ParseResult result)
        {
            var commandPath = result
                              .CommandDefinition()
                              .RecurseWhileNotNull(c => c.Parent)
                              .Select(c => c.Name)
                              .Reverse()
                              .ToArray();

            var symbol = result.Symbols[commandPath.First()];

            foreach (var commandName in commandPath.Skip(1))
            {
                symbol = symbol.Children[commandName];
            }

            return (Command)symbol;
        }

        internal static Symbol CurrentSymbol(this ParseResult result) =>
            result.Symbols
                  .LastOrDefault()
                  .AllSymbols()
                  .LastOrDefault();

        public static string Diagram(this ParseResult result)
        {
            var builder = new StringBuilder();

            foreach (var o in result.Symbols)
            {
                builder.Diagram(o);
            }

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

        internal static void Diagram(
            this StringBuilder builder,
            Symbol symbol)
        {
            builder.Append("[ ");

            builder.Append(symbol.SymbolDefinition.Token());

            foreach (var child in symbol.Children)
            {
                builder.Append(" ");
                builder.Diagram(child);
            }

            foreach (var arg in symbol.Arguments)
            {
                builder.Append(" <");
                builder.Append(arg);
                builder.Append(">");
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

            var specifiedCommand = parseResult.Command();

            if (specifiedCommand != null)
            {
                return specifiedCommand.Children.Any(s => s.SymbolDefinition == optionDefinition);
            }

            return parseResult.Symbols.Any(s => s.SymbolDefinition == optionDefinition);
        }

        public static bool HasOption(
            this ParseResult parseResult,
            string alias)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.Command().Children.Contains(alias);
        }

        public static IEnumerable<string> Suggestions(this ParseResult parseResult, int? position = null) =>
            parseResult?.CurrentSymbol()
                       ?.SymbolDefinition
                       ?.Suggest(parseResult, position) ??
            Array.Empty<string>();
    }
}
