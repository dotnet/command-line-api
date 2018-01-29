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
        public static string TextToMatch(this ParseResult source)
        {
            var lastToken = source.Tokens.LastOrDefault();

            if (string.IsNullOrWhiteSpace(lastToken))
            {
                return "";
            }

            if (source.IsProgressive)
            {
                return lastToken;
            }

            return source.UnmatchedTokens.LastOrDefault() ?? "";
        }

        internal static Command Command(this AppliedOptionSet options) =>
            options.FlattenBreadthFirst()
                   .Select(a => a.Option)
                   .OfType<Command>()
                   .LastOrDefault();

        public static AppliedOption AppliedCommand(this ParseResult result)
        {
            var commandPath = result
                .Command()
                .RecurseWhileNotNull(c => c.Parent as Command)
                .Select(c => c.Name)
                .Reverse()
                .ToArray();

            var option = result[commandPath.First()];

            foreach (var commandName in commandPath.Skip(1))
            {
                option = option[commandName];
            }

            return option;
        }

        internal static AppliedOption CurrentOption(this ParseResult result) =>
            result.AppliedOptions
                  .LastOrDefault()
                  .AllOptions()
                  .LastOrDefault();

        public static string Diagram(this ParseResult result)
        {
            var builder = new StringBuilder();

            foreach (var o in result.AppliedOptions)
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

        public static string Diagram(this AppliedOption option)
        {
            var stringbuilder = new StringBuilder();

            stringbuilder.Diagram(option);

            return stringbuilder.ToString();
        }

        private static void Diagram(
            this StringBuilder builder,
            AppliedOption option)
        {
            builder.Append("[ ");

            builder.Append(option.Option);

            foreach (var childOption in option.AppliedOptions)
            {
                builder.Append(" ");
                builder.Diagram(childOption);
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
            this ParseResult parseResult,
            string alias)
        {
            if (parseResult == null)
            {
                throw new ArgumentNullException(nameof(parseResult));
            }

            return parseResult.AppliedOptions.Contains(alias);
        }

        public static IEnumerable<string> Suggestions(this ParseResult parseResult) =>
            parseResult?.CurrentOption()
                       ?.Option
                       ?.Suggest(parseResult) ??
            Array.Empty<string>();
    }
}