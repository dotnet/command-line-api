// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal static class StringExtensions
    {
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

        // this method is not returning a Value Tuple or a dedicated type to avoid JITting
        internal static void Tokenize(
            this IReadOnlyList<string> args,
            Command rootCommand,
            ParserConfiguration configuration,
            bool inferRootCommand,
            out List<Token> tokens,
            out List<string>? errors)
        {
            const int FirstArgIsNotRootCommand = -1;

            List<string>? errorList = null;

            var currentCommand = rootCommand;
            var foundDoubleDash = false;
            var foundEndOfDirectives = false;

            var tokenList = new List<Token>(args.Count);

            var knownTokens = rootCommand.ValidTokens();

            int i = FirstArgumentIsRootCommand(args, rootCommand, inferRootCommand)
                ? 0
                : FirstArgIsNotRootCommand;

            for (; i < args.Count; i++)
            {
                var arg = i == FirstArgIsNotRootCommand
                    ? rootCommand.Name
                    : args[i];

                if (foundDoubleDash)
                {
                    tokenList.Add(CommandArgument(arg, currentCommand!));

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
                        arg[arg.Length - 1] == ']')
                    {
                        int colonIndex = arg.AsSpan().IndexOf(':');
                        string directiveName = colonIndex > 0
                            ? arg.Substring(1, colonIndex - 1) // [name:value]
                            : arg.Substring(1, arg.Length - 2); // [name] is a legal directive

                        Directive? directive;
                        if (knownTokens.TryGetValue($"[{directiveName}]", out var directiveToken))
                        {
                            directive = (Directive)directiveToken.Symbol!;
                        }
                        else
                        {
                            directive = null;
                        }

                        tokenList.Add(Directive(arg, directive));
                        continue;
                    }

                    if (!rootCommand.EqualsNameOrAlias(arg))
                    {
                        foundEndOfDirectives = true;
                    }
                }

                if (configuration.ResponseFileTokenReplacer is { } replacer &&
                    arg.GetReplaceableTokenValue() is { } value)
                {
                    if (replacer(
                            value,
                            out var newTokens,
                            out var error))
                    {
                        if (newTokens is not null && newTokens.Count > 0)
                        {
                            List<string> listWithReplacedTokens = args.ToList();
                            listWithReplacedTokens.InsertRange(i + 1, newTokens);
                            args = listWithReplacedTokens;
                        }
                        continue;
                    }
                    else if (!string.IsNullOrWhiteSpace(error))
                    {
                        (errorList ??= new()).Add(error!);
                        continue;
                    }
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
                                    if (cmd != rootCommand)
                                    {
                                        knownTokens = cmd.ValidTokens(); // config contains Directives, they are allowed only for RootCommand
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

                Token Directive(string value, Directive? directive) => new(value, TokenType.Directive, directive, i);
            }

            tokens = tokenList;
            errors = errorList;

            bool CanBeUnbundled(string arg)
                => arg.Length > 2
                    && arg[0] == '-'
                    && arg[1] != '-'// don't check for "--" prefixed args
                    && arg[2] != ':' && arg[2] != '=' // handled by TrySplitIntoSubtokens
                    && !PreviousTokenIsAnOptionExpectingAnArgument(out _);

            bool TryUnbundle(ReadOnlySpan<char> alias, int argumentIndex)
            {
                int tokensBefore = tokenList.Count;

                Span<char> candidate = ['-', '-'];
                for (int i = 0; i < alias.Length; i++)
                {
                    if (alias[i] == ':' || alias[i] == '=')
                    {
                        tokenList.Add(new Token(alias.Slice(i + 1).ToString(), TokenType.Argument, default, argumentIndex));
                        return true;
                    }

                    candidate[1] = alias[i];
                    if (!knownTokens.TryGetValue(candidate.ToString(), out Token? found))
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
                    if (i != alias.Length - 1 && ((Option)found.Symbol!).Greedy)
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

                return true;
            }

            bool PreviousTokenIsAnOptionExpectingAnArgument(out Option? option)
            {
                if (tokenList.Count > 1)
                {
                    var token = tokenList[tokenList.Count - 1];

                    if (token.Type == TokenType.Option)
                    {
                        if (token.Symbol is Option { Greedy: true } opt)
                        {
                            option = opt;
                            return true;
                        }
                    }
                }

                option = null;
                return false;
            }
        }

        private static bool FirstArgumentIsRootCommand(IReadOnlyList<string> args, Command rootCommand, bool inferRootCommand)
        {
            if (args.Count > 0)
            {
                if (inferRootCommand && args[0] == RootCommand.ExecutablePath)
                {
                    return true;
                }

                try
                {
                    var potentialRootCommand = Path.GetFileName(args[0]);

                    if (rootCommand.EqualsNameOrAlias(potentialRootCommand))
                    {
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // possible exception for illegal characters in path on .NET Framework
                }
            }

            return false;
        }

        private static string? GetReplaceableTokenValue(this string arg) =>
            arg.Length > 1 && arg[0] == '@'
                ? arg.Substring(1)
                : null;

        internal static bool TrySplitIntoSubtokens(
            this string arg,
            out string first,
            out string? rest)
        {
            var i = arg.AsSpan().IndexOfAny(':', '=');

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

        internal static bool TryReadResponseFile(
            string filePath,
            out IReadOnlyList<string>? newTokens,
            out string? error)
        {
            try
            {
                newTokens = ExpandResponseFile(filePath).ToArray();
                error = null;
                return true;
            }
            catch (FileNotFoundException)
            {
                error = LocalizationResources.ResponseFileNotFound(filePath);
            }
            catch (IOException e)
            {
                error = LocalizationResources.ErrorReadingResponseFile(filePath, e);
            }

            newTokens = null;
            return false;

            static IEnumerable<string> ExpandResponseFile(string filePath)
            {
                var lines = File.ReadAllLines(filePath);

                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];

                    foreach (var p in SplitLine(line))
                    {
                        if (p.GetReplaceableTokenValue() is { } path)
                        {
                            foreach (var q in ExpandResponseFile(path))
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
            }

            static IEnumerable<string> SplitLine(string line)
            {
                var arg = line.Trim();

                if (arg.Length == 0 || arg[0] == '#')
                {
                    yield break;
                }

                foreach (var word in CommandLineParser.SplitCommandLine(arg))
                {
                    yield return word;
                }
            }
        }

        private static Dictionary<string, Token> ValidTokens(this Command command)
        {
            Dictionary<string, Token> tokens = new(StringComparer.Ordinal);

            if (command is RootCommand { Directives: IList<Directive> directives })
            {
                for (int i = 0; i < directives.Count; i++)
                {
                    var directive = directives[i];
                    var tokenString = $"[{directive.Name}]";
                    tokens[tokenString] = new Token(tokenString, TokenType.Directive, directive, Token.ImplicitPosition);
                }
            }

            AddCommandTokens(tokens, command);

            if (command.HasSubcommands)
            {
                var subCommands = command.Subcommands;
                for (int i = 0; i < subCommands.Count; i++)
                {
                    AddCommandTokens(tokens, subCommands[i]);
                }
            }

            if (command.HasOptions)
            {
                var options = command.Options;
                
                for (int i = 0; i < options.Count; i++)
                {
                    AddOptionTokens(tokens, options[i]);
                }
            }

            Command? current = command;
            while (current is not null)
            {
                Command? parentCommand = null;
                SymbolNode? parent = current.FirstParent;
                while (parent is not null)
                {
                    if ((parentCommand = parent.Symbol as Command) is not null)
                    {
                        if (parentCommand.HasOptions)
                        {
                            for (var i = 0; i < parentCommand.Options.Count; i++)
                            {
                                Option option = parentCommand.Options[i];
                                if (option.Recursive)
                                {
                                    AddOptionTokens(tokens, option);
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

            static void AddCommandTokens(Dictionary<string, Token> tokens, Command cmd)
            {
                tokens.Add(cmd.Name, new Token(cmd.Name, TokenType.Command, cmd, Token.ImplicitPosition));

                if (cmd._aliases is not null)
                {
                    foreach (string childAlias in cmd._aliases)
                    {
                        tokens.Add(childAlias, new Token(childAlias, TokenType.Command, cmd, Token.ImplicitPosition));
                    }
                }
            }

            static void AddOptionTokens(Dictionary<string, Token> tokens, Option option)
            {
                if (!tokens.ContainsKey(option.Name))
                {
                    tokens.Add(option.Name, new Token(option.Name, TokenType.Option, option, Token.ImplicitPosition));
                }

                if (option._aliases is not null)
                {
                    foreach (string childAlias in option._aliases)
                    {
                        if (!tokens.ContainsKey(childAlias))
                        {
                            tokens.Add(childAlias, new Token(childAlias, TokenType.Option, option, Token.ImplicitPosition));
                        }
                    }
                }
            }
        }
    }
}