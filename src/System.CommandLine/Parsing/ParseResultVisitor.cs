// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.Diagnostics;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal class ParseResultVisitor : SyntaxVisitor
    {
        private readonly Parser _parser;
        private readonly TokenizeResult _tokenizeResult;
        private readonly string? _rawInput;

        private readonly DirectiveCollection _directives = new DirectiveCollection();
        private readonly List<string> _unparsedTokens;
        private readonly List<string> _unmatchedTokens;
        private readonly List<ParseError> _errors;

        private RootCommandResult? _rootCommandResult;
        private CommandResult? _innermostCommandResult;

        public ParseResultVisitor(
            Parser parser,
            TokenizeResult tokenizeResult,
            IReadOnlyCollection<Token> unparsedTokens,
            IReadOnlyCollection<Token> unmatchedTokens,
            IReadOnlyCollection<ParseError> parseErrors,
            string? rawInput)
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

            _innermostCommandResult!
                .Children
                .Add(commandResult);

            _innermostCommandResult = commandResult;
        }

        protected override void VisitCommandArgumentNode(
            CommandArgumentNode argumentNode)
        {
            var commandResult = _innermostCommandResult;

            var argumentResult =
                commandResult!.Children
                             .OfType<ArgumentResult>()
                             .SingleOrDefault(r => r.Symbol == argumentNode.Argument);

            if (argumentResult is null)
            {
                argumentResult =
                    new ArgumentResult(
                        argumentNode.Argument,
                        commandResult);
                
                commandResult.Children.Add(argumentResult);
            }

            argumentResult.AddToken(argumentNode.Token);
            commandResult.AddToken(argumentNode.Token);
        }

        protected override void VisitOptionNode(OptionNode optionNode)
        {
            if (_innermostCommandResult!.Children.ResultFor(optionNode.Option) is null)
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

            var optionResult = _innermostCommandResult!.Children.ResultFor(option);

            var argument = argumentNode.Argument;

            var argumentResult =
                (ArgumentResult?)optionResult!.Children.ResultFor(argument);

            if (argumentResult is null)
            {
                argumentResult =
                    new ArgumentResult(
                        argumentNode.Argument,
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
            var helpWasRequested =
                _innermostCommandResult
                    ?.Children
                    .Any(o => o.Symbol is HelpOption) == true;

            if (helpWasRequested)
            {
                return;
            }

            ValidateCommandHandler();

            PopulateDefaultValues();

            ValidateCommandResult();

            foreach (var result in _innermostCommandResult!.Children.ToArray())
            {
                switch (result)
                {
                    case ArgumentResult argumentResult:

                        ValidateArgumentResult(argumentResult);

                        break;

                    case OptionResult optionResult:

                        ValidateOptionResult(optionResult);

                        break;
                }
            }
        }

        private void ValidateCommandResult()
        {
            if (_innermostCommandResult!.Command is Command command)
            {
                foreach (var validator in command.Validators)
                {
                    var errorMessage = validator(_innermostCommandResult);

                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        _errors.Add(
                            new ParseError(errorMessage!, _innermostCommandResult));
                    }
                }
            }

            foreach (var option in _innermostCommandResult
                                   .Command
                                   .Options)
            {
                if (option is Option o &&
                    o.Required && 
                    _rootCommandResult!.FindResultFor(o) is null)
                {
                    _errors.Add(
                        new ParseError($"Option '{o.RawAliases.First()}' is required.",
                                       _innermostCommandResult));
                }
            }

            foreach (var symbol in _innermostCommandResult
                                   .Command
                                   .Arguments)
            {
                var arityFailure = ArgumentArity.Validate(
                    _innermostCommandResult,
                    symbol,
                    symbol.Arity.MinimumNumberOfValues,
                    symbol.Arity.MaximumNumberOfValues);

                if (arityFailure != null)
                {
                    _errors.Add(
                        new ParseError(arityFailure.ErrorMessage!, _innermostCommandResult));
                }
            }
        }

        private void ValidateCommandHandler()
        {
            if (!(_innermostCommandResult!.Command is Command cmd) ||
                cmd.Handler != null)
            {
                return;
            }

            if (!cmd.Children.OfType<ICommand>().Any())
            {
                return;
            }

            _errors.Insert(
                0,
                new ParseError(
                    _innermostCommandResult.ValidationMessages.RequiredCommandWasNotProvided(),
                    _innermostCommandResult));
        }

        private void ValidateOptionResult(OptionResult optionResult)
        {
            var argument = optionResult.Option.Argument;

            var arityFailure = ArgumentArity.Validate(
                optionResult,
                argument,
                argument.Arity.MinimumNumberOfValues,
                argument.Arity.MaximumNumberOfValues);

            if (arityFailure != null)
            {
                _errors.Add(
                    new ParseError(arityFailure.ErrorMessage!, optionResult));
            }

            if (optionResult.Option is Option option)
            {
                foreach (var validate in option.Validators)
                {
                    var message = validate(optionResult);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        _errors.Add(new ParseError(message!, optionResult));
                    }
                }
            }

            foreach (var argumentResult in optionResult
                                           .Children
                                           .OfType<ArgumentResult>())
            {
                ValidateArgumentResult(argumentResult);
            }
        }

        private void ValidateArgumentResult(ArgumentResult argumentResult)
        {
            if (argumentResult.Argument is Argument argument)
            {
                var parseError =
                    argumentResult.Parent?.UnrecognizedArgumentError(argument) ??
                    argumentResult.CustomError(argument);

                if (parseError != null)
                {
                    _errors.Add(parseError);
                    return;
                }
            }

            if (argumentResult.GetArgumentConversionResult() is FailedArgumentConversionResult failed)
            {
                _errors.Add(
                    new ParseError(
                        failed.ErrorMessage!,
                        argumentResult));
            }
        }

        private void PopulateDefaultValues()
        {
            var commandResults = _innermostCommandResult!
                .RecurseWhileNotNull(c => c.Parent as CommandResult);

            foreach (var commandResult in commandResults)
            {
                foreach (var symbol in commandResult.Command.Children)
                {
                    var symbolResult = _rootCommandResult!.FindResultFor(symbol);

                    if (symbolResult is null)
                    {
                        switch (symbol)
                        {
                            case Option option when option.Argument.HasDefaultValue:

                                var optionResult = new OptionResult(
                                    option,
                                    null,
                                    commandResult);

                                var childArgumentResult = optionResult.GetOrCreateDefaultArgumentResult(
                                    option.Argument);

                                optionResult.Children.Add(childArgumentResult);
                                commandResult.Children.Add(optionResult);
                                _rootCommandResult.AddToSymbolMap(optionResult);

                                break;

                            case Argument argument when argument.HasDefaultValue:

                                var argumentResult = commandResult.GetOrCreateDefaultArgumentResult(argument);

                                commandResult.Children.Add(argumentResult);
                                _rootCommandResult.AddToSymbolMap(argumentResult);

                                break;
                        }
                    }

                    if (symbolResult is OptionResult o &&
                        o.Option.Argument.ValueType == typeof(bool) &&
                        o.Children.Count == 0)
                    {
                        o.Children.Add(
                            new ArgumentResult(
                                o.Option.Argument,
                                o));
                    }
                }
            }
        }

        public ParseResult Result =>
            new ParseResult(
                _parser,
                _rootCommandResult ?? throw new InvalidOperationException("No root command was found"),
                _innermostCommandResult ?? throw new InvalidOperationException("No command was found"),
                _directives,
                _tokenizeResult,
                _unparsedTokens,
                _unmatchedTokens,
                _errors,
                _rawInput);
    }
}
