// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class ParseResult
    {
        private readonly List<ParseError> _errors;

        internal ParseResult(
            Parser parser,
            CommandResult rootCommandResult,
            CommandResult commandResult,
            IDirectiveCollection directives,
            TokenizeResult tokenizeResult,
            IReadOnlyCollection<string> unparsedTokens,
            IReadOnlyCollection<string> unmatchedTokens,
            List<ParseError> errors = null,
            string rawInput = null)
        {
            Parser = parser;
            RootCommandResult = rootCommandResult;
            CommandResult = commandResult;
            Directives = directives;

            // skip the root command
            Tokens = tokenizeResult.Tokens.Skip(1).ToArray();

            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;

            RawInput = rawInput;

            _errors = errors ?? new List<ParseError>();

            if (parser.Configuration.RootCommand.TreatUnmatchedTokensAsErrors)
            {
                _errors.AddRange(
                    unmatchedTokens.Select(token =>
                                               new ParseError(parser.Configuration.ValidationMessages.UnrecognizedCommandOrArgument(token))));
            }

            if (!CommandLineConfiguration.UseNewParser)
            {
                AddImplicitOptionsAndCheckForErrors();
            }
        }

        public CommandResult CommandResult { get; }

        public Parser Parser { get; }

        public CommandResult RootCommandResult { get; }

        public IReadOnlyCollection<ParseError> Errors => _errors;

        public IDirectiveCollection Directives { get; }

        public IReadOnlyList<Token> Tokens { get; }

        public IReadOnlyCollection<string> UnmatchedTokens { get; }

        internal string RawInput { get; }

        public IReadOnlyCollection<string> UnparsedTokens { get; }

        [Obsolete]
        private void AddImplicitOptionsAndCheckForErrors()
        {
            foreach (var result in RootCommandResult.AllSymbolResults().ToArray())
            {
                if (result is CommandResult commandResult)
                {
                    foreach (var symbol in commandResult.Command.Children)
                    {
                        if (commandResult.Children[symbol.Name] == null)
                        {
                            foreach (var argument in symbol.Arguments())
                            {
                                if (argument.HasDefaultValue)
                                {
                                    switch (symbol)
                                    {
                                        case IOption option:
                                            commandResult.AddImplicitOption(option);
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    if (!commandResult.IsArgumentLimitReached)
                    {
                        foreach (var argument in commandResult.Symbol.Arguments())
                        {
                            if (argument.HasDefaultValue)
                            {
                                var defaultValue = argument.GetDefaultValue();

                                if (defaultValue is string stringArg)
                                {
                                    commandResult.TryTakeToken(new Token(stringArg, TokenType.Argument));
                                }
                                else
                                {
                                    commandResult.UseDefaultValueFor(argument, true);
                                }
                            }
                        }
                    }
                }

                var errors = result.Validate();

                _errors.AddRange(errors);
            }

            if (CommandResult.Command is Command cmd &&
                cmd.Handler == null && 
                cmd.Children.OfType<ICommand>().Any())
            {
                _errors.Insert(0,
                               new ParseError(
                                   CommandResult.ValidationMessages.RequiredCommandWasNotProvided(),
                                   CommandResult));
            }
        }

        public object ValueForOption(
            string alias) =>
            ValueForOption<object>(alias);

        public T ValueForOption<T>(
            string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
            }

            if (this[alias] is OptionResult optionResult)
            {
                return optionResult.GetValueOrDefault<T>();
            }
            else
            {
                return default;
            }
        }

        public SymbolResult this[string alias] => CommandResult.Children[alias];

        public override string ToString() => $"{nameof(ParseResult)}: {this.Diagram()}";

        public OptionResult FindResultFor(IOption option) =>
            RootCommandResult
                .AllSymbolResults()
                .OfType<OptionResult>()
                .FirstOrDefault(s => s.Symbol == option);
    }
}
