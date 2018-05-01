// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.DotNet.Cli.CommandLine
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

        internal static Command Command(this ParsedSymbolSet options) =>
            options.FlattenBreadthFirst()
                   .Select(a => a.Symbol)
                   .OfType<Command>()
                   .LastOrDefault();

        public static ParsedCommand ParsedCommand(this CommandParseResult result)
        {
            var commandPath = result
                              .Command()
                              .RecurseWhileNotNull(c => c.Parent)
                              .Select(c => c.Name)
                              .Reverse()
                              .ToArray();

            var symbol = result.ParsedSymbols[commandPath.First()];

            foreach (var commandName in commandPath.Skip(1))
            {
                symbol = symbol.Children[commandName];
            }

            return (ParsedCommand) symbol;
        }

        internal static ParsedSymbol CurrentOption(this ParseResult result) =>
            result.ParsedSymbols
                  .LastOrDefault()
                  .AllOptions()
                  .LastOrDefault();

        public static string Diagram(this ParseResult result)
        {
            var builder = new StringBuilder();

            foreach (var o in result.ParsedSymbols)
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

        public static string Diagram(this ParsedSymbol option)
        {
            var stringbuilder = new StringBuilder();

            stringbuilder.Diagram(option);

            return stringbuilder.ToString();
        }

        private static void Diagram(
            this StringBuilder builder,
            ParsedSymbol option)
        {
            builder.Append("[ ");

            builder.Append(option.Symbol);

            foreach (var child in option.Children)
            {
                builder.Append(" ");
                builder.Diagram(child);
            }

            foreach (var arg in option.Arguments)
            {
                builder.Append(" <");
                builder.Append(arg);
                builder.Append(">");
            }

            builder.Append(" ]");
        }

        public static bool HasOption(
            this CommandParseResult parseResult,
            string alias)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.ParsedCommand().Children.Contains(alias);
        }

        public static bool HasOption(
            this OptionParseResult parseResult,
            string alias)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.ParsedSymbols.Contains(alias);
        }

        internal static int? ImplicitCursorPosition(this ParseResult parseResult)
        {
            if (parseResult.RawInput != null)
            {
                return parseResult.RawInput.Length;
            }

            return string.Join(" ", parseResult.Tokens).Length;
        }

        public static IEnumerable<string> Suggestions(this ParseResult parseResult, int? position = null) =>
            parseResult?.CurrentOption()
                       ?.Symbol
                       ?.Suggest(parseResult, position ) ??
            Array.Empty<string>();
    }
}