// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;
using System.Globalization;
using System.IO;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal static class StringExtensions
    {
        private static readonly string[] _optionPrefixStrings = { "--", "-", "/" };
        private static readonly char[] _argumentDelimiters = { ':', '=' };

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

        internal static string RemovePrefix(this string alias)
        {
            int prefixLength = GetPrefixLength(alias);
            return prefixLength > 0 ? alias.Substring(prefixLength) : alias;
        }

        internal static int GetPrefixLength(this string alias)
        {
            if (alias[0] == '-')
                return alias.Length > 1 && alias[1] == '-' ? 2 : 1;
            if (alias[0] == '/')
                return 1;

            return 0;
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
            var foundDoubleDash = false;
            var foundEndOfDirectives = !configuration.EnableDirectives;
            var argList = NormalizeRootCommand(configuration, args);

            var knownTokens = configuration.RootCommand.ValidTokens();

            for (var i = 0; i < argList.Count; i++)
            {
                var arg = argList[i];
                
                if (foundDoubleDash)
                {
                    if (configuration.EnableLegacyDoubleDashBehavior)
                    {
                        tokenList.Add(Unparsed(arg));
                    }
                    else
                    {
                        tokenList.Add(Argument(arg));
                    }
                    continue;
                }

                if (!foundDoubleDash && 
                    arg == "--")
                {
                    tokenList.Add(DoubleDash());
                    foundDoubleDash = true;
                    continue;
                }

                if (!foundEndOfDirectives)
                {
                    if (arg.Length > 2 &&
                        arg[0] == '[' &&
                        arg[1] != ']' &&
                        arg[1] != ':' &&
                        arg.EndsWith("]", StringComparison.Ordinal))
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

                if (knownTokens.TryGetValue(arg, out var token))
                {
                    if (PreviousTokenIsAnOptionExpectingAnArgument())
                    {
                        tokenList.Add(Argument(arg));
                    }
                    else
                    {
                        switch (token.Type)
                        {
                            case TokenType.Option:
                                tokenList.Add(Option(arg, (Option)token.Symbol!));
                                break;

                            case TokenType.Command:
                                Command cmd = (Command)token.Symbol!;
                                if (cmd != currentCommand)
                                {
                                    if (!(currentCommand is null && cmd == configuration.RootCommand))
                                    {
                                        knownTokens = cmd.ValidTokens();
                                    }
                                    currentCommand = cmd;
                                    tokenList.Add(Command(arg, cmd));
                                }
                                else
                                {
                                    tokenList.Add(Argument(arg));
                                }

                                break;
                        }
                    }
                }
                else if (arg.TrySplitIntoSubtokens(out var first, out var rest) 
                    && knownTokens.TryGetValue(first, out var subtoken) && subtoken.Type == TokenType.Option)
                {
                    tokenList.Add(Option(first, (Option)subtoken.Symbol!));

                    if (rest is { })
                    {
                        tokenList.Add(Argument(rest));
                    }
                }
                else if (configuration.EnablePosixBundling && CanBeUnbundled(arg) && TryUnbundle(arg.AsSpan(1), i))
                {
                    continue;
                }
                else if (arg.Length > 0)
                {
                    tokenList.Add(Argument(arg));
                }

                Token Argument(string value) => new(value, TokenType.Argument, default, i);

                Token Command(string value, Command cmd) => new(value, TokenType.Command, cmd, i);

                Token Option(string value, Option option) => new(value, TokenType.Option, option, i);

                Token DoubleDash() => new("--", TokenType.DoubleDash, default, i);

                Token Unparsed(string value) => new(value, TokenType.Unparsed, default, i);

                Token Directive(string value) => new(value, TokenType.Directive, default, i);
            }

            return new TokenizeResult(tokenList, errorList);

            bool CanBeUnbundled(string arg)
                => arg.Length > 2
                    && arg[0] == '-'
                    && char.IsLetter(arg[1]) // don't check for "--" prefixed args
                    && arg[2] != ':' && arg[2] != '=' // handled by TrySplitIntoSubtokens
                    && !PreviousTokenIsAnOptionExpectingAnArgument();

            bool TryUnbundle(ReadOnlySpan<char> alias, int argumentIndex)
            {
                int tokensBefore = tokenList.Count;

                string candidate = new string('-', 2); // mutable string used to avoid allocations
                unsafe
                {
                    fixed (char* pCandidate = candidate)
                    {
                        for (int i = 0; i < alias.Length; i++)
                        {
                            if (alias[i] == ':' || alias[i] == '=')
                            {
                                tokenList.Add(new Token(alias.Slice(i + 1).ToString(), TokenType.Argument, default, argumentIndex));
                                return true;
                            }

                            pCandidate[1] = alias[i];
                            if (!knownTokens.TryGetValue(candidate, out Token found))
                            {
                                if (tokensBefore != tokenList.Count && tokenList[tokenList.Count - 1].Type == TokenType.Option)
                                {
                                    // Invalid_char_in_bundle_causes_rest_to_be_interpreted_as_value
                                    tokenList.Add(new Token(alias.Slice(i).ToString(), TokenType.Argument, default, argumentIndex));
                                    return true;
                                }

                                RevertTokens(tokensBefore);
                                return false;
                            }

                            if (found.Type != TokenType.Option)
                            {
                                RevertTokens(tokensBefore);
                                return false;
                            }

                            tokenList.Add(new Token(found.Value, found.Type, found.Symbol, argumentIndex));
                            if (i != alias.Length - 1 && ((Option)found.Symbol!).IsGreedy)
                            {
                                int index = i + 1;
                                if (alias[index] == ':' || alias[index] == '=')
                                {
                                    index++; // Last_bundled_option_can_accept_argument_with_colon_separator
                                }
                                tokenList.Add(new Token(alias.Slice(index).ToString(), TokenType.Argument, default, argumentIndex));
                                return true;
                            }
                        }
                    }
                }

                return true;

                void RevertTokens(int lastValidIndex)
                {
                    for (int i = tokenList.Count - 1; i > lastValidIndex; i--)
                    {
                        tokenList.RemoveAt(i);
                    }
                }
            }

            bool PreviousTokenIsAnOptionExpectingAnArgument()
            {
                if (tokenList.Count <= 1)
                {
                    return false;
                }

                var token = tokenList[tokenList.Count - 1];

                if (token.Type != TokenType.Option)
                {
                    return false;
                }

                if (((Option)token.Symbol!).IsGreedy)
                {
                    return true;
                }

                return false;
            }

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
                    var message = configuration.LocalizationResources
                                               .ResponseFileNotFound(filePath);

                    errorList.Add(
                        new TokenizeError(message));
                }
                catch (IOException e)
                {
                    var message = configuration.LocalizationResources
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
            out string first,
            out string? rest)
        {
            var i = arg.IndexOfAny(_argumentDelimiters);

            if (i >= 0)
            {
                first = arg.Substring(0, i);
                rest = arg.Substring(i + 1);
                if (rest.Length == 0)
                {
                    rest = null;
                }

                return true;
            }

            first = arg;
            rest = null;
            return false;
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
            Dictionary<string, Token> tokens = new ();

            foreach (var commandAlias in command.Aliases)
            {
                tokens.Add(
                    commandAlias,
                    new Token(commandAlias, TokenType.Command, command, Token.ImplicitPosition));
            }

            for (int childIndex = 0; childIndex < command.Children.Count; childIndex++)
            {
                switch (command.Children[childIndex])
                {
                    case Command cmd:
                        foreach (var childAlias in cmd.Aliases)
                        {
                            tokens.Add(childAlias, new Token(childAlias, TokenType.Command, cmd, Token.ImplicitPosition));
                        }
                        break;
                    case Option option:
                        foreach (var childAlias in option.Aliases)
                        {
                            if (!option.IsGlobal || !tokens.ContainsKey(childAlias))
                            {
                                tokens.Add(childAlias, new Token(childAlias, TokenType.Option, option, Token.ImplicitPosition));
                            }
                        }
                        break;
                }
            }

            Command? current = command as Command;
            while (current is not null)
            {
                Command? parentCommand = null;
                for (int parentIndex = 0; parentIndex < current.Parents.Count; parentIndex++)
                {
                    if ((parentCommand = current.Parents[parentIndex] as Command) is not null)
                    {
                        foreach (Option option in parentCommand.Options)
                        {
                            if (option.IsGlobal)
                            {
                                foreach (var childAlias in option.Aliases)
                                {
                                    if (!tokens.ContainsKey(childAlias))
                                    {
                                        tokens.Add(childAlias, new Token(childAlias, TokenType.Option, option, Token.ImplicitPosition));
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
                current = parentCommand;
            }

            return tokens;
        }
    }
}