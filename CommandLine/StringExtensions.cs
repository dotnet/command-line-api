// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class StringExtensions
    {
        private static readonly char[] optionPrefixCharacters = { '-', '/' };
        private static readonly char[] argumentDelimiterCharacters = { '=', ':' };

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

        internal static void RemoveEnd(
            this StringBuilder source,
            string value)
        {
            if (source.ToString().EndsWith(value))
            {
                RemoveEnd(source, value.Length);
            }
        }

        internal static void RemoveEnd(
            this StringBuilder source,
            int length) =>
            source.Remove(
                source.Length - length,
                length);

        internal static IEnumerable<string> Lex(this IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                var argHasPrefix = HasPrefix(arg);

                if (argHasPrefix && HasDelimiter(arg))
                {
                    var parts = arg.Split(argumentDelimiterCharacters, 2);

                    yield return parts[0];
                    yield return parts[1];
                }
                else if (argHasPrefix && !arg.StartsWith("--"))
                {
                    foreach (var character in arg.Skip(1))
                    {
                        // unbundle e.g. -xyz into -x -y -z
                        yield return $"-{character}";
                    }
                }
                else
                {
                    yield return arg;
                }
            }
        }

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