// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.CommandLine
{
    public static class StringExtensions
    {
        private static readonly char[] optionPrefixCharacters = { '-' };

        private static readonly Regex tokenizer = new Regex(
            @"(""(?<token>[^""]*)"")|(?<token>\S+)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        internal static bool ContainsCaseInsensitive(
            this string source,
            string value) =>
            CultureInfo.InvariantCulture
                       .CompareInfo
                       .IndexOf(source,
                                value ?? "",
                                CompareOptions.OrdinalIgnoreCase) >= 0;

        internal static string RemovePrefix(this string option) =>
            option.TrimStart(optionPrefixCharacters);

        internal static IEnumerable<Token> Lex(
            this IEnumerable<string> args,
            ParserConfiguration configuration)
        {
            Symbol currentCommand = null;
            bool foundEndOfArguments = false;

            var argumentDelimiters = configuration.ArgumentDelimiters.ToArray();

            var knownTokens = new HashSet<Token>(configuration.DefinedSymbols.SelectMany(ValidTokens));

            foreach (var arg in args)
            {
                if (foundEndOfArguments)
                {
                    yield return Operand(arg);
                    continue;
                }
                else if (arg == "--")
                {
                    yield return EndOfArguments();
                    foundEndOfArguments = true;
                    continue;
                }

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
                else if (configuration.AllowUnbundling && arg.CanBeUnbundled(knownTokens))
                {
                    foreach (var character in arg.Skip(1))
                    {
                        // unbundle e.g. -xyz into -x -y -z
                        yield return Option($"-{character}");
                    }
                }
                else if (knownTokens.All(t => t.Value != arg) ||
                         // if token matches the current command name, consider it an argument
                         currentCommand?.Name == arg)
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
                        // when a subcommand is encountered, re-scope which tokens are valid
                        currentCommand = (currentCommand?.DefinedSymbols ??
                                          configuration.DefinedSymbols)[arg];
                        knownTokens = currentCommand.ValidTokens();
                        yield return Command(arg);
                    }
                }
            }
        }

        private static Token Argument(string value) => new Token(value, TokenType.Argument);

        private static Token Command(string value) => new Token(value, TokenType.Command);

        private static Token Option(string value) => new Token(value, TokenType.Option);

        private static Token EndOfArguments() => new Token("--", TokenType.EndOfArguments);

        private static Token Operand(string value) => new Token(value, TokenType.Operand);

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
            arg != string.Empty &&
            optionPrefixCharacters.Contains(arg[0]);

        public static IEnumerable<string> Tokenize(this string s)
        {
            var matches = tokenizer.Matches(s);

            foreach (Match match in matches)
            {
                foreach (var capture in match.Groups["token"].Captures)
                {
                    yield return capture.ToString();
                }
            }
        }

        internal static string NotWhitespace(this string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        private static HashSet<Token> ValidTokens(this Symbol symbol) =>
            new HashSet<Token>(
                symbol.RawAliases
                      .Select(Command)
                      .Concat(
                          symbol.DefinedSymbols
                                .SelectMany(
                                    s =>
                                        s.RawAliases
                                         .Select(
                                             a =>
                                                 new Token(
                                                     a,
                                                     s is Command
                                                         ? TokenType.Command
                                                         : TokenType.Option)))));
    }
}
