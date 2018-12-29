// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System.CommandLine
{
    public static class StringExtensions
    {
        private static readonly string[] _optionPrefixStrings = { "--", "-", "/" };

        private static readonly Regex _tokenizer = new Regex(
            @"((?<opt>[^""\s]+)""(?<arg>[^""]+)"") # token + quoted argument with non-space argument delimiter, ex: --opt:""c:\path with\spaces""
              |                                
              (""(?<token>[^""]*)"")               # tokens surrounded by spaces, ex: ""c:\path with\spaces""
              |
              (?<token>\S+)                        # tokens containing no quotes or spaces
              ",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace
        );

        internal static bool ContainsCaseInsensitive(
            this string source,
            string value) =>
            CultureInfo.InvariantCulture
                       .CompareInfo
                       .IndexOf(source,
                                value ?? "",
                                CompareOptions.OrdinalIgnoreCase) >= 0;

        internal static string RemovePrefix(this string option)
        {
            foreach (var prefix in _optionPrefixStrings)
            {
                if (option.StartsWith(prefix))
                {
                    return option.Substring(prefix.Length);
                }
            }

            return option;
        }

        internal static LexResult Lex(
            this IEnumerable<string> args,
            CommandLineConfiguration configuration)
        {
            var tokenList = new List<Token>();
            var errorList = new List<ParseError>();

            ISymbol currentSymbol = null;
            var foundEndOfArguments = false;
            var foundEndOfDirectives = false;
            var argList = args.ToList();

            var argumentDelimiters = configuration.ArgumentDelimiters.ToArray();

            var knownTokens = new HashSet<Token>(configuration.Symbols.SelectMany(ValidTokens));

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

                if (!foundEndOfDirectives && arg.StartsWith("[") && arg.EndsWith("]"))
                {
                    tokenList.Add(Directive(arg));
                    continue;
                }

                if (!foundEndOfDirectives && 
                    !configuration.RootCommand.HasRawAlias(arg))
                {
                    foundEndOfDirectives = true;
                }

                if (configuration.ResponseFileHandling != ResponseFileHandling.Disabled &&
                    arg.StartsWith("@"))
                {
                    var filePath = arg.Substring(1);
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        try
                        {
                            var next = i + 1;
                            foreach (var newArg in ParseResponseFile(
                                filePath,
                                configuration.ResponseFileHandling))
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

                if (argHasPrefix &&
                    SplitTokenByArgumentDelimiter(arg, argumentDelimiters) is string[] subtokens &&
                    subtokens.Length > 1)
                {
                    if (knownTokens.Any(t => t.Value == subtokens.First()))
                    {
                        tokenList.Add(Option(subtokens[0]));

                        if (subtokens.Length > 1)
                        {
                            // trim outer quotes in case of, e.g., -x="why"
                            var secondPartWithOuterQuotesRemoved = subtokens[1].Trim('"');
                            tokenList.Add(Argument(secondPartWithOuterQuotesRemoved));
                        }
                    }
                    else
                    {
                        tokenList.Add(Argument(arg));
                    }
                }
                else if (configuration.EnablePosixBundling && 
                         arg.CanBeUnbundled(knownTokens))
                {
                    foreach (var character in arg.Skip(1))
                    {
                        // unbundle e.g. -xyz into -x -y -z
                        tokenList.Add(Option($"-{character}"));
                    }
                }
                else if (knownTokens.All(t => t.Value != arg) ||
                         // if token matches the current command name, consider it an argument
                         currentSymbol?.HasRawAlias(arg) == true)
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
                        ISymbolSet symbolSet;

                        if (currentSymbol is ICommand subcommand)
                        {
                            symbolSet = subcommand.Children;
                        }
                        else
                        {
                            symbolSet = configuration.Symbols;
                        }

                        currentSymbol = symbolSet.GetByAlias(arg);
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

        internal static string[] SplitTokenByArgumentDelimiter(string arg, char[] argumentDelimiters) => arg.Split(argumentDelimiters, 2);

        public static string ToKebabCase(this string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return value;
            }

            var sb = new StringBuilder();
            int i = 0;
            bool addDash = false;

            for (; i < value.Length; i++)
            {
                char ch = value[i];
                if (Char.IsLetterOrDigit(ch))
                {
                    addDash = !Char.IsUpper(ch);
                    sb.Append(Char.ToLowerInvariant(ch));
                    i++;
                    break;
                }
            }

            for (; i < value.Length; i++)
            {
                char ch = value[i];
                if (Char.IsUpper(ch))
                {
                    if (addDash)
                    {
                        addDash = false;
                        sb.Append('-');
                    }

                    sb.Append(Char.ToLowerInvariant(ch));
                }
                else if (Char.IsLetterOrDigit(ch))
                {
                    addDash = true;
                    sb.Append(ch);
                }
                else
                {
                    addDash = false;
                    sb.Append('-');
                }
            }

            return sb.ToString();
        }

        internal static string FromKebabCase(this string value) => value.Replace("-", "");

        private static Token Argument(string value) => new Token(value, TokenType.Argument);

        private static Token Command(string value) => new Token(value, TokenType.Command);

        private static Token Option(string value) => new Token(value, TokenType.Option);

        private static Token EndOfArguments() => new Token("--", TokenType.EndOfArguments);

        private static Token Operand(string value) => new Token(value, TokenType.Operand);

        private static Token Directive(string value) => new Token(value, TokenType.Directive);

        private static bool CanBeUnbundled(
            this string arg,
            IReadOnlyCollection<Token> knownTokens)
        {
            return arg.StartsWith("-") &&
                   !arg.StartsWith("--") &&
                   arg.RemovePrefix()
                      .All(CharacterIsValidOptionAlias);

            bool CharacterIsValidOptionAlias(char c) =>
                knownTokens.Where(t => t.Type == TokenType.Option)
                           .Select(t => t.Value.RemovePrefix())
                           .Contains(c.ToString());
        }

        private static bool HasPrefix(string arg) => _optionPrefixStrings.Any(arg.StartsWith);

        public static IEnumerable<string> Tokenize(this string commandLine)
        {
            var matches = _tokenizer.Matches(commandLine);

            foreach (Match match in matches)
            {
                if (match.Groups["arg"].Captures.Count > 0)
                {
                    var opt = match.Groups["opt"];
                    var arg = match.Groups["arg"];
                    yield return $"{opt}{arg}";
                }
                else
                {
                    foreach (var capture in match.Groups["token"].Captures)
                    {
                        yield return capture.ToString();
                    }
                }
            }
        }

        private static IEnumerable<string> ParseResponseFile(
            string filePath,
            ResponseFileHandling responseFileHandling)
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

        private static HashSet<Token> ValidTokens(this ISymbol symbol)
        {
            var tokens = symbol.RawAliases.Select(Command);

            if (symbol is ICommand command)
            {
                tokens =
                    tokens.Concat(
                        command.Children
                               .SelectMany(
                                   s => s.RawAliases
                                         .Select(a => new Token(
                                                     a,
                                                     s is ICommand
                                                         ? TokenType.Command
                                                         : TokenType.Option))));
            }

            return new HashSet<Token>(tokens);
        }
    }
}
