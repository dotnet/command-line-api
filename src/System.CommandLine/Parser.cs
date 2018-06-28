// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine
{
    public class Parser
    {
        public Parser(CommandLineConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Parser(params SymbolDefinition[] symbolDefinitions) : this(new CommandLineConfiguration(symbolDefinitions))
        {
        }

        internal CommandLineConfiguration Configuration { get; }

        public virtual ParseResult Parse(IReadOnlyCollection<string> arguments, string rawInput = null)
        {
            var rawTokens = arguments;  // allow a more user-friendly name for callers of Parse
            var lexResult = NormalizeRootCommand(rawTokens).Lex(Configuration);
            var unparsedTokens = new Queue<Token>(lexResult.Tokens);
            var allSymbols = new List<Symbol>();
            var errors = new List<ParseError>(lexResult.Errors);
            var unmatchedTokens = new List<Token>();
            Command rootCommand = null;
            Command innermostCommand = null;

            IList<OptionDefinition> optionQueue = GatherOptions(Configuration.SymbolDefinitions);

            while (unparsedTokens.Any())
            {
                var token = unparsedTokens.Dequeue();

                if (token.Type == TokenType.EndOfArguments)
                {
                    // stop parsing further tokens
                    break;
                }

                if (token.Type != TokenType.Argument)
                {
                    var symbolDefinition =
                        Configuration.SymbolDefinitions
                                     .SingleOrDefault(o => o.HasAlias(token.Value));

                    if (symbolDefinition != null)
                    {
                        var symbol = allSymbols
                            .LastOrDefault(o => o.HasAlias(token.Value));

                        if (symbol == null)
                        {
                            symbol = Symbol.Create(symbolDefinition, token.Value, validationMessages: Configuration.ValidationMessages);

                            rootCommand = (Command)symbol;
                        }

                        allSymbols.Add(symbol);

                        continue;
                    }
                }

                var added = false;

                foreach (var topLevelSymbol in Enumerable.Reverse(allSymbols))
                {
                    var symbolForToken = topLevelSymbol.TryTakeToken(token);

                    if (symbolForToken != null)
                    {
                        allSymbols.Add(symbolForToken);
                        added = true;
                        if (symbolForToken is Command command)
                        {
                            ProcessImplicitTokens();
                            innermostCommand = command;
                        }

                        if (token.Type == TokenType.Option)
                        {
                            var existing = optionQueue.FirstOrDefault(symdef => symdef.Name == symbolForToken.Name);
                            if (existing != null)
                            {
                                // we've used this option - don't use it again
                                optionQueue.Remove(existing);
                            }
                        }
                        break;
                    }

                    if (token.Type == TokenType.Argument &&
                        topLevelSymbol.SymbolDefinition is CommandDefinition)
                    {
                        break;
                    }
                }

                if (!added)
                {
                    unmatchedTokens.Add(token);
                }
            }

            ProcessImplicitTokens();

            if (Configuration.RootCommandDefinition.TreatUnmatchedTokensAsErrors)
            {
                errors.AddRange(
                    unmatchedTokens.Select(token => new ParseError(Configuration.ValidationMessages.UnrecognizedCommandOrArgument(token.Value))));
            }

            return new ParseResult(
                rootCommand,
                innermostCommand ?? rootCommand,
                rawTokens,
                unparsedTokens.Select(t => t.Value).ToArray(),
                unmatchedTokens.Select(t => t.Value).ToArray(),
                errors,
                rawInput);

            void ProcessImplicitTokens()
            {
                Command currentCommand = innermostCommand ?? rootCommand;
                if (currentCommand == null) return;

                Token[] tokensToAttemptByPosition =
                    Enumerable.Reverse(unmatchedTokens)
                    .TakeWhile(x => x.Type != TokenType.Command)
                    .Reverse()
                    .ToArray();

                foreach (Token token in tokensToAttemptByPosition)
                {
                    SymbolDefinition optionSymdef = optionQueue.FirstOrDefault();
                    if (optionSymdef != null)
                    {
                        var newToken = new Token(optionSymdef.RawAliases.First(), TokenType.Option);
                        Symbol optionSymbol = currentCommand.TryTakeToken(newToken);
                        Symbol optionArgument = optionSymbol?.TryTakeToken(token);
                        if (optionArgument != null)
                        {
                            optionQueue.RemoveAt(0);
                            allSymbols.Add(optionSymbol);
                            if (optionSymbol != optionArgument)
                            {
                                allSymbols.Add(optionArgument);
                            }
                            unmatchedTokens.Remove(token);
                        }
                        else if (optionSymbol != null)
                        {
                            currentCommand.Children.Remove(optionSymbol);
                        }
                    }
                }
            }
        }

        private static IList<OptionDefinition> GatherOptions(SymbolDefinitionSet symbolDefinitions)
        {
            var optionList = new List<OptionDefinition>();
            foreach (SymbolDefinition symbolDefinition in symbolDefinitions)
            {
                if (symbolDefinition is OptionDefinition optionDefinition)
                {
                    var validator = optionDefinition.ArgumentDefinition.Parser.ArityValidator;
                    if (validator?.MaximumNumberOfArguments == 1 &&
                        validator.MinimumNumberOfArguments == 1)    // Exactly One
                    {
                        optionList.Add(optionDefinition);
                    }
                }

                optionList.AddRange(GatherOptions(symbolDefinition.SymbolDefinitions));
            }
            return optionList;
        }

        internal IReadOnlyCollection<string> NormalizeRootCommand(IReadOnlyCollection<string> args)
        {
            var firstArg = args.FirstOrDefault();

            var commandName = Configuration.RootCommandDefinition.Name;

            if (string.Equals(firstArg, commandName, StringComparison.OrdinalIgnoreCase))
            {
                return args;
            }

            if (firstArg != null &&
                firstArg.Contains(Path.DirectorySeparatorChar) &&
                (firstArg.EndsWith(commandName, StringComparison.OrdinalIgnoreCase) ||
                 firstArg.EndsWith($"{commandName}.exe", StringComparison.OrdinalIgnoreCase)))
            {
                args = new[] { commandName }.Concat(args.Skip(1)).ToArray();
            }
            else
            {
                args = new[] { commandName }.Concat(args).ToArray();
            }

            return args;
        }
    }
}
