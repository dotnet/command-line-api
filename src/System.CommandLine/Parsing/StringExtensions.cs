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

        internal static (string? Prefix, string Alias) SplitPrefix(this string rawAlias)
        {
            if (rawAlias[0] == '/')
            {
                return ("/", rawAlias.Substring(1));
            }
            else if (rawAlias[0] == '-')
            {
                if (rawAlias.Length > 1 && rawAlias[1] == '-')
                {
                    return ("--", rawAlias.Substring(2));
                }

                return ("-", rawAlias.Substring(1));
            }

            return (null, rawAlias);
        }

        // this method is not returning a Value Tuple or a dedicated type to avoid JITting
        internal static void Tokenize(
            this IReadOnlyList<string> args,
            CliConfiguration configuration,
            bool inferRootCommand,
            out List<CliToken> tokens,
            out List<string>? errors)
        {
            const int FirstArgIsNotRootCommand = -1;

            List<string>? errorList = null;

            var currentCommand = configuration.RootCommand;
            var foundDoubleDash = false;
            var foundEndOfDirectives = false;

            var tokenList = new List<CliToken>(args.Count);

            var knownTokens = configuration.RootCommand.ValidTokens();

            int i = FirstArgumentIsRootCommand(args, configuration.RootCommand, inferRootCommand)
                ? 0
                : FirstArgIsNotRootCommand;

            for (; i < args.Count; i++)
            {
                var arg = i == FirstArgIsNotRootCommand
                    ? configuration.RootCommand.Name
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

                        CliDirective? directive;
                        if (knownTokens.TryGetValue($"[{directiveName}]", out var directiveToken))
                        {
                            directive = (CliDirective)directiveToken.Symbol!;
                        }
                        else
                        {
                            directive = null;
                        }

                        tokenList.Add(Directive(arg, directive));
                        continue;
                    }

                    if (!configuration.RootCommand.EqualsNameOrAlias(arg))
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
                            case CliTokenType.Option:
                                tokenList.Add(Option(arg, (CliOption)token.Symbol!));
                                break;

                            case CliTokenType.Command:
                                CliCommand cmd = (CliCommand)token.Symbol!;
                                if (cmd != currentCommand)
                                {
                                    if (cmd != configuration.RootCommand)
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
                         subtoken.Type == CliTokenType.Option)
                {
                    tokenList.Add(Option(first, (CliOption)subtoken.Symbol!));

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

                CliToken Argument(string value) => new(value, CliTokenType.Argument, default, i);

                CliToken CommandArgument(string value, CliCommand command) => new(value, CliTokenType.Argument, command, i);

                CliToken OptionArgument(string value, CliOption option) => new(value, CliTokenType.Argument, option, i);

                CliToken Command(string value, CliCommand cmd) => new(value, CliTokenType.Command, cmd, i);

                CliToken Option(string value, CliOption option) => new(value, CliTokenType.Option, option, i);

                CliToken DoubleDash() => new("--", CliTokenType.DoubleDash, default, i);

                CliToken Directive(string value, CliDirective? directive) => new(value, CliTokenType.Directive, directive, i);
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

                string candidate = new('-', 2); // mutable string used to avoid allocations
                unsafe
                {
                    fixed (char* pCandidate = candidate)
                    {
                        for (int i = 0; i < alias.Length; i++)
                        {
                            if (alias[i] == ':' || alias[i] == '=')
                            {
                                tokenList.Add(new CliToken(alias.Slice(i + 1).ToString(), CliTokenType.Argument, default, argumentIndex));
                                return true;
                            }

                            pCandidate[1] = alias[i];
                            if (!knownTokens.TryGetValue(candidate, out CliToken? found))
                            {
                                if (tokensBefore != tokenList.Count && tokenList[tokenList.Count - 1].Type == CliTokenType.Option)
                                {
                                    // Invalid_char_in_bundle_causes_rest_to_be_interpreted_as_value
                                    tokenList.Add(new CliToken(alias.Slice(i).ToString(), CliTokenType.Argument, default, argumentIndex));
                                    return true;
                                }

                                return false;
                            }

                            tokenList.Add(new CliToken(found.Value, found.Type, found.Symbol, argumentIndex));
                            if (i != alias.Length - 1 && ((CliOption)found.Symbol!).Greedy)
                            {
                                int index = i + 1;
                                if (alias[index] == ':' || alias[index] == '=')
                                {
                                    index++; // Last_bundled_option_can_accept_argument_with_colon_separator
                                }
                                tokenList.Add(new CliToken(alias.Slice(index).ToString(), CliTokenType.Argument, default, argumentIndex));
                                return true;
                            }
                        }
                    }
                }

                return true;
            }

            bool PreviousTokenIsAnOptionExpectingAnArgument(out CliOption? option)
            {
                if (tokenList.Count > 1)
                {
                    var token = tokenList[tokenList.Count - 1];

                    if (token.Type == CliTokenType.Option)
                    {
                        if (token.Symbol is CliOption { Greedy: true } opt)
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

        private static bool FirstArgumentIsRootCommand(IReadOnlyList<string> args, CliCommand rootCommand, bool inferRootCommand)
        {
            if (args.Count > 0)
            {
                if (inferRootCommand && args[0] == CliRootCommand.ExecutablePath)
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

                foreach (var word in CliParser.SplitCommandLine(arg))
                {
                    yield return word;
                }
            }
        }

        private static Dictionary<string, CliToken> ValidTokens(this CliCommand command)
        {
            Dictionary<string, CliToken> tokens = new(StringComparer.Ordinal);

            if (command is CliRootCommand { Directives: IList<CliDirective> directives })
            {
                for (int i = 0; i < directives.Count; i++)
                {
                    var directive = directives[i];
                    var tokenString = $"[{directive.Name}]";
                    tokens[tokenString] = new CliToken(tokenString, CliTokenType.Directive, directive, CliToken.ImplicitPosition);
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

            CliCommand? current = command;
            while (current is not null)
            {
                CliCommand? parentCommand = null;
                SymbolNode? parent = current.FirstParent;
                while (parent is not null)
                {
                    if ((parentCommand = parent.Symbol as CliCommand) is not null)
                    {
                        if (parentCommand.HasOptions)
                        {
                            for (var i = 0; i < parentCommand.Options.Count; i++)
                            {
                                CliOption option = parentCommand.Options[i];
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

            static void AddCommandTokens(Dictionary<string, CliToken> tokens, CliCommand cmd)
            {
                tokens.Add(cmd.Name, new CliToken(cmd.Name, CliTokenType.Command, cmd, CliToken.ImplicitPosition));

                if (cmd._aliases is not null)
                {
                    foreach (string childAlias in cmd._aliases)
                    {
                        tokens.Add(childAlias, new CliToken(childAlias, CliTokenType.Command, cmd, CliToken.ImplicitPosition));
                    }
                }
            }

            static void AddOptionTokens(Dictionary<string, CliToken> tokens, CliOption option)
            {
                if (!tokens.ContainsKey(option.Name))
                {
                    tokens.Add(option.Name, new CliToken(option.Name, CliTokenType.Option, option, CliToken.ImplicitPosition));
                }

                if (option._aliases is not null)
                {
                    foreach (string childAlias in option._aliases)
                    {
                        if (!tokens.ContainsKey(childAlias))
                        {
                            tokens.Add(childAlias, new CliToken(childAlias, CliTokenType.Option, option, CliToken.ImplicitPosition));
                        }
                    }
                }
            }
        }
    }
}