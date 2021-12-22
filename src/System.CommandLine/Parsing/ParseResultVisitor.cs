// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal sealed class ParseResultVisitor
    {
        private readonly Parser _parser;
        private readonly TokenizeResult _tokenizeResult;
        private readonly string? _rawInput;

        private readonly DirectiveCollection _directives = new();
        private List<Token>? _unparsedTokens;
        private readonly List<Token>? _unmatchedTokens;
        private readonly List<ParseError> _errors;

        private readonly Dictionary<Symbol, SymbolResult> _symbolResults = new();

        private readonly List<OptionResult> _optionResults = new();
        private readonly List<ArgumentResult> _argumentResults = new();

        private RootCommandResult? _rootCommandResult;
        private CommandResult? _innermostCommandResult;
        private bool _isHelpRequested;

        public ParseResultVisitor(
            Parser parser,
            TokenizeResult tokenizeResult,
            List<Token>? unparsedTokens,
            List<Token>? unmatchedTokens,
            string? rawInput)
        {
            _parser = parser;
            _tokenizeResult = tokenizeResult;
            _unparsedTokens = unparsedTokens;
            _unmatchedTokens = unmatchedTokens;
            _rawInput = rawInput;
            _errors = new List<ParseError>(_tokenizeResult.Errors.Count);

            for (var i = 0; i < _tokenizeResult.Errors.Count; i++)
            {
                var error = _tokenizeResult.Errors[i];
                _errors.Add(new ParseError(error.Message));
            }
        }

        public void Visit(SyntaxNode node)
        {
            VisitInternal(node);

            Stop();
        }

        private void VisitInternal(SyntaxNode node)
        {
            switch (node)
            {
                case DirectiveNode directiveNode:
                    VisitDirectiveNode(directiveNode);

                    break;

                case RootCommandNode rootCommandNode:
                    VisitRootCommandNode(rootCommandNode);

                    for (var i = 0; i < rootCommandNode.Children.Count; i++)
                    {
                        VisitInternal(rootCommandNode.Children[i]);
                    }

                    break;

                case CommandNode commandNode:
                    VisitCommandNode(commandNode);

                    for (var i = 0; i < commandNode.Children.Count; i++)
                    {
                        VisitInternal(commandNode.Children[i]);
                    }

                    break;

                case OptionNode optionNode:
                    VisitOptionNode(optionNode);

                    for (var i = 0; i < optionNode.Children.Count; i++)
                    {
                        VisitInternal(optionNode.Children[i]);
                    }

                    break;

                case CommandArgumentNode commandArgumentNode:
                    VisitCommandArgumentNode(commandArgumentNode);

                    break;

                case OptionArgumentNode optionArgumentNode:
                    VisitOptionArgumentNode(optionArgumentNode);

                    break;
            }
        }

        private void AddToResult(CommandResult result)
        {
            _innermostCommandResult?.AddChild(result);
            _symbolResults.Add(result.Command, result);
        }

        private void AddToResult(OptionResult result)
        {
            _innermostCommandResult?.AddChild(result);
            if (_symbolResults.TryAdd(result.Option, result))
            {
                _optionResults.Add(result);
            }
        }

        private void AddToResult(ArgumentResult result)
        {
            _innermostCommandResult?.AddChild(result);
            if (_symbolResults.TryAdd(result.Argument, result))
            {
                _argumentResults.Add(result);
            }
        }

        private void VisitRootCommandNode(RootCommandNode rootCommandNode)
        {
            _rootCommandResult = new RootCommandResult(
                rootCommandNode.Command,
                rootCommandNode.Token,
                _symbolResults);

            _rootCommandResult.LocalizationResources = _parser.Configuration.LocalizationResources;

            _innermostCommandResult = _rootCommandResult;
        }

        private void VisitCommandNode(CommandNode commandNode)
        {
            var commandResult = new CommandResult(
                commandNode.Command,
                commandNode.Token,
                _innermostCommandResult);

            AddToResult(commandResult);

            _innermostCommandResult = commandResult;
        }

        private void VisitCommandArgumentNode(CommandArgumentNode argumentNode)
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

        private void VisitOptionNode(OptionNode optionNode)
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

        private void VisitOptionArgumentNode(
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
                optionResult.AddChild(argumentResult);
                _symbolResults.TryAdd(argument, argumentResult);
            }

            argumentResult.AddToken(argumentNode.Token);
            optionResult.AddToken(argumentNode.Token);
        }

        private void VisitDirectiveNode(DirectiveNode directiveNode)
        {
            _directives.Add(directiveNode.Name, directiveNode.Value);
        }

        private void Stop()
        {
            if (_isHelpRequested)
            {
                return;
            }

            ValidateCommandHandler();

            PopulateDefaultValues();

            ValidateCommandResult();

            foreach (var optionResult in _optionResults)
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

                            if (_innermostCommandResult is not null)
                            {
                                nextArgumentResult.AddToken(_innermostCommandResult.Tokens[j]);
                            }
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
                        _unparsedTokens ??= new List<Token>();
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
            var command = _innermostCommandResult!.Command;

            for (var i = 0; i < command.Validators.Count; i++)
            {
                var validator = command.Validators[i];
                var errorMessage = validator(_innermostCommandResult);

                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    AddErrorToResult(_innermostCommandResult, new ParseError(errorMessage!, _innermostCommandResult));
                }
            }

            var commandToCheckForRequiredOptions = command;
            ParentNode? currentNode = null;
            var checkOnlyGlobalOptions = false;

            while (commandToCheckForRequiredOptions is not null)
            {
                var options = commandToCheckForRequiredOptions.Options;

                for (var i = 0; i < options.Count; i++)
                {
                    var option = options[i];

                    if (option.IsRequired &&
                        _rootCommandResult!.FindResultFor(option) is null)
                    {
                        if (checkOnlyGlobalOptions && !option.IsGlobal)
                        {
                            continue;
                        }

                        AddErrorToResult(
                            _innermostCommandResult,
                            new ParseError($"Option '{option.Aliases.First()}' is required.",
                                           _innermostCommandResult));
                    }
                }

                if (currentNode is not null)
                {
                    commandToCheckForRequiredOptions = currentNode.Next?.Symbol as Command;
                    if (commandToCheckForRequiredOptions is null)
                    {
                        currentNode = null;
                    }
                }
                else if (commandToCheckForRequiredOptions.FirstParent is not null)
                {
                    currentNode = commandToCheckForRequiredOptions.FirstParent;
                    commandToCheckForRequiredOptions = currentNode.Symbol as Command;
                    currentNode = currentNode.Next;
                    checkOnlyGlobalOptions = true;
                }
                else
                {
                    break;
                }
            }

            var arguments = command.Arguments;

            for (var i = 0; i < arguments.Count; i++)
            {
                var symbol = arguments[i];

                var arityFailure = ArgumentArity.Validate(
                    _innermostCommandResult,
                    symbol,
                    symbol.Arity.MinimumNumberOfValues,
                    symbol.Arity.MaximumNumberOfValues);

                if (arityFailure is not null)
                {
                    AddErrorToResult(_innermostCommandResult, new ParseError(arityFailure.ErrorMessage!, _innermostCommandResult));
                }
            }
        }

        private void ValidateCommandHandler()
        {
            if (_innermostCommandResult!.Command is not { Handler: null } cmd)
            {
                return;
            }

            if (!cmd.Children.HasAnyOfType<Command>())
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
                AddErrorToResult(optionResult, new ParseError(arityFailure.ErrorMessage!, optionResult));
            }

            if (optionResult.Option.HasValidators)
            {
                for (var i = 0; i < optionResult.Option.Validators.Count; i++)
                {
                    var validate = optionResult.Option.Validators[i];
                    var message = validate(optionResult);

                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        AddErrorToResult(optionResult, new ParseError(message!, optionResult));
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
                        optionResult.AddChild(argResult);
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
                    AddErrorToResult(argumentResult, parseError);
                    return;
                }
            }

            if (argumentResult.GetArgumentConversionResult() is FailedArgumentConversionResult failed 
                and not FailedArgumentConversionArityResult)
            {
                AddErrorToResult(argumentResult, new ParseError(failed.ErrorMessage!, argumentResult));
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

                            if (o.Children.Count == 0 && 
                                o.Option.Argument.ValueType == typeof(bool))
                            {
                                o.AddChild(
                                    new ArgumentResult(o.Option.Argument, o));
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

                                    optionResult.AddChild(childArgumentResult);
                                    commandResult.AddChild(optionResult);
                                    if (_symbolResults.TryAdd(optionResult.Symbol, optionResult))
                                    {
                                        _optionResults.Add(optionResult);
                                    }

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

        private void AddErrorToResult(SymbolResult symbolResult, ParseError parseError)
        {
            symbolResult.ErrorMessage ??= parseError.Message;

            if (symbolResult.Parent is OptionResult optionResult)
            {
                optionResult.ErrorMessage ??= symbolResult.ErrorMessage;
            }

            _errors.Add(parseError);
        }

        public ParseResult GetResult() =>
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
