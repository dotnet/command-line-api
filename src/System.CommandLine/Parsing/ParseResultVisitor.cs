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

        private readonly DirectiveCollection _directives = new();
        private readonly List<Token> _unparsedTokens;
        private readonly List<Token> _unmatchedTokens;
        private readonly List<ParseError> _errors;

        private readonly Dictionary<ISymbol, SymbolResult> _symbolResults = new();

        private readonly List<OptionResult> _optionResults = new();
        private readonly List<ArgumentResult> _argumentResults = new();

        private RootCommandResult? _rootCommandResult;
        private CommandResult? _innermostCommandResult;
        private bool _isHelpRequested;

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

            for (var i = 0; i < _tokenizeResult.Errors.Count; i++)
            {
                var error = _tokenizeResult.Errors[i];
                _errors.Add(new ParseError(error.Message));
            }

            _errors.AddRange(parseErrors);
        }

        private void AddToResult(CommandResult result)
        {
            _innermostCommandResult?.Children.Add(result);
            _symbolResults.Add(result.Command, result);
        }

        private void AddToResult(OptionResult result)
        {
            _innermostCommandResult?.Children.Add(result);
            if (_symbolResults.TryAdd(result.Option, result))
            {
                _optionResults.Add(result);
            }
        }

        private void AddToResult(ArgumentResult result)
        {
            _innermostCommandResult?.Children.Add(result);

            if (_symbolResults.TryAdd(result.Argument, result))
            {
                _argumentResults.Add(result);
            }
        }

        protected override void VisitRootCommandNode(RootCommandNode rootCommandNode)
        {
            _rootCommandResult = new RootCommandResult(
                rootCommandNode.Command,
                rootCommandNode.Token,
                _symbolResults);

            _rootCommandResult.LocalizationResources = _parser.Configuration.LocalizationResources;

            _innermostCommandResult = _rootCommandResult;
        }

        protected override void VisitCommandNode(CommandNode commandNode)
        {
            var commandResult = new CommandResult(
                commandNode.Command,
                commandNode.Token,
                _innermostCommandResult);

            AddToResult(commandResult);

            _innermostCommandResult = commandResult;
        }

        protected override void VisitCommandArgumentNode(CommandArgumentNode argumentNode)
        {
            _symbolResults.TryGetValue(argumentNode.Argument, out var symbolResult);

            if (symbolResult is not ArgumentResult argumentResult)
            {
                argumentResult =
                    new ArgumentResult(
                        argumentNode.Argument,
                        _innermostCommandResult);

                AddToResult(argumentResult);
            }

            argumentResult.AddToken(argumentNode.Token);
            _innermostCommandResult?.AddToken(argumentNode.Token);
        }

        protected override void VisitOptionNode(OptionNode optionNode)
        {
            _symbolResults.TryGetValue(optionNode.Option, out var symbolResult);

            if (symbolResult is not OptionResult)
            {
                if (optionNode.Option is HelpOption)
                {
                    _isHelpRequested = true;
                }

                var optionResult = new OptionResult(
                    optionNode.Option,
                    optionNode.Token,
                    _innermostCommandResult);

                AddToResult(optionResult);
            }
        }

        protected override void VisitOptionArgumentNode(
            OptionArgumentNode argumentNode)
        {
            _symbolResults.TryGetValue(
                argumentNode.ParentOptionNode.Option,
                out var optionResult);

            if (optionResult is not OptionResult)
            {
                return;
            }

            var argument = argumentNode.Argument;

            if (!_symbolResults.TryGetValue(argument, out var argumentResult))
            {
                argumentResult =
                    new ArgumentResult(
                        argumentNode.Argument,
                        optionResult);
                optionResult.Children.Add(argumentResult);
                _symbolResults.TryAdd(argument, argumentResult);
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
            if (_isHelpRequested)
            {
                return;
            }

            ValidateCommandHandler();

            PopulateDefaultValues();

            ValidateCommandResult();

            var optionResults = _symbolResults.Values.OfType<OptionResult>().ToArray();

            // FIX: (Stop) use _optionResults
            foreach (var optionResult in optionResults)
            {
                ValidateAndConvertOptionResult(optionResult);
            }

            if (_argumentResults.Count > 0)
            {
                var arguments = _innermostCommandResult!.Command.Arguments;

                var commandArgumentResultCount = _argumentResults.Count;

                for (var i = 0; i < arguments.Count; i++)
                {
                    // If this is the current last result but there are more arguments, see if we can shift tokens to the next argument
                    if (commandArgumentResultCount == i)
                    {
                        var nextArgument = arguments[i];
                        var nextArgumentResult = new ArgumentResult(
                            nextArgument,
                            _innermostCommandResult);

                        var previousArgumentResult = _argumentResults[i - 1];

                        var passedOnTokensCount = _innermostCommandResult?.Tokens.Count;

                        for (var j = previousArgumentResult.Tokens.Count; j < passedOnTokensCount; j++)
                        {
                            if (nextArgumentResult.IsArgumentLimitReached)
                            {
                                break;
                            }

                            var token = _innermostCommandResult?.Tokens[j];

                            nextArgumentResult.AddToken(token!);
                        }

                        _argumentResults.Add(nextArgumentResult);

                        if (previousArgumentResult.Parent is CommandResult)
                        {
                            AddToResult(nextArgumentResult);
                        }

                        _symbolResults.TryAdd(nextArgumentResult.Symbol, nextArgumentResult);
                    }

                    var argumentResult = _argumentResults[i];

                    ValidateAndConvertArgumentResult(argumentResult);

                    if (argumentResult.PassedOnTokens is { } &&
                        i == arguments.Count - 1)
                    {
                        _unparsedTokens.AddRange(argumentResult.PassedOnTokens);
                    }
                }

                if (_argumentResults.Count > arguments.Count)
                {
                    for (var i = arguments.Count; i < _argumentResults.Count; i++)
                    {
                        var result = _argumentResults[i];

                        if (result.Parent is CommandResult)
                        {
                            ValidateAndConvertArgumentResult(result);
                        }
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

                if (option is Option { IsRequired: true } o && 
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

                if (arityFailure is not null)
                {
                    _errors.Add(
                        new ParseError(arityFailure.ErrorMessage!, _innermostCommandResult));
                }
            }
        }

        private void ValidateCommandHandler()
        {
            if (_innermostCommandResult!.Command is not Command { Handler: null } cmd)
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
                    _innermostCommandResult.LocalizationResources.RequiredCommandWasNotProvided(),
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

            if (arityFailure is { })
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

            if (optionResult.Children.Count == 0)
            {
                if (optionResult.Option.Argument is Argument { HasCustomParser: true })
                {
                    if (optionResult.Option is Option opt)
                    {
                        var argResult = optionResult.GetOrCreateDefaultArgumentResult(opt.Argument);
                        optionResult.Children.Add(argResult);
                        ValidateAndConvertArgumentResult(argResult);
                    }
                }
            }
            else
            {
                for (var i = 0; i < optionResult.Children.Count; i++)
                {
                    var result = optionResult.Children[i];
                    if (result is ArgumentResult argumentResult)
                    {
                        ValidateAndConvertArgumentResult(argumentResult);
                    }
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

                if (parseError is { })
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
            
            while (commandResult is { })
            {
                for (var symbolIndex = 0; symbolIndex < commandResult.Command.Children.Count; symbolIndex++)
                {
                    var symbol = commandResult.Command.Children[symbolIndex];
                    var symbolResult = _rootCommandResult!.FindResultForSymbol(symbol);

                    switch (symbolResult)
                    {
                        case OptionResult o:

                            if (o.Option.Argument.ValueType == typeof(bool) && o.Children.Count == 0)
                            {
                                o.Children.Add(
                                    new ArgumentResult(
                                        o.Option.Argument,
                                        o));
                            }

                            break;

                        case null:
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
                                    _symbolResults.TryAdd(optionResult.Symbol, optionResult);

                                    break;

                                case Argument { HasDefaultValue: true } argument:

                                    var argumentResult = commandResult.GetOrCreateDefaultArgumentResult(argument);
                                    
                                    AddToResult(argumentResult);
                                    break;
                            }

                            break;
                    }
                }

                commandResult = commandResult.Parent as CommandResult;
            }
        }

        public ParseResult Result =>
            new(_parser,
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
