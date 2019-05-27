// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
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

        public Parser(params Symbol[] symbols) : this(new CommandLineConfiguration(symbols))
        {
        }

        public Parser() : this(new RootCommand())
        {
        }

        public CommandLineConfiguration Configuration { get; }

        public virtual ParseResult Parse(
            IReadOnlyList<string> arguments,
            string rawInput = null)
        {
            if (CommandLineConfiguration.UseNewParser)
            {
                return new CommandLineParser(Configuration).Parse(arguments, rawInput);
            }

            var tokenizeResult = arguments.Tokenize(Configuration);
            var directives = new DirectiveCollection();
            var unparsedTokens = new Queue<Token>(tokenizeResult.Tokens);
            var allSymbolResults = new List<SymbolResult>();
            var unmatchedTokens = new List<Token>();
            CommandResult rootCommandResult = null;
            CommandResult innermostCommandResult = null;

            IList<IOption> optionQueue = GatherOptions(Configuration.Symbols);

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
                    var symbol =
                        Configuration.Symbols
                                     .SingleOrDefault(o => o.HasAlias(token.Value));

                    if (symbol != null)
                    {
                        var symbolResult = SymbolResult.Create(symbol, token, validationMessages: Configuration.ValidationMessages);

                        rootCommandResult = (CommandResult) symbolResult;

                        allSymbolResults.Add(symbolResult);

                        continue;
                    }
                }

                if (token.Type == TokenType.Directive)
                {
                    var withoutBrackets = token.Value.Substring(1, token.Value.Length - 2);
                    var keyAndValue = withoutBrackets.Split(new[] { ':' }, 2);
                    var key = keyAndValue[0];
                    var value = keyAndValue.Length == 2
                                    ? keyAndValue[1]
                                    : null;

                    directives.Add(key, value);

                    continue;
                }

                var added = false;

                foreach (var topLevelSymbol in Enumerable.Reverse(allSymbolResults))
                {
                    var symbolForToken = topLevelSymbol.TryTakeToken(token);

                    if (symbolForToken != null)
                    {
                        allSymbolResults.Add(symbolForToken);
                        added = true;

                        if (symbolForToken is CommandResult command)
                        {
                            ProcessImplicitTokens();
                            innermostCommandResult = command;
                        }

                        if (token.Type == TokenType.Option)
                        {
                            var existing = optionQueue.FirstOrDefault(option => option.Name == symbolForToken.Name);

                            if (existing != null)
                            {
                                // we've used this option - don't use it again
                                optionQueue.Remove(existing);
                            }
                        }

                        break;
                    }

                    if (token.Type == TokenType.Argument &&
                        topLevelSymbol.Symbol is ICommand)
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

            return new ParseResult(
                this,
                rootCommandResult,
                innermostCommandResult ?? rootCommandResult,
                directives,
                tokenizeResult,
                unparsedTokens.Select(t => t.Value).ToArray(),
                unmatchedTokens.Select(t => t.Value).ToArray(),
                rawInput: rawInput);

            void ProcessImplicitTokens()
            {
                if (!Configuration.EnablePositionalOptions)
                {
                    return;
                }

                var currentCommand = innermostCommandResult ?? rootCommandResult;

                if (currentCommand == null)
                {
                    return;
                }

                Token[] tokensToAttemptByPosition =
                    Enumerable.Reverse(unmatchedTokens)
                    .TakeWhile(x => x.Type != TokenType.Command)
                    .Reverse()
                    .ToArray();

                foreach (Token token in tokensToAttemptByPosition)
                {
                    var option = optionQueue.FirstOrDefault();
                    if (option != null)
                    {
                        var newToken = new Token(option.RawAliases.First(), TokenType.Option);
                        var optionResult = currentCommand.TryTakeToken(newToken);
                        var optionArgument = optionResult?.TryTakeToken(token);
                        if (optionArgument != null)
                        {
                            optionQueue.RemoveAt(0);
                            allSymbolResults.Add(optionResult);
                            if (optionResult != optionArgument)
                            {
                                allSymbolResults.Add(optionArgument);
                            }
                            unmatchedTokens.Remove(token);
                        }
                        else if (optionResult != null)
                        {
                            currentCommand.Children.Remove(optionResult);
                        }
                    }
                }
            }
        }

        protected IList<IOption> GatherOptions(ISymbolSet symbols)
        {
            var optionList = new List<IOption>();

            foreach (var symbol in symbols)
            {
                if (symbol is IOption option)
                {
                    var arity = option.Argument.Arity;

                    if (arity.MaximumNumberOfValues == 1 &&
                        arity.MinimumNumberOfValues == 1) // Exactly One
                    {
                        optionList.Add(option);
                    }
                }

                if (symbol is ICommand command)
                {
                    optionList.AddRange(GatherOptions(command.Children));
                }
            }

            return optionList;
        }
    }
}
