// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.CommandLine
{
    public static class StringExtensions
    {
        private static readonly char[] _optionPrefixCharacters = { '-' };

        private static readonly Regex _tokenizer = new Regex(
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

        internal static string RemovePrefix(this string option) => option.TrimStart(_optionPrefixCharacters);

        internal static LexResult Lex(
            this IEnumerable<string> args,
            ParserConfiguration configuration)
        {
            var tokenList = new List<Token>();
            var errorList = new List<ParseError>();

            SymbolDefinition currentSymbol = null;
            bool foundEndOfArguments = false;
            var argList = args.ToList();

            var argumentDelimiters = configuration.ArgumentDelimiters.ToArray();

            var knownTokens = new HashSet<Token>(configuration.SymbolDefinitions.SelectMany(ValidTokens));

            for (var i = 0; i < argList.Count; i++)
            {
                var arg = argList[i];

                if (foundEndOfArguments)
                {
                    tokenList.Add(Operand(arg));
                    continue;
                }

                if (arg == "--")
                {
                    tokenList.Add(EndOfArguments());
                    foundEndOfArguments = true;
                    continue;
                }
                else if (configuration.ResponseFileHandling != ResponseFileHandling.Disabled && arg.StartsWith("@"))
                {
                    var filePath = arg.Substring(1);
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        try
                        {
                            var next = i + 1;
                            foreach (var newArg in ParseResponseFile(filePath, configuration.ResponseFileHandling))
                            {
                                argList.Insert(next, newArg);
                                next += 1;
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            errorList.Add(new ParseError(configuration.ValidationMessages.ResponseFileNotFound(filePath),
                                null,
                                false));
                        }
                        catch (IOException e)
                        {
                            errorList.Add(new ParseError(
                                configuration.ValidationMessages.ErrorReadingResponseFile(filePath, e),
                                null,
                                false));
                        }

                        continue;
                    }
                }

                var argHasPrefix = HasPrefix(arg);

                if (argHasPrefix && HasDelimiter(arg))
                {
                    var parts = arg.Split(argumentDelimiters, 2);

                    if (knownTokens.Any(t => t.Value == parts.First()))
                    {
                        tokenList.Add(Option(parts[0]));

                        if (parts.Length > 1)
                        {
                            tokenList.Add(Argument(parts[1]));
                        }
                    }
                    else
                    {
                        tokenList.Add(Argument(arg));
                    }
                }
                else if (configuration.AllowUnbundling && arg.CanBeUnbundled(knownTokens))
                {
                    foreach (var character in arg.Skip(1))
                    {
                        // unbundle e.g. -xyz into -x -y -z
                        tokenList.Add(Option($"-{character}"));
                    }
                }
                else if (knownTokens.All(t => t.Value != arg) ||
                         // if token matches the current commandDefinition name, consider it an argument
                         currentSymbol?.Name == arg)
                {
                    tokenList.Add(Argument(arg));
                }
                else
                {
                    if (argHasPrefix)
                    {
                        tokenList.Add(Option(arg));
                    }
                    else
                    {
                        // when a subcommand is encountered, re-scope which tokens are valid
                        currentSymbol = (currentSymbol?.SymbolDefinitions ??
                                          configuration.SymbolDefinitions)[arg];
                        knownTokens = currentSymbol.ValidTokens();
                        tokenList.Add(Command(arg));
                    }
                }
            }

            return new LexResult {
                Tokens = tokenList,
                Errors = errorList
            };
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
            arg != string.Empty && _optionPrefixCharacters.Contains(arg[0]);

        public static IEnumerable<string> Tokenize(this string s)
        {
            var matches = _tokenizer.Matches(s);

            foreach (Match match in matches)
            {
                foreach (var capture in match.Groups["token"].Captures)
                {
                    yield return capture.ToString();
                }
            }
        }

        private static IEnumerable<string> ParseResponseFile(string filePath, ResponseFileHandling responseFileHandling)
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                var arg = line.Trim();

                if (arg.Length == 0 || arg.StartsWith("#"))
                {
                    continue;
                }

                switch (responseFileHandling)
                {
                    case ResponseFileHandling.ParseArgsAsLineSeparated:
                        yield return line;
                        break;
                    case ResponseFileHandling.ParseArgsAsSpaceSeparated:
                        foreach (var word in Tokenize(arg))
                        {
                            yield return word;
                        }
                        break;
                }

            }
        }

        internal static string NotWhitespace(this string value) => string.IsNullOrWhiteSpace(value) ? null : value;

        private static HashSet<Token> ValidTokens(this SymbolDefinition symbolDefinition) =>
            new HashSet<Token>(
                symbolDefinition.RawAliases
                    .Select(Command)
                    .Concat(
                        symbolDefinition.SymbolDefinitions
                        .SelectMany(
                            s => s.RawAliases
                                .Select(a => new Token(
                                    a,
                                    s is CommandDefinition
                                        ? TokenType.Command
                                        : TokenType.Option)))));
    }
}
