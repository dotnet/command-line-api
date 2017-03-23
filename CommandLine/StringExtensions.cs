// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class StringExtensions
    {
        private static readonly char[] optionPrefixCharacters = { '-' };

        private static readonly Regex tokenizer = new Regex(
            @"(""(?<q>[^""]+)"")|(?<q>\S+)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        internal static string AddPrefix(this string option) =>
            (option = option.RemovePrefix())
            .Length == 1
                ? $"-{option}"
                : $"--{option}";

        internal static bool ContainsCaseInsensitive(
            this string source,
            string value) =>
            CultureInfo.InvariantCulture
                       .CompareInfo
                       .IndexOf(source,
                                value ?? "",
                                CompareOptions.OrdinalIgnoreCase) >= 0;

        internal static IEnumerable<string> FindSuggestions(
            this IReadOnlyCollection<string> candidates,
            ParseResult parseResult) =>
            candidates.FindSuggestions(parseResult.TextToMatch());

        internal static IEnumerable<string> FindSuggestions(
            this IReadOnlyCollection<string> candidates,
            string textToMatch) =>
            candidates
                .OrderBy(c => c)
                .Where(c => c.ContainsCaseInsensitive(textToMatch))
                .Distinct()
                .OrderBy(c => c);

        internal static string RemoveEnd(
            this string source,
            int length) =>
            source.Remove(
                source.Length - length,
                length);

        internal static string RemovePrefix(this string option) =>
            option.TrimStart(optionPrefixCharacters);

        internal static IEnumerable<Token> Lex(
            this IEnumerable<string> args,
            HashSet<Token> knownTokens,
            ParserConfiguration configuration)
        {
            var foundCommandss = new HashSet<string>();

            var argumentDelimiters = configuration.ArgumentDelimiters.ToArray();

            foreach (var arg in args)
            {
                var argHasPrefix = HasPrefix(arg);

                if (argHasPrefix && HasDelimiter(arg))
                {
                    var parts = arg.Split(argumentDelimiters, 2);

                    if (knownTokens.Any(t => t.Value == parts.First()))
                    {
                        yield return Option(parts[0]);

                        if (parts.Length > 1)
                        {
                            yield return Argument(parts[1]);
                        }
                    }
                    else
                    {
                        yield return Argument(arg);
                    }
                }
                else if (arg.CanBeUnbundled(knownTokens))
                {
                    foreach (var character in arg.Skip(1))
                    {
                        // unbundle e.g. -xyz into -x -y -z
                        yield return Option($"-{character}");
                    }
                }
                else if (knownTokens.All(t => t.Value != arg) ||
                         // a given command can only occur once in a command line
                         foundCommandss.Contains(arg))
                {
                    yield return Argument(arg);
                }
                else
                {
                    if (argHasPrefix)
                    {
                        yield return Option(arg);
                    }
                    else
                    {
                        foundCommandss.Add(arg);
                        yield return Command(arg);
                    }
                }
            }
        }

        private static Token Argument(this string value) => new Token(value, TokenType.Argument);

        private static Token Command(this string value) => new Token(value, TokenType.Command);

        private static Token Option(this string value) => new Token(value, TokenType.Option);

        private static bool CanBeUnbundled(
            this string arg,
            IReadOnlyCollection<Token> knownTokens) =>
            arg.StartsWith("-") &&
            !arg.StartsWith("--") &&
            arg.RemovePrefix()
               .All(c => knownTokens
                        .Where(t => t.Type == TokenType.Option)
                        .Select(t => t.Value.RemovePrefix())
                        .Contains(c.ToString()));

        private static bool HasDelimiter(string arg) =>
            arg.Contains("=") ||
            arg.Contains(":");

        private static bool HasPrefix(string arg) =>
            optionPrefixCharacters.Contains(arg[0]);

        public static IEnumerable<string> Tokenize(this string s)
        {
            var matches = tokenizer.Matches(s);

            foreach (Match match in matches)
            {
                foreach (var capture in match.Groups["q"].Captures)
                {
                    yield return capture.ToString();
                }
            }
        }
    }
}