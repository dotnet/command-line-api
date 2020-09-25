// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace System.CommandLine.Parsing
{
    public static class StringExtensions
    {
        private static readonly string[] _optionPrefixStrings = { "--", "-", "/" };

        internal static bool ContainsCaseInsensitive(
            this string source,
            string? value) =>
            source.IndexOfCaseInsensitive(value) >= 0;

        internal static int IndexOfCaseInsensitive(
            this string source,
            string? value) =>
            CultureInfo.InvariantCulture
                       .CompareInfo
                       .IndexOf(source,
                                value ?? "",
                                CompareOptions.OrdinalIgnoreCase);

        internal static string RemovePrefix(this string rawAlias)
        {
            for (var i = 0; i < _optionPrefixStrings.Length; i++)
            {
                var prefix = _optionPrefixStrings[i];
                if (rawAlias.StartsWith(prefix))
                {
                    return rawAlias.Substring(prefix.Length);
                }
            }

            return rawAlias;
        }

        internal static (string? prefix, string alias) SplitPrefix(this string rawAlias)
        {
            for (var i = 0; i < _optionPrefixStrings.Length; i++)
            {
                var prefix = _optionPrefixStrings[i];
                if (rawAlias.StartsWith(prefix))
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

            var argumentDelimiters = configuration.ArgumentDelimitersInternal.ToArray();

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
                    if (arg.StartsWith("[") && 
                        arg.EndsWith("]") && 
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

                if (configuration.ResponseFileHandling != ResponseFileHandling.Disabled &&
                    arg.GetResponseFileReference() is { } filePath)
                {
                    ReadResponseFile(filePath, i);
                    continue;
                }

                if (configuration.EnablePosixBundling && 
                    CanBeUnbundled(arg, out IReadOnlyCollection<string>? replacement))
                {
                    argList.InsertRange(i + 1, replacement);
                    argList.RemoveAt(i);
                    arg = argList[i];
                }

                if (arg.TrySplitIntoSubtokens(argumentDelimiters, 
                                              out var first, 
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
                else if (!knownTokens.ContainsKey(arg) ||
                         // if token matches the current command name, consider it an argument
                         currentCommand?.HasAlias(arg) == true)
                {
                    tokenList.Add(Argument(arg));
                }
                else
                {
                    if (knownTokens.TryGetValue(arg, out var token) &&
                        token.Type == TokenType.Option)
                    {
                        tokenList.Add(Option(arg));
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
            }

            return new TokenizeResult(tokenList, errorList);

            bool CanBeUnbundled(string arg, out IReadOnlyCollection<string>? replacement)
            {
                replacement = null;

                if (tokenList.Count == 0)
                {
                    replacement = null;
                    return false;
                }

                // don't unbundle if the last token is an option expecting an argument
                if (tokenList[tokenList.Count - 1] is { } lastToken &&
                    lastToken.Type == TokenType.Option &&
                    currentCommand?.Children.GetByAlias(lastToken.Value) is IOption option && 
                    option.Argument.Arity.MinimumNumberOfValues > 0)
                {
                    return false;
                }

                var (prefix, alias) = arg.SplitPrefix();

                return prefix == "-" && 
                       TryUnbundle(out replacement);

                Token? TokenForOptionAlias(char c)
                {
                    if (argumentDelimiters.Contains(c))
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
                    if (argumentDelimiters.Contains(rest[0]))
                    {
                        list[list.Count - 1] += rest;
                    }
                    else
                    {
                        list.Add(rest);
                    }
                }
                
                bool TryUnbundle(out IReadOnlyCollection<string>? replacement)
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
            arg.StartsWith("@") && arg.Length > 1
                ? arg.Substring(1)
                : null;

        internal static bool TrySplitIntoSubtokens(
            this string arg,
            char[] delimiters,
            out string? first,
            out string? rest)
        {
            var delimitersArray = delimiters;

            for (var j = 0; j < delimiters.Length; j++)
            {
                var i = arg.IndexOfAny(delimitersArray);

                if (i >= 0)
                {
                    first = arg.Substring(0, i);

                    if (arg.Length > i)
                    {
                        rest = arg.Substring(i + 1, arg.Length - 1 - i);
                    }
                    else
                    {
                        rest = null;
                    }
                    return true;
                }
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

            var sb = new StringBuilder();
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

            return sb.ToString();
        }

        private static Token Argument(string value) => new Token(value, TokenType.Argument);

        private static Token Command(string value) => new Token(value, TokenType.Command);

        private static Token Option(string value) => new Token(value, TokenType.Option);

        private static Token EndOfArguments() => new Token("--", TokenType.EndOfArguments);

        private static Token Operand(string value) => new Token(value, TokenType.Operand);

        private static Token Directive(string value) => new Token(value, TokenType.Directive);

        private static IEnumerable<string> ExpandResponseFile(
            string filePath,
            ResponseFileHandling responseFileHandling)
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
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

                if (arg.Length == 0 || arg.StartsWith("#"))
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
                        TokenType.Command));

                for (var childIndex = 0; childIndex < command.Children.Count; childIndex++)
                {
                    var child = command.Children[childIndex];

                    for (var childAliasIndex = 0; childAliasIndex < child.Aliases.Count; childAliasIndex++)
                    {
                        var childAlias = child.Aliases.ElementAt(childAliasIndex);

                        switch (child)
                        {
                            case ICommand _:
                                tokens.TryAdd(childAlias, new Token(childAlias, TokenType.Command));
                                break;

                            case IOption _:
                                tokens.TryAdd(childAlias, new Token(childAlias, TokenType.Option));
                                break;
                        }
                    }
                }
            }

            return tokens;
        }
    }
}
