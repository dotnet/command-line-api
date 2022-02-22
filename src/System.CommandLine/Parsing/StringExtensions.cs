// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
            return prefixLength > 0 
                       ? alias.Substring(prefixLength) 
                       : alias;
        }

        internal static int GetPrefixLength(this string alias)
        {
            if (alias[0] == '-')
            {
                return alias.Length > 1 && alias[1] == '-' ? 2 : 1;
            }
            if (alias[0] == '/')
            {
                return 1;
            }

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
            var errorList = new List<TokenizeError>();

            Command currentCommand = configuration.RootCommand;
            var foundDoubleDash = false;
            var foundEndOfDirectives = !configuration.EnableDirectives;
            var argList = NormalizeRootCommand(configuration, args);
            var tokenList = new List<Token>(argList.Count);

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
                        tokenList.Add(CommandArgument(arg, currentCommand!));
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
                    if (PreviousTokenIsAnOptionExpectingAnArgument(out var option))
                    {
                        tokenList.Add(OptionArgument(arg, option!));
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
                                    if (cmd != configuration.RootCommand)
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
                else if (arg.TrySplitIntoSubtokens(out var first, out var rest) && 
                         knownTokens.TryGetValue(first, out var subtoken) && 
                         subtoken.Type == TokenType.Option)
                {
                    tokenList.Add(Option(first, (Option)subtoken.Symbol!));

                    if (rest is not null)
                    {
                        tokenList.Add(Argument(rest));
                    }
                }
                else if (!configuration.EnablePosixBundling ||
                         !CanBeUnbundled(arg) ||
                         !TryUnbundle(arg.AsSpan(1), i))
                {
                    tokenList.Add(Argument(arg));
                }
              
                Token Argument(string value) => new(value, TokenType.Argument, default, i);

                Token CommandArgument(string value, Command command) => new(value, TokenType.Argument, command, i);
                
                Token OptionArgument(string value, Option option) => new(value, TokenType.Argument, option, i);

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
                    && arg[1]  != '-'// don't check for "--" prefixed args
                    && arg[2] != ':' && arg[2] != '=' // handled by TrySplitIntoSubtokens
                    && !PreviousTokenIsAnOptionExpectingAnArgument(out _);

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
                            if (!knownTokens.TryGetValue(candidate, out Token? found))
                            {
                                if (tokensBefore != tokenList.Count && tokenList[tokenList.Count - 1].Type == TokenType.Option)
                                {
                                    // Invalid_char_in_bundle_causes_rest_to_be_interpreted_as_value
                                    tokenList.Add(new Token(alias.Slice(i).ToString(), TokenType.Argument, default, argumentIndex));
                                    return true;
                                }

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
            }

            bool PreviousTokenIsAnOptionExpectingAnArgument(out Option? option)
            {
                if (tokenList.Count > 1)
                {
                    var token = tokenList[tokenList.Count - 1];

                    if (token.Type == TokenType.Option)
                    {
                        if (token.Symbol is Option { IsGreedy: true } opt)
                        {
                            option = opt;
                            return true;
                        }
                    }
                }

                option = null;
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

        private static Dictionary<string, Token> ValidTokens(this Command command)
        {
            Dictionary<string, Token> tokens = new (StringComparer.Ordinal);

            foreach (string commandAlias in command.Aliases)
            {
                tokens.Add(
                    commandAlias,
                    new Token(commandAlias, TokenType.Command, command, Token.ImplicitPosition));
            }

            var subCommands = command.Subcommands;
            for (int childIndex = 0; childIndex < subCommands.Count; childIndex++)
            {
                Command cmd = subCommands[childIndex];
                foreach (string childAlias in cmd.Aliases)
                {
                    tokens.Add(childAlias, new Token(childAlias, TokenType.Command, cmd, Token.ImplicitPosition));
                }
            }

            var options = command.Options;
            for (int childIndex = 0; childIndex < options.Count; childIndex++)
            {
                Option option = options[childIndex];
                foreach (string childAlias in option.Aliases)
                {
                    if (!option.IsGlobal || !tokens.ContainsKey(childAlias))
                    {
                        tokens.Add(childAlias, new Token(childAlias, TokenType.Option, option, Token.ImplicitPosition));
                    }
                }
            }

            Command? current = command;
            while (current is not null)
            {
                Command? parentCommand = null;
                ParentNode? parent = current.FirstParent;
                while (parent is not null)
                {
                    if ((parentCommand = parent.Symbol as Command) is not null)
                    {
                        for (var i = 0; i < parentCommand.Options.Count; i++)
                        {
                            Option option = parentCommand.Options[i];
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
                    parent = parent.Next;
                }
                current = parentCommand;
            }

            return tokens;
        }
    }
}