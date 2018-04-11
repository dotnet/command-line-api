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

            if (!string.IsNullOrWhiteSpace(source.RawInput))
            {
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
                else
                {
                    var before = source.RawInput.Substring(0, position.Value);

                    var after = source.RawInput.Substring(position.Value);

                    var word = before.Split(' ').LastOrDefault() +
                               after.Split(' ').FirstOrDefault();

                    return word;
                }
            }

            return source.UnmatchedTokens.LastOrDefault() ?? "";
        }

        internal static Command Command(this ParsedSet options) =>
            options.FlattenBreadthFirst()
                   .Select(a => a.Option)
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

            var option = result.ParsedOptions[commandPath.First()];

            foreach (var commandName in commandPath.Skip(1))
            {
                option = option.ParsedOptions[commandName];
            }

            return (ParsedCommand) option;
        }

        internal static Parsed CurrentOption(this ParseResult result) =>
            result.ParsedOptions
                  .LastOrDefault()
                  .AllOptions()
                  .LastOrDefault();

        public static string Diagram(this ParseResult result)
        {
            var builder = new StringBuilder();

            foreach (var o in result.ParsedOptions)
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

        public static string Diagram(this Parsed option)
        {
            var stringbuilder = new StringBuilder();

            stringbuilder.Diagram(option);

            return stringbuilder.ToString();
        }

        private static void Diagram(
            this StringBuilder builder,
            Parsed option)
        {
            builder.Append("[ ");

            builder.Append(option.Option);

            foreach (var child in option.ParsedOptions)
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

        public static CommandExecutionResult Execute(this ParseResult parseResult)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return new CommandExecutionResult(parseResult);
        }

        public static bool HasOption(
            this CommandParseResult parseResult,
            string alias)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.ParsedCommand().ParsedOptions.Contains(alias);
        }

        public static bool HasOption(
            this OptionParseResult parseResult,
            string alias)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.ParsedOptions.Contains(alias);
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
                       ?.Option
                       ?.Suggest(parseResult, position ) ??
            Array.Empty<string>();
    }
}