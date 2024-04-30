// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal static class StringExtensions
    {
        /*
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
        */
    }

    internal static class Tokenizer
    {
        private const string doubleDash = "--";

        internal static (string? Prefix, string Alias) SplitPrefix(string rawAlias)
        {
            // TODO: I believe this code would be faster and easier to understand with collection patterns
            if (rawAlias[0] == '/')
            {
                return ("/", rawAlias.Substring(1));
            }
            else if (rawAlias[0] == '-')
            {
                if (rawAlias.Length > 1 && rawAlias[1] == '-')
                {
                    return (doubleDash, rawAlias.Substring(2));
                }

                return ("-", rawAlias.Substring(1));
            }

            return (null, rawAlias);
        }

        // TODO: What does the following comment do, and do we need it
        // this method is not returning a Value Tuple or a dedicated type to avoid JITting

        // TODO: When would we ever not infer the rootcommand? This might have been to solve a bug where the first argument could not be the name of the root command.
        internal static void Tokenize(
            IReadOnlyList<string> args,
            CliCommand rootCommand,
            CliConfiguration configuration,
            bool inferRootCommand,
            out List<CliToken> tokens,
            out List<string>? errors)
        {
            tokens = new List<CliToken>(args.Count);

            // Handle exe not being in args
            var rootIsExplicit = FirstArgIsRootCommand(args, rootCommand, inferRootCommand);
            var rootLocation = Location.CreateRoot(rootCommand.Name, rootIsExplicit, rootIsExplicit ? 0 : -1);
            if (!rootIsExplicit) // If it is explicit it will be added in the normal handling loop
            {
                tokens.Add(Command(rootCommand.Name, rootCommand, rootLocation));
            }

            var maxSkippedPositions = configuration.PreProcessedLocations is null
                                    || !configuration.PreProcessedLocations.Any()
                                        ? 0
                                        : configuration.PreProcessedLocations.Max(x => x.Index);


            var validTokens = GetValidTokens(rootCommand);
            var newErrors = MapTokens(args,
                      rootLocation,
                      maxSkippedPositions,
                      rootCommand,
                      validTokens,
                      configuration,
                      false,
                      tokens);

            errors = newErrors;

            static List<string>? MapTokens(IReadOnlyList<string> args,
                                           Location location,
                                           int maxSkippedPositions,
                                           CliCommand currentCommand,
                                           Dictionary<string, (CliSymbol Symbol, CliTokenType TokenType)> validTokens,
                                           CliConfiguration configuration,
                                           bool foundDoubleDash,
                                           List<CliToken> tokens)
            {
                List<string>? errors = null;
                var previousOptionWasClosed = false;

                for (var i = 0; i < args.Count; i++)
                {
                    var arg = args[i];

                    if (i <= maxSkippedPositions
                        && configuration.PreProcessedLocations is not null
                        && configuration.PreProcessedLocations.Any(x => x.Index == i))
                    {
                        continue;
                    }

                    if (foundDoubleDash)
                    {
                        // everything after the double dash is added as an argument
                        tokens.Add(CommandArgument(arg, currentCommand!, Location.FromOuterLocation(arg, i, location)));
                        continue;
                    }

                    if (arg == doubleDash)
                    {
                        tokens.Add(DoubleDash(i, Location.FromOuterLocation(arg, i, location)));
                        foundDoubleDash = true;
                        continue;
                    }

                    // TODO: Figure out a place to put this test, or at least the prefix, somewhere not hard-coded
                    if (configuration.ResponseFileTokenReplacer is not null &&
                        arg.StartsWith("@"))
                    {
                        var responseName = arg.Substring(1);
                        var (insertArgs, insertErrors) = configuration.ResponseFileTokenReplacer(responseName);
                        // TODO: Handle errors
                        if (insertArgs is not null && insertArgs.Any())
                        {
                            var innerLocation = Location.CreateResponse(responseName, i, location);
                            var newErrors = MapTokens(insertArgs, innerLocation, 0, currentCommand,
                                validTokens, configuration, foundDoubleDash, tokens);
                        }
                        continue;
                    }

                    if (TryGetSymbolAndTokenType(validTokens,arg, out var symbol, out var tokenType))
                    {
                        // This test and block is to handle the case `-x -x` where -x takes a string arg and "-x" is the value. Normal 
                        // option argument parsing is handled as all other arguments, because it is not a found token.
                        if (PreviousTokenIsAnOptionExpectingAnArgument(out var option, tokens, previousOptionWasClosed))
                        {
                            tokens.Add(OptionArgument(arg, option!, Location.FromOuterLocation(arg, i, location)));
                            continue;
                        }
                        else
                        {
                            currentCommand = AddToken(currentCommand, tokens, ref validTokens, arg,
                                Location.FromOuterLocation(arg, i, location), tokenType, symbol);
                            previousOptionWasClosed = false;
                        }
                    }
                    else
                    {
                        if (TrySplitIntoSubtokens(arg, out var first, out var rest) &&
                            TryGetSymbolAndTokenType(validTokens, first, out var subSymbol, out var subTokenType) &&
                             subTokenType == CliTokenType.Option)
                        {
                            CliOption option = (CliOption)subSymbol!;
                            tokens.Add(Option(first, option, Location.FromOuterLocation(first, i, location)));

                            if (rest is not null)
                            {
                                tokens.Add(Argument(rest, Location.FromOuterLocation(rest, i, location, first.Length + 1)));
                            }
                        }
                        else if (!configuration.EnablePosixBundling ||
                                 !CanBeUnbundled(arg, tokens) ||
                                 !TryUnbundle(arg.AsSpan(1), Location.FromOuterLocation(arg, i, location), validTokens, tokens))
                        {
                            tokens.Add(Argument(arg, Location.FromOuterLocation(arg, i, location)));
                        }
                    }
                }

                return errors;
            }

            static bool TryGetSymbolAndTokenType(Dictionary<string, (CliSymbol Symbol, CliTokenType TokenType)> validTokens,
                                                 string arg,
                                                 [NotNullWhen(true)] out CliSymbol? symbol,
                                                 out CliTokenType tokenType)
            {
                if (validTokens.TryGetValue(arg, out var t))
                {
                    symbol = t.Symbol;
                    tokenType = t.TokenType;
                    return true;
                }
                symbol = null;
                tokenType = 0;
                return false;
            }

            static bool CanBeUnbundled(string arg, List<CliToken> tokenList)
                => arg.Length > 2
                    && arg[0] == '-'
                    && arg[1] != '-'// don't check for "--" prefixed args
                    && arg[2] != ':' && arg[2] != '=' // handled by TrySplitIntoSubtokens
                    && !PreviousTokenIsAnOptionExpectingAnArgument(out _, tokenList, false);

            static bool TryUnbundle(ReadOnlySpan<char> alias,
                                    Location outerLocation,
                                    Dictionary<string, (CliSymbol Symbol, CliTokenType TokenType)> validTokens,
                                    List<CliToken> tokenList)
            {
                int tokensBefore = tokenList.Count;
                // TODO: Determine if these pointers are helping us enough for complexity. I do not see how it works, but changing it broke it.
                string candidate = new('-', 2); // mutable string used to avoid allocations
                unsafe
                {
                    fixed (char* pCandidate = candidate)
                    {
                        for (int i = 0; i < alias.Length; i++)
                        {
                            if (alias[i] == ':' || alias[i] == '=')
                            {
                                string value = alias.Slice(i + 1).ToString();
                                tokenList.Add(Argument(value,
                                    Location.FromOuterLocation(value, outerLocation.Index, outerLocation, i + 1)));
                                return true;
                            }

                            pCandidate[1] = alias[i];
                            if (!validTokens.TryGetValue(candidate, out var found))
                            {
                                if (tokensBefore != tokenList.Count && tokenList[tokenList.Count - 1].Type == CliTokenType.Option)
                                {
                                    // Invalid_char_in_bundle_causes_rest_to_be_interpreted_as_value
                                    string value = alias.Slice(i).ToString();
                                    tokenList.Add(Argument(value,
                                        Location.FromOuterLocation(value, outerLocation.Index, outerLocation, i)));
                                    return true;
                                }

                                return false;
                            }

                            tokenList.Add(new CliToken(candidate, found.TokenType, found.Symbol,
                                Location.FromOuterLocation(candidate, outerLocation.Index, outerLocation, i + 1)));

                            if (i != alias.Length - 1 && ((CliOption)found.Symbol).Greedy)
                            {
                                int index = i + 1;
                                if (alias[index] == ':' || alias[index] == '=')
                                {
                                    index++; // Last_bundled_option_can_accept_argument_with_colon_separator
                                }

                                string value = alias.Slice(index).ToString();
                                tokenList.Add(Argument(value, Location.FromOuterLocation(value, outerLocation.Index, outerLocation, index)));
                                return true;
                            }
                        }
                    }
                }

                return true;
            }

            static bool PreviousTokenIsAnOptionExpectingAnArgument(out CliOption? option, List<CliToken> tokenList, bool previousOptionWasClosed)
            {
                if (tokenList.Count > 1)
                {
                    var token = tokenList[tokenList.Count - 1];

                    if (token.Type == CliTokenType.Option)// && !previousOptionWasClosed)
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

            static CliCommand AddToken(CliCommand currentCommand,
                                            List<CliToken> tokenList,
                                            ref Dictionary<string, (CliSymbol Symbol, CliTokenType TokenType)> validTokens,
                                            string arg,
                                            Location location,
                                            CliTokenType tokenType,
                                            CliSymbol symbol)
            {
                //var location = Location.FromOuterLocation(outerLocation, argPosition, arg.Length);
                switch (tokenType)
                {
                    case CliTokenType.Option:
                        var option = (CliOption)symbol!;
                        tokenList.Add(Option(arg, option, location));
                        break;

                    case CliTokenType.Command:
                        // All arguments are initially classified as commands because they might be
                        CliCommand cmd = (CliCommand)symbol!;
                        if (cmd != currentCommand)
                        {
                            currentCommand = cmd;
                            // TODO: In the following determine how the cmd could be RootCommand AND the cmd not equal currentCmd. This looks like it would always be true.. If it is a massive side case, is it important not to double the ValidTokens call?
                            if (true)  // cmd != rootCommand)
                            {
                                validTokens = GetValidTokens(cmd); // config contains Directives, they are allowed only for RootCommand
                            }
                            tokenList.Add(Command(arg, cmd, location));
                        }
                        else
                        {
                            tokenList.Add(Argument(arg, location));
                        }

                        break;
                }
                return currentCommand;
            }
        }

        private static bool FirstArgIsRootCommand(IReadOnlyList<string> args, CliCommand rootCommand, bool inferRootCommand)
        {
            if (args.Count > 0)
            {
                if (inferRootCommand && args[0] == CliExecutable.ExecutablePath)
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

        // TODO: Naming rules - sub-tokens has a dash and thus should be SubToken
        private static bool TrySplitIntoSubtokens(
            string arg,
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

        private static Dictionary<string, (CliSymbol Symbol, CliTokenType TokenType)> GetValidTokens(CliCommand command)
        {
            Dictionary<string, (CliSymbol Symbol, CliTokenType TokenType)> tokens = new(StringComparer.Ordinal);

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

            // TODO: Be sure recursive/global options are handled in the Initialize of Help (add to all)
            return tokens;

            static void AddCommandTokens(Dictionary<string, (CliSymbol Symbol, CliTokenType TokenType)> tokens, CliCommand cmd)
            {
                tokens.Add(cmd.Name, (cmd, CliTokenType.Command));

                if (cmd._aliases is not null)
                {
                    foreach (string childAlias in cmd._aliases)
                    {
                        tokens.Add(childAlias, (cmd, CliTokenType.Command));
                    }
                }
            }

            static void AddOptionTokens(Dictionary<string, (CliSymbol Symbol, CliTokenType TokenType)> tokens, CliOption option)
            {
                if (!tokens.ContainsKey(option.Name))
                {
                    tokens.Add(option.Name, (option, CliTokenType.Option));
                }

                if (option._aliases is not null)
                {
                    foreach (string childAlias in option._aliases)
                    {
                        if (!tokens.ContainsKey(childAlias))
                        {
                            tokens.Add(childAlias, (option, CliTokenType.Option));
                        }
                    }
                }
            }
        }

        private static CliToken GetToken(string? value, CliTokenType tokenType, CliSymbol? symbol, Location location)
            => new(value, tokenType, symbol, location);

        private static CliToken Argument(string arg, Location location)
            => GetToken(arg, CliTokenType.Argument, default, location);

        private static CliToken CommandArgument(string arg, CliCommand command, Location location)
            => GetToken(arg, CliTokenType.Argument, command, location);

        private static CliToken OptionArgument(string arg, CliOption option, Location location)
            => GetToken(arg, CliTokenType.Argument, option, location);

        private static CliToken Command(string arg, CliCommand cmd, Location location)
            => GetToken(arg, CliTokenType.Command, cmd, location);

        private static CliToken Option(string arg, CliOption option, Location location)
            => GetToken(arg, CliTokenType.Option, option, location);

        // TODO: Explore whether double dash should track its command
        private static CliToken DoubleDash(int i, Location location)
            => GetToken(doubleDash, CliTokenType.DoubleDash, default, location);

    }


}