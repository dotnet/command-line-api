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

            bool hasCaptureRemainingArgs = HasCaptureRemainingArguments(currentCommand);
            int currentArgIndex = 0;
            int currentArgCount = 0;
            Option? activeOption = null;
            int activeOptionArgCount = 0;

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

                if (hasCaptureRemainingArgs && IsMidCapture())
                {
                    activeOption = null;
                    activeOptionArgCount = 0;
                    currentArgCount++;
                    tokenList.Add(CommandArgument(arg, currentCommand));
                    continue;
                }

                if (!foundDoubleDash &&
                    arg == "--")
                {
                    activeOption = null;
                    activeOptionArgCount = 0;
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

                if (hasCaptureRemainingArgs && IsCapturingNonArgumentToken())
                {
                    activeOption = null;
                    activeOptionArgCount = 0;
                    currentArgCount++;
                    tokenList.Add(CommandArgument(arg, currentCommand));
                    continue;
                }

                if (knownTokens.TryGetValue(arg, out var token))
                {
                    if (PreviousTokenIsAnOptionExpectingAnArgument(out var option))
                    {
                        tokenList.Add(OptionArgument(arg, option!));
                        RecordArgument(arg);
                    }
                    else
                    {
                        switch (token.Type)
                        {
                            case TokenType.Option:
                                Option matchedOpt = (Option)token.Symbol!;
                                tokenList.Add(Option(arg, matchedOpt));
                                RecordOption(matchedOpt);
                                break;

                            case TokenType.Command:
                                Command cmd = token.Symbol is null
                                    ? currentCommand._subcommands!.GetLazy(arg)
                                    : (Command)token.Symbol;
                                if (cmd != currentCommand)
                                {
                                    if (cmd != rootCommand)
                                    {
                                        knownTokens = cmd.ValidTokens(); // config contains Directives, they are allowed only for RootCommand
                                    }
                                    currentCommand = cmd;
                                    ResetCaptureState(currentCommand);
                                    tokenList.Add(Command(arg, cmd));
                                }
                                else
                                {
                                    bool isRootInitialToken = tokenList.Count == 0;
                                    tokenList.Add(Argument(arg));
                                    if (!isRootInitialToken)
                                    {
                                        RecordArgument(arg);
                                    }
                                }

                                break;
                        }
                    }
                }
                else if (arg.TrySplitIntoSubtokens(out var first, out var rest) &&
                         knownTokens.TryGetValue(first, out var subtoken) &&
                         subtoken.Type == TokenType.Option)
                {
                    Option splitOpt = (Option)subtoken.Symbol!;
                    tokenList.Add(Option(first, splitOpt));
                    RecordOption(splitOpt);

                    if (rest is not null)
                    {
                        tokenList.Add(Argument(rest));
                        RecordArgument(rest);
                    }
                }
                else if (!configuration.EnablePosixBundling ||
                         !CanBeUnbundled(arg) ||
                         !TryUnbundle(arg.AsSpan(1), i))
                {
                    tokenList.Add(Argument(arg));
                    RecordArgument(arg);
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
                Command commandBefore = currentCommand;
                Dictionary<string, Token> knownTokensBefore = knownTokens;
                bool hasCaptureRemainingArgsBefore = hasCaptureRemainingArgs;
                int currentArgIndexBefore = currentArgIndex;
                int currentArgCountBefore = currentArgCount;
                Option? activeOptionBefore = activeOption;
                int activeOptionArgCountBefore = activeOptionArgCount;

                Span<char> candidate = ['-', '-'];
                for (int i = 0; i < alias.Length; i++)
                {
                    if (alias[i] == ':' || alias[i] == '=')
                    {
                        string val = alias.Slice(i + 1).ToString();
                        tokenList.Add(new Token(val, TokenType.Argument, default, argumentIndex));
                        RecordArgument(val);
                        return true;
                    }

                    candidate[1] = alias[i];
                    if (!knownTokens.TryGetValue(candidate.ToString(), out Token? found))
                    {
                        if (tokensBefore != tokenList.Count && tokenList[tokenList.Count - 1].Type == TokenType.Option)
                        {
                            // Invalid_char_in_bundle_causes_rest_to_be_interpreted_as_value
                            string val = alias.Slice(i).ToString();
                            tokenList.Add(new Token(val, TokenType.Argument, default, argumentIndex));
                            RecordArgument(val);
                            return true;
                        }

                        if (tokenList.Count > tokensBefore)
                        {
                            RestoreBundleState();
                        }

                        return false;
                    }

                    if (found.Type == TokenType.Command)
                    {
                        if (i != alias.Length - 1)
                        {
                            RestoreBundleState();
                            return false;
                        }

                        Command cmd = found.Symbol is null
                            ? currentCommand._subcommands!.GetLazy(found.Value)
                            : (Command)found.Symbol;

                        if (cmd != currentCommand)
                        {
                            if (cmd != rootCommand)
                            {
                                knownTokens = cmd.ValidTokens();
                            }
                            currentCommand = cmd;
                            ResetCaptureState(currentCommand);
                            tokenList.Add(new Token(found.Value, TokenType.Command, cmd, argumentIndex));
                        }
                        else
                        {
                            tokenList.Add(new Token(found.Value, TokenType.Argument, default, argumentIndex));
                            RecordArgument(found.Value);
                        }
                    }
                    else
                    {
                        Option opt = (Option)found.Symbol!;
                        tokenList.Add(new Token(found.Value, found.Type, opt, argumentIndex));
                        RecordOption(opt);

                        if (i != alias.Length - 1 && opt.Greedy)
                        {
                            int index = i + 1;
                            if (alias[index] == ':' || alias[index] == '=')
                            {
                                index++; // Last_bundled_option_can_accept_argument_with_colon_separator
                            }
                            string val = alias.Slice(index).ToString();
                            tokenList.Add(new Token(val, TokenType.Argument, default, argumentIndex));
                            RecordArgument(val);
                            return true;
                        }
                    }
                }

                return true;

                void RestoreBundleState()
                {
                    if (tokenList.Count > tokensBefore)
                    {
                        tokenList.RemoveRange(tokensBefore, tokenList.Count - tokensBefore);
                    }

                    currentCommand = commandBefore;
                    knownTokens = knownTokensBefore;
                    hasCaptureRemainingArgs = hasCaptureRemainingArgsBefore;
                    currentArgIndex = currentArgIndexBefore;
                    currentArgCount = currentArgCountBefore;
                    activeOption = activeOptionBefore;
                    activeOptionArgCount = activeOptionArgCountBefore;
                }
            }

            void ResetCaptureState(Command cmd)
            {
                hasCaptureRemainingArgs = HasCaptureRemainingArguments(cmd);
                currentArgIndex = 0;
                currentArgCount = 0;
                activeOption = null;
                activeOptionArgCount = 0;
            }

            void RecordOption(Option opt)
            {
                if (!hasCaptureRemainingArgs)
                {
                    return;
                }

                activeOption = opt;
                activeOptionArgCount = 0;
            }

            void RecordArgument(string val)
            {
                if (!hasCaptureRemainingArgs)
                {
                    return;
                }

                if (activeOption is not null)
                {
                    Argument optArg = activeOption.Argument;
                    if (activeOptionArgCount < optArg.Arity.MaximumNumberOfValues &&
                        !(optArg.IsBoolean() && !bool.TryParse(val, out _)))
                    {
                        activeOptionArgCount++;
                        if (!activeOption.AllowMultipleArgumentsPerToken)
                        {
                            activeOption = null;
                            activeOptionArgCount = 0;
                        }
                        return;
                    }

                    activeOption = null;
                    activeOptionArgCount = 0;
                }

                IList<Argument> cmdArgs = currentCommand.Arguments;
                while (currentArgIndex < cmdArgs.Count &&
                       currentArgCount >= cmdArgs[currentArgIndex].Arity.MaximumNumberOfValues)
                {
                    currentArgCount = 0;
                    currentArgIndex++;
                }

                if (currentArgIndex < cmdArgs.Count)
                {
                    currentArgCount++;
                }
            }

            bool IsMidCapture()
            {
                if (tokenList.Count == 0 || activeOption is not null)
                {
                    return false;
                }

                IList<Argument> cmdArgs = currentCommand.Arguments;
                while (currentArgIndex < cmdArgs.Count &&
                       currentArgCount >= cmdArgs[currentArgIndex].Arity.MaximumNumberOfValues)
                {
                    currentArgCount = 0;
                    currentArgIndex++;
                }

                return currentArgIndex < cmdArgs.Count &&
                       cmdArgs[currentArgIndex].CaptureRemainingTokens &&
                       currentArgCount > 0;
            }

            bool IsCapturingNonArgumentToken()
            {
                if (tokenList.Count == 0 || PreviousTokenIsAnOptionExpectingAnArgument(out _))
                {
                    return false;
                }

                IList<Argument> cmdArgs = currentCommand.Arguments;
                while (currentArgIndex < cmdArgs.Count &&
                       currentArgCount >= cmdArgs[currentArgIndex].Arity.MaximumNumberOfValues)
                {
                    currentArgCount = 0;
                    currentArgIndex++;
                }

                return currentArgIndex < cmdArgs.Count &&
                       cmdArgs[currentArgIndex].CaptureRemainingTokens &&
                       (currentArgIndex > 0 || currentArgCount > 0);
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

                if (args[0].TrySplitIntoSubtokens(out var first, out _) &&
                    rootCommand.ValidTokens().TryGetValue(first, out var token) &&
                    token.Type == TokenType.Option)
                {
                    return false;
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

        private static bool HasCaptureRemainingArguments(Command command)
        {
            if (!command.HasArguments)
            {
                return false;
            }

            IList<Argument> arguments = command.Arguments;
            for (int i = 0; i < arguments.Count; i++)
            {
                if (arguments[i].CaptureRemainingTokens)
                {
                    return true;
                }
            }

            return false;
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

            if (command._subcommands is { Count: > 0 } subCommands)
            {
                for (int i = 0; i < subCommands.Count; i++)
                {
                    subCommands.AddEntryTokens(tokens, i);
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