// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal class ParseResultVisitor : SyntaxVisitor
    {
        private readonly Parser _parser;
        private readonly TokenizeResult _tokenizeResult;
        private readonly string _rawInput;

        private readonly DirectiveCollection _directives = new DirectiveCollection();
        private readonly List<string> _unparsedTokens;
        private readonly List<string> _unmatchedTokens;
        private readonly List<ParseError> _errors;

        private RootCommandResult _rootCommandResult;
        private CommandResult _innermostCommandResult;

        public ParseResultVisitor(
            Parser parser,
            TokenizeResult tokenizeResult,
            IReadOnlyCollection<Token> unparsedTokens,
            IReadOnlyCollection<Token> unmatchedTokens,
            IReadOnlyCollection<ParseError> parseErrors,
            string rawInput)
        {
            _parser = parser;
            _tokenizeResult = tokenizeResult;
            _rawInput = rawInput;

            _unparsedTokens = new List<string>();
            if (unparsedTokens?.Count > 0)
            {
                _unparsedTokens.AddRange(unparsedTokens.Select(t => t.Value));
            }

            _unmatchedTokens = new List<string>();
            if (unmatchedTokens?.Count > 0)
            {
                _unmatchedTokens.AddRange(unmatchedTokens.Select(t => t.Value));
            }

            _errors = new List<ParseError>();
            _errors.AddRange(_tokenizeResult.Errors.Select(t => new ParseError(t.Message)));
            _errors.AddRange(parseErrors);
        }

        protected override void VisitRootCommandNode(RootCommandNode rootCommandNode)
        {
            _rootCommandResult = new RootCommandResult(
                rootCommandNode.Command,
                rootCommandNode.Token);
            _rootCommandResult.ValidationMessages = _parser.Configuration.ValidationMessages;

            _innermostCommandResult = _rootCommandResult;
        }

        protected override void VisitCommandNode(CommandNode commandNode)
        {
            var commandResult = new CommandResult(
                commandNode.Command,
                commandNode.Token,
                _innermostCommandResult);

            _innermostCommandResult
                .Children
                .Add(commandResult);

            _innermostCommandResult = commandResult;
        }

        protected override void VisitCommandArgumentNode(
            CommandArgumentNode argumentNode)
        {
            var commandResult = _innermostCommandResult;

            var argumentResult =
                commandResult.Children
                             .OfType<ArgumentResult>()
                             .SingleOrDefault(r => r.Symbol == argumentNode.Argument);

            if (argumentResult == null)
            {
                argumentResult =
                    new ArgumentResult(
                        argumentNode.Argument,
                        argumentNode.Token,
                        commandResult);

                commandResult.Children.Add(argumentResult);
            }

            argumentResult.AddToken(argumentNode.Token);
            commandResult.AddToken(argumentNode.Token);
        }

        protected override void VisitOptionNode(OptionNode optionNode)
        {
            if (_innermostCommandResult.Children.ResultFor(optionNode.Option) == null)
            {
                var optionResult = new OptionResult(
                    optionNode.Option,
                    optionNode.Token,
                    _innermostCommandResult);

                _innermostCommandResult
                    .Children
                    .Add(optionResult);
            }
        }

        protected override void VisitOptionArgumentNode(
            OptionArgumentNode argumentNode)
        {
            var option = argumentNode.ParentOptionNode.Option;

            var optionResult = _innermostCommandResult.Children.ResultFor(option);

            var argument = argumentNode.Argument;

            var argumentResult =
                (ArgumentResult)optionResult.Children.ResultFor(argument);

            if (argumentResult == null)
            {
                argumentResult =
                    new ArgumentResult(
                        argumentNode.Argument,
                        argumentNode.Token,
                        optionResult);
                optionResult.Children.Add(argumentResult);
            }

            argumentResult.AddToken(argumentNode.Token);
            optionResult.AddToken(argumentNode.Token);
        }

        protected override void VisitDirectiveNode(DirectiveNode directiveNode)
        {
            _directives.Add(directiveNode.Name, directiveNode.Value);
        }

        protected override void VisitUnknownNode(SyntaxNode node)
        {
            _unmatchedTokens.Add(node.Token.Value);
        }

        protected override void Stop(SyntaxNode node)
        {
            ValidateCommandHandler(_innermostCommandResult);

            foreach (var commandResult in _innermostCommandResult.RecurseWhileNotNull(c => c.ParentCommandResult))
            {
                foreach (var symbol in commandResult.Command.Children)
                {
                    PopulateDefaultValues(commandResult, symbol);
                }

                ValidateCommand(commandResult);

                foreach (var option in commandResult.Command.Children.OfType<Option>()
                    .Where(o => o.Argument.Arity.MinimumNumberOfValues > 0))
                {
                    var symbolResult = commandResult.Children.ResultFor(option);
                    if (symbolResult == null)
                    {
                        var argument = option.Argument;
                        var optionResult = new OptionResult(
                            option,
                            option.CreateImplicitToken());
                        
                        _errors.Add(new ParseError(
                            optionResult.ValidationMessages.RequiredArgumentMissing(optionResult),
                            optionResult));
                    }
                }

                foreach (var result in commandResult.Children)
                {
                    switch (result)
                    {
                        case ArgumentResult argumentResult:

                            ValidateArgument(argumentResult);

                            break;

                        case OptionResult optionResult:

                            var argument = optionResult.Option.Argument;

                            var arityFailure = ArgumentArity.Validate(
                                optionResult,
                                argument,
                                argument.Arity.MinimumNumberOfValues,
                                argument.Arity.MaximumNumberOfValues);

                            if (arityFailure != null)
                            {
                                _errors.Add(
                                    new ParseError(arityFailure.ErrorMessage, optionResult));
                            }

                            var results = optionResult.Children
                                                      .OfType<ArgumentResult>()
                                                      .ToArray();

                            foreach (var a in results)
                            {
                                ValidateArgument(a);
                            }

                            break;
                    }
                }
            }
        }

        private void ValidateCommand(CommandResult commandResult)
        {
            foreach (var a in commandResult
                              .Command
                              .Arguments)
            {
                if (a is Argument argument)
                {
                    var arityFailure = ArgumentArity.Validate(
                        commandResult,
                        a,
                        a.Arity.MinimumNumberOfValues,
                        a.Arity.MaximumNumberOfValues);

                    if (arityFailure != null)
                    {
                        _errors.Add(
                            new ParseError(arityFailure.ErrorMessage, commandResult));
                    }

                    foreach (var validator in argument.SymbolValidators)
                    {
                        var errorMessage = validator(commandResult);

                        if (!string.IsNullOrWhiteSpace(errorMessage))
                        {
                            _errors.Add(
                                new ParseError(errorMessage, commandResult));
                        }
                    }
                }
            }
        }

        private void ValidateCommandHandler(CommandResult commandResult)
        {
            if (commandResult.Command is Command cmd &&
                cmd.Handler == null &&
                cmd.Children.OfType<ICommand>().Any())
            {
                _errors.Insert(0,
                               new ParseError(
                                   commandResult.ValidationMessages.RequiredCommandWasNotProvided(),
                                   commandResult));
            }
        }

        private void ValidateArgument(ArgumentResult argumentResult)
        {
            var arityFailure = ArgumentArity.Validate(argumentResult);

            if (_errors.Any())
            {
                return;
            }

            if (arityFailure != null)
            {
                _errors.Add(new ParseError(arityFailure.ErrorMessage));
                return;
            }

            if (argumentResult.Argument is Argument argument)
            {
                var parseError = argumentResult.Parent.UnrecognizedArgumentError(argument) ??
                                 argumentResult.Parent.CustomError(argument);

                if (parseError != null)
                {
                    _errors.Add(parseError);
                    return;
                }
            }

            if (argumentResult.ArgumentConversionResult is FailedArgumentConversionResult failed)
            {
                _errors.Add(
                    new ParseError(
                        failed.ErrorMessage,
                        argumentResult));
            }
        }

        private static void PopulateDefaultValues(
            CommandResult parentCommandResult,
            ISymbol symbol)
        {
            var symbolResult = parentCommandResult.Children.ResultFor(symbol);

            if (symbolResult == null)
            {
                switch (symbol)
                {
                    case Option option when option.Argument.HasDefaultValue:

                        var optionResult = new OptionResult(
                            option,
                            option.CreateImplicitToken());

                        var token = new ImplicitToken(
                            optionResult.GetDefaultValueFor(option.Argument),
                            TokenType.Argument);

                        optionResult.Children.Add(
                            new ArgumentResult(
                                option.Argument,
                                token,
                                optionResult));

                        parentCommandResult.Children.Add(optionResult);
                        optionResult.AddToken(token);

                        break;

                    case Argument argument when argument.HasDefaultValue:

                        var implicitToken = new ImplicitToken(argument.GetDefaultValue(), TokenType.Argument);

                        var argumentResult = new ArgumentResult(
                            argument,
                            implicitToken,
                            parentCommandResult);

                        parentCommandResult.Children.Add(argumentResult);
                        parentCommandResult.AddToken(implicitToken);

                        break;
                }
            }

            if (symbolResult is OptionResult o &&
                o.Option.Argument.Type == typeof(bool) &&
                o.Children.Count == 0)
            {
                o.Children.Add(
                    new ArgumentResult(
                        o.Option.Argument,
                        new ImplicitToken(true, TokenType.Argument),
                        o));
            }
        }

        public ParseResult Result =>
            new ParseResult(
                _parser,
                _rootCommandResult,
                _innermostCommandResult,
                _directives,
                _tokenizeResult,
                _unparsedTokens,
                _unmatchedTokens,
                _errors,
                _rawInput
            );
    }
}
