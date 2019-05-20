// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace System.CommandLine
{
    public static class StringExtensions
    {
        private static readonly string[] _optionPrefixStrings = { "--", "-", "/" };


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

        internal static TokenizeResult Tokenize(
            this IEnumerable<string> args,
            CommandLineConfiguration configuration)
        {
            var tokenList = new List<Token>();
            var errorList = new List<TokenizeError>();

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

                if (!foundEndOfDirectives)
                {
                    if (arg.StartsWith("[") && 
                        arg.EndsWith("]") && 
                        arg[1] != ']' && 
                        arg[1] != ':')
                    {
                        tokenList.Add(Directive(arg));
                        continue;
                    }

                    if (!configuration.RootCommand.HasRawAlias(arg))
                    {
                        foundEndOfDirectives = true;
                    }
                }

                if (configuration.ResponseFileHandling != ResponseFileHandling.Disabled &&
                    arg.GetResponseFileReference() is string filePath)
                {
                    ReadResponseFile(filePath, i);
                    continue;
                }

                var argHasPrefix = HasPrefix(arg);

                if (argHasPrefix &&
                    arg.SplitByDelimiters(argumentDelimiters) is string[] subtokens &&
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

            return new TokenizeResult(tokenList, errorList);

            void ReadResponseFile(string filePath, int i)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    errorList.Add(
                        new TokenizeError(
                            $"Invalid response file token: {filePath}"));
                    return;
                }

                try
                {
                    var next = i + 1;

                    foreach (var newArg in ExpandResponseFile(
                        filePath,
                        configuration.ResponseFileHandling))
                    {
                        argList.Insert(next, newArg);
                        next += 1;
                    }
                }
                catch (FileNotFoundException)
                {
                    var message = configuration.ValidationMessages
                                               .ResponseFileNotFound(filePath);

                    errorList.Add(
                        new TokenizeError(message));
                }
                catch (IOException e)
                {
                    var message = configuration.ValidationMessages
                                               .ErrorReadingResponseFile(filePath, e);

                    errorList.Add(
                        new TokenizeError(message));
                }
            }
        }

        private static string GetResponseFileReference(this string arg) =>
            arg.StartsWith("@") && arg.Length > 1
                ? arg.Substring(1)
                : null;

        internal static string[] SplitByDelimiters(
            this string arg, 
            char[] argumentDelimiters) => arg.Split(argumentDelimiters, 2);

        public static string ToKebabCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var sb = new StringBuilder();
            int i = 0;
            bool addDash = false;

            // handles beginning of string, breaks onfirst letter or digit. addDash might be better named "canAddDash"
            for (; i < value.Length; i++)
            {
                char ch = value[i];
                if (char.IsLetterOrDigit(ch))
                {
                    addDash = !char.IsUpper(ch);
                    sb.Append(char.ToLowerInvariant(ch));
                    i++;
                    break;
                }
            }

            // reusing i, start at the same place
            for (; i < value.Length; i++)
            {
                char ch = value[i];
                if (char.IsUpper(ch))
                {
                    if (addDash)
                    {
                        addDash = false;
                        sb.Append('-');
                    }

                    sb.Append(char.ToLowerInvariant(ch));
                }
                else if (char.IsLetterOrDigit(ch))
                {
                    addDash = true;
                    sb.Append(ch);
                }
                else  //this coverts all non letter/digits to dash - specifically periods and underscores. Is this needed?
                {
                    addDash = false;
                    sb.Append('-');
                }
            }

            return sb.ToString();
        }

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

        public static IEnumerable<string> SplitCommandLine(this string commandLine)
        {
            return CommandLineStringSplitter.Instance.Split(commandLine);
        }

        private static IEnumerable<string> ExpandResponseFile(
            string filePath,
            ResponseFileHandling responseFileHandling)
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                foreach (var p in SplitLine(line))
                {
                    if (p.GetResponseFileReference() is string path)
                    {
                        foreach (var q in ExpandResponseFile(
                            path,
                            responseFileHandling))
                        {
                            yield return q;
                        }
                    }
                    else
                    {
                        yield return p;
                    }
                }
            }

            IEnumerable<string> SplitLine(string line)
            {
                var arg = line.Trim();

                if (arg.Length == 0 || arg.StartsWith("#"))
                {
                    yield break;
                }

                switch (responseFileHandling)
                {
                    case ResponseFileHandling.ParseArgsAsLineSeparated:

                        yield return line;

                        break;
                    case ResponseFileHandling.ParseArgsAsSpaceSeparated:

                        foreach (var word in SplitCommandLine(arg))
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
