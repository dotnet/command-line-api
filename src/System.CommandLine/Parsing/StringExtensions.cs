﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;
using System.Globalization;
using System.IO;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public static class StringExtensions
    {
        private static readonly string[] _optionPrefixStrings = { "--", "-", "/" };
        private static readonly char[] _argumentDelimiters = {  ':', '=' };

        internal static bool ContainsCaseInsensitive(
            this string source,
            string value) =>
            source.IndexOfCaseInsensitive(value) >= 0;

        internal static int IndexOfCaseInsensitive(
            this string source,
            string value) =>
            CultureInfo.InvariantCulture
                       .CompareInfo
                       .IndexOf(source,
                                value,
                                CompareOptions.OrdinalIgnoreCase);

        internal static string RemovePrefix(this string rawAlias)
        {
            for (var i = 0; i < _optionPrefixStrings.Length; i++)
            {
                var prefix = _optionPrefixStrings[i];
                if (rawAlias.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return rawAlias.Substring(prefix.Length);
                }
            }

            return rawAlias;
        }

        internal static (string? Prefix, string Alias) SplitPrefix(this string rawAlias)
        {
            for (var i = 0; i < _optionPrefixStrings.Length; i++)
            {
                var prefix = _optionPrefixStrings[i];
                if (rawAlias.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return (prefix, rawAlias.Substring(prefix.Length));
                }
            }

            return (null, rawAlias);
        }

        internal static TokenizeResult Tokenize(
            this IReadOnlyList<string> args,
            CommandLineConfiguration configuration)
        {
            var tokenList = new List<Token>();
            var errorList = new List<TokenizeError>();

            ICommand? currentCommand = null;
            var foundEndOfArguments = false;
            var foundEndOfDirectives = !configuration.EnableDirectives;
            var argList = NormalizeRootCommand(configuration, args);

            var knownTokens = configuration.RootCommand.ValidTokens();

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
                    if (arg.StartsWith("[", StringComparison.Ordinal) &&
                        arg.EndsWith("]", StringComparison.Ordinal) &&
                        arg[1] != ']' &&
                        arg[1] != ':')
                    {
                        tokenList.Add(Directive(arg));
                        continue;
                    }

                    if (!configuration.RootCommand.HasAlias(arg))
                    {
                        foundEndOfDirectives = true;
                    }
                }

                if (arg.GetResponseFileReference() is { } filePath &&
                    configuration.ResponseFileHandling != ResponseFileHandling.Disabled)
                {
                    ReadResponseFile(filePath, i);
                    continue;
                }

                if (configuration.EnablePosixBundling &&
                    CanBeUnbundled(arg, out var replacements))
                {
                    for (var ri = 0; ri < replacements!.Count - 1; ri++)
                    {
                        tokenList.Add(UnbundledOption(replacements[ri]));
                    }

                    var lastBundledOptionArg = replacements[replacements.Count - 1];
                    argList.Insert(i + 1, lastBundledOptionArg);
                    argList.RemoveAt(i);
                    arg = argList[i];
                }

                if (arg.TrySplitIntoSubtokens(out var first,
                                              out var rest))
                {
                    if (knownTokens.TryGetValue(first!, out var token) &&
                        token.Type == TokenType.Option)
                    {
                        tokenList.Add(Option(first!));

                        // trim outer quotes in case of, e.g., -x="why"
                        var secondPartWithOuterQuotesRemoved = rest!.Trim('"');
                        tokenList.Add(Argument(secondPartWithOuterQuotesRemoved));
                    }
                    else
                    {
                        tokenList.Add(Argument(arg));
                    }
                }
                else if (!knownTokens.ContainsKey(arg))
                {
                    tokenList.Add(Argument(arg));
                }
                else
                {
                    if (knownTokens.TryGetValue(arg, out var token))
                    {
                        if (token.Type == TokenType.Option)
                        {
                            tokenList.Add(Option(arg));
                        }
                        else if (PreviousTokenIsAnOptionExpectingAnArgument())
                        {
                            tokenList.Add(Argument(arg));
                        }
                        else
                        {
                            // when a subcommand is encountered, re-scope which tokens are valid
                            ISymbolSet symbolSet;

                            if (currentCommand is { } subcommand)
                            {
                                symbolSet = subcommand.Children;
                            }
                            else
                            {
                                symbolSet = configuration.Symbols;
                            }

                            currentCommand = (ICommand) symbolSet.GetByAlias(arg)!;

                            knownTokens = currentCommand.ValidTokens();

                            tokenList.Add(Command(arg));
                        }
                    }
                    else
                    {
                        // when a subcommand is encountered, re-scope which tokens are valid
                        ISymbolSet symbolSet;

                        if (currentCommand is { } subcommand)
                        {
                            symbolSet = subcommand.Children;
                        }
                        else
                        {
                            symbolSet = configuration.Symbols;
                        }

                        currentCommand = (ICommand)symbolSet.GetByAlias(arg)!;

                        knownTokens = currentCommand.ValidTokens();

                        tokenList.Add(Command(arg));
                    }
                }

                Token Argument(string value) => new(value, TokenType.Argument, i);

                Token Command(string value) => new(value, TokenType.Command, i);

                Token Option(string value) => new(value, TokenType.Option, i);
                
                Token UnbundledOption(string value) => new(value, i, wasBundled: true);

                Token EndOfArguments() => new("--", TokenType.EndOfArguments, i);

                Token Operand(string value) => new(value, TokenType.Operand, i);

                Token Directive(string value) => new(value, TokenType.Directive, i);
            }

            return new TokenizeResult(tokenList, errorList);

            bool CanBeUnbundled(string arg, out IReadOnlyList<string>? replacement)
            {
                replacement = null;

                if (arg.Length < 2)
                {
                    return false;
                }

                if (arg[0] != '-')
                {
                    return false;
                }

                // don't unbundle if this is a known token
                if (knownTokens.ContainsKey(arg))
                {
                    return false;
                }

                if (PreviousTokenIsAnOptionExpectingAnArgument())
                {
                    return false;
                }

                // don't unbundle if arg contains an argument token, e.g. "value" in "-abc:value"
                for (var i = 0; i < _argumentDelimiters.Length; i++)
                {
                    var delimiter = _argumentDelimiters[i];

                    if (arg.Contains(delimiter))
                    {
                        foreach (var knownToken in knownTokens.Keys)
                        {
                            if (arg.StartsWith(knownToken + delimiter))
                            {
                                return false;
                            }
                        }
                    }
                }   

                // remove the leading "-"
                var alias = arg.Substring(1);

                return TryUnbundle(out replacement);

                Token? TokenForOptionAlias(char c)
                {
                    if (_argumentDelimiters.Contains(c))
                    {
                        return null;
                    }

                    foreach (var token in knownTokens.Values)
                    {
                        if (token.Type == TokenType.Option &&
                            token.UnprefixedValue == c.ToString())
                        {
                            return token;
                        }
                    }

                    return null;
                }

                void AddRestValue(List<string> list, string rest)
                {
                    if (_argumentDelimiters.Contains(rest[0]))
                    {
                        list[list.Count - 1] += rest;
                    }
                    else
                    {
                        list.Add(rest);
                    }
                }

                bool TryUnbundle(out IReadOnlyList<string>? replacement)
                {
                    if (alias == string.Empty)
                    {
                        replacement = null;
                        return false;
                    }

                    var lastTokenHasArgument = false;
                    var builder = new List<string>();
                    for (var i = 0; i < alias.Length; i++)
                    {
                        var token = TokenForOptionAlias(alias[i]);
                        if (token is null)
                        {
                            if (lastTokenHasArgument)
                            {
                                // The previous token had an optional argument while the current
                                // character does not match any known tokens. Interpret this as
                                // the current character is the first char in the argument.
                                AddRestValue(builder, alias.Substring(i));
                                break;
                            }
                            else
                            {
                                // The previous token did not expect an argument, and the current
                                // character does not match an option, so unbundeling cannot be
                                // done.
                                replacement = null;
                                return false;
                            }
                        }

                        var option = currentCommand?.Children.GetByAlias(token.Value) as IOption;
                        builder.Add(token.Value);

                        // Here we're at an impass, because if we don't have the `IOption`
                        // because we haven't received the correct command yet for instance,
                        // we will take the wrong decision. This is the same logic as the earlier
                        // `CanBeUnbundled` check to take the decision.
                        // A better option is probably introducing a new token-type, and resolve
                        // this after we have the correct model available.
                        var requiresArgument = option?.Argument.Arity.MinimumNumberOfValues > 0;
                        lastTokenHasArgument = option?.Argument.Arity.MaximumNumberOfValues > 0;

                        // If i == arg.Length - 1, we're already at the end of the string
                        // so no need for the custom handling of argument.
                        if (requiresArgument && i < alias.Length - 1)
                        {
                            // The current option requires an argument, and we're still in
                            // the middle of unbundling a string. Example: `-lsomelib.so`
                            // should be interpreted as `-l somelib.so`.
                            AddRestValue(builder, alias.Substring(i + 1));
                            break;
                        }
                    }

                    replacement = builder;
                    return true;
                }
            }

            bool PreviousTokenIsAnOptionExpectingAnArgument() =>
                tokenList.Count > 1 &&
                tokenList[tokenList.Count - 1] is { Type: TokenType.Option } optToken &&
                currentCommand?.Children.GetByAlias(optToken.Value) is Option { Arity: { MaximumNumberOfValues: > 0 } };

            void ReadResponseFile(string filePath, int i)
            {
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
                    var message = configuration.Resources
                                               .ResponseFileNotFound(filePath);

                    errorList.Add(
                        new TokenizeError(message));
                }
                catch (IOException e)
                {
                    var message = configuration.Resources
                                               .ErrorReadingResponseFile(filePath, e);

                    errorList.Add(
                        new TokenizeError(message));
                }
            }
        }

        private static List<string> NormalizeRootCommand(
            CommandLineConfiguration commandLineConfiguration,
            IReadOnlyList<string>? args)
        {
            if (args is null)
            {
                args = new List<string>();
            }

            var list = new List<string>();

            string? potentialRootCommand = null;

            if (args.Count > 0)
            {
                try
                {
                    potentialRootCommand = Path.GetFileName(args[0]);
                }
                catch (ArgumentException)
                {
                    // possible exception for illegal characters in path on .NET Framework
                }

                if (potentialRootCommand != null &&
                    commandLineConfiguration.RootCommand.HasAlias(potentialRootCommand))
                {
                    list.AddRange(args);
                    return list;
                }
            }

            var commandName = commandLineConfiguration.RootCommand.Name;

            list.Add(commandName);

            int startAt = 0;

            if (FirstArgMatchesRootCommand())
            {
                startAt = 1;
            }

            for (var i = startAt; i < args.Count; i++)
            {
                list.Add(args[i]);
            }

            return list;

            bool FirstArgMatchesRootCommand()
            {
                if (potentialRootCommand is null)
                {
                    return false;
                }

                if (potentialRootCommand.Equals($"{commandName}.dll", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (potentialRootCommand.Equals($"{commandName}.exe", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }
        }

        private static string? GetResponseFileReference(this string arg) =>
            arg.Length > 1 && arg[0] == '@'
                ? arg.Substring(1)
                : null;

        internal static bool TrySplitIntoSubtokens(
            this string arg,
            out string? first,
            out string? rest)
        {
            var i = arg.IndexOfAny(_argumentDelimiters);

            if (i >= 0)
            {
                first = arg.Substring(0, i);
                rest = arg.Substring(i + 1, arg.Length - 1 - i);
                return true;
            }

            first = null;
            rest = null;
            return false;
        }

        public static string ToKebabCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var sb = StringBuilderPool.Default.Rent();
            int i = 0;
            bool addDash = false;

            // handles beginning of string, breaks on first letter or digit. addDash might be better named "canAddDash"
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

            return StringBuilderPool.Default.GetStringAndReturn(sb);
        }

        private static IEnumerable<string> ExpandResponseFile(
            string filePath,
            ResponseFileHandling responseFileHandling)
        {
            var lines = File.ReadAllLines(filePath);

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                foreach (var p in SplitLine(line))
                {
                    if (p.GetResponseFileReference() is { } path)
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

                if (arg.Length == 0 || arg[0] == '#')
                {
                    yield break;
                }

                switch (responseFileHandling)
                {
                    case ResponseFileHandling.ParseArgsAsLineSeparated:

                        yield return arg;

                        break;
                    case ResponseFileHandling.ParseArgsAsSpaceSeparated:

                        foreach (var word in CommandLineStringSplitter.Instance.Split(arg))
                        {
                            yield return word;
                        }

                        break;
                }
            }
        }

        private static Dictionary<string, Token> ValidTokens(this ICommand command)
        {
            var tokens = new Dictionary<string, Token>();

            for (var commandAliasIndex = 0; commandAliasIndex < command.Aliases.Count; commandAliasIndex++)
            {
                var commandAlias = command.Aliases.ElementAt(commandAliasIndex);

                tokens.Add(
                    commandAlias,
                    new Token(
                        commandAlias,
                        TokenType.Command, 
                        -1));

                for (var childIndex = 0; childIndex < command.Children.Count; childIndex++)
                {
                    if (command.Children[childIndex] is IIdentifierSymbol identifier)
                    {
                        foreach (var childAlias in identifier.Aliases)
                        {
                            switch (identifier)
                            {
                                case ICommand _:
                                    tokens.TryAdd(childAlias, new Token(childAlias, TokenType.Command, -1));
                                    break;

                                case IOption _:
                                    tokens.TryAdd(childAlias, new Token(childAlias, TokenType.Option, -1));
                                    break;
                            }
                        }
                    }
                }
            }

            return tokens;
        }
    }
}
