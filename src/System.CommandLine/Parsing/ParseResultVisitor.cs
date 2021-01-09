// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal class ParseResultVisitor : SyntaxVisitor
    {
        private readonly Parser _parser;
        private readonly TokenizeResult _tokenizeResult;
        private readonly string? _rawInput;

        private readonly DirectiveCollection _directives = new DirectiveCollection();
        private readonly List<Token> _unparsedTokens;
        private readonly List<Token> _unmatchedTokens;
        private readonly List<ParseError> _errors;

        private RootCommandResult? _rootCommandResult;
        private CommandResult? _innermostCommandResult;

        public ParseResultVisitor(
            Parser parser,
            TokenizeResult tokenizeResult,
            List<Token> unparsedTokens,
            List<Token> unmatchedTokens,
            IReadOnlyCollection<ParseError> parseErrors,
            string? rawInput)
        {
            _parser = parser;
            _tokenizeResult = tokenizeResult;
            _unparsedTokens = unparsedTokens;
            _unmatchedTokens = unmatchedTokens;
            _rawInput = rawInput;

            _errors = new List<ParseError>(_tokenizeResult.Errors.Count + parseErrors.Count);

            foreach(var error in _tokenizeResult.Errors)
            {
                _errors.Add(new ParseError(error.Message));
            }

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
                commandResult!.Children.ResultFor(argumentNode.Argument);

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
            _unmatchedTokens.Add(node.Token);
        }

        protected override void Stop(SyntaxNode node)
        {
            for (var i = 0; i < _innermostCommandResult!.Children.Count; i++)
            {
                if (_innermostCommandResult!.Children[i].Symbol is HelpOption)
                {
                    return;
                }
            }

            ValidateCommandHandler();

            PopulateDefaultValues();

            ValidateCommandResult();

            foreach (var optionResult in _rootCommandResult!.AllOptionResults)
            {
                ValidateAndConvertOptionResult(optionResult);
            }

            var argumentResults = _rootCommandResult!
                                  .AllArgumentResults
                                  .ToList();

            if (argumentResults.Count > 0)
            {
                var arguments = _innermostCommandResult!.Command.Arguments;

                for (var i = 0; i < arguments.Count; i++)
                {
                    if (argumentResults.Count == i)
                    {
                        var nextArgument = arguments[i];
                        var nextArgumentResult = new ArgumentResult(
                            nextArgument,
                            _innermostCommandResult);

                        var previousArgumentResult = argumentResults[i - 1];

                        var passedOnTokens = _innermostCommandResult.Tokens.Skip(previousArgumentResult.Tokens.Count);

                        foreach (var token in passedOnTokens)
                        {
                            if (nextArgumentResult.IsArgumentLimitReached)
                            {
                                break;
                            }

                            nextArgumentResult.AddToken(token);
                        }

                        argumentResults.Add(nextArgumentResult);

                        previousArgumentResult.Parent!.Children.Add(nextArgumentResult);

                        _rootCommandResult.AddToSymbolMap(nextArgumentResult);
                    }

                    var argumentResult = argumentResults[i];

                    ValidateAndConvertArgumentResult(argumentResult);

                    if (argumentResult.PassedOnTokens is {} &&
                        i == arguments.Count - 1)
                    {
                        _unparsedTokens.AddRange(argumentResult.PassedOnTokens);
                    }
                }
            }
        }

        private void ValidateCommandResult()
        {
            if (_innermostCommandResult!.Command is Command command)
            {
                for (var i = 0; i < command.Validators.Count; i++)
                {
                    var validator = command.Validators[i];
                    var errorMessage = validator(_innermostCommandResult);

                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        _errors.Add(
                            new ParseError(errorMessage!, _innermostCommandResult));
                    }
                }
            }

            var options = _innermostCommandResult.Command.Options;

            for (var i = 0;
                i < options.Count;
                i++)
            {
                var option = options[i];

                if (option is Option o &&
                    o.IsRequired &&
                    _rootCommandResult!.FindResultFor(o) is null)
                {
                    _errors.Add(
                        new ParseError($"Option '{o.Aliases.First()}' is required.",
                                       _innermostCommandResult));
                }
            }

            var arguments = _innermostCommandResult.Command.Arguments;

            for (var i = 0;
                i < arguments.Count;
                i++)
            {
                var symbol = arguments[i];

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

            if (!cmd.Children.HasAnyOfType<ICommand>())
            {
                return;
            }

            _errors.Insert(
                0,
                new ParseError(
                    _innermostCommandResult.ValidationMessages.RequiredCommandWasNotProvided(),
                    _innermostCommandResult));
        }

        private void ValidateAndConvertOptionResult(OptionResult optionResult)
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
                for (var i = 0; i < option.Validators.Count; i++)
                {
                    var validate = option.Validators[i];
                    var message = validate(optionResult);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        _errors.Add(new ParseError(message!, optionResult));
                    }
                }
            }

            for (var i = 0; i < optionResult.Children.Count; i++)
            {
                var result = optionResult.Children[i];
                if (result is ArgumentResult argumentResult)
                {
                    ValidateAndConvertArgumentResult(argumentResult);
                }
            }
        }

        private void ValidateAndConvertArgumentResult(ArgumentResult argumentResult)
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
            CommandResult? commandResult = _innermostCommandResult;

            while (commandResult != null)
            {
                for (var symbolIndex = 0; symbolIndex < commandResult.Command.Children.Count; symbolIndex++)
                {
                    var symbol = commandResult.Command.Children[symbolIndex];
                    var symbolResult = _rootCommandResult!.FindResultForSymbol(symbol);

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

                commandResult = commandResult.Parent as CommandResult;
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
                _unmatchedTokens.ToArray(),
                _errors,
                _rawInput);
    }
}
