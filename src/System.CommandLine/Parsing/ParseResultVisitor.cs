﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal sealed class ParseResultVisitor
    {
        private readonly Parser _parser;
        private readonly List<Token> _tokens;
        private readonly string? _rawInput;

        private Dictionary<string, IReadOnlyList<string>>? _directives;
        private List<Token>? _unmatchedTokens;
        private List<ParseError>? _errors;

        private readonly Dictionary<Symbol, SymbolResult> _symbolResults = new();

        private List<OptionResult>? _optionResults;
        private List<ArgumentResult>? _argumentResults;

        private RootCommandResult? _rootCommandResult;
        private CommandResult? _innermostCommandResult;
        private bool _isHelpRequested;

        internal ParseResultVisitor(
            Parser parser,
            List<Token> tokens,
            List<string>? tokenizeErrors,
            List<Token>? unmatchedTokens,
            string? rawInput)
        {
            _parser = parser;
            _tokens = tokens;
            _unmatchedTokens = unmatchedTokens;
            _rawInput = rawInput;

            if (tokenizeErrors is not null)
            {
                _errors = new List<ParseError>(tokenizeErrors.Count);

                for (var i = 0; i < tokenizeErrors.Count; i++)
                {
                    var error = tokenizeErrors[i];
                    _errors.Add(new ParseError(error));
                }
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

                case CommandNode commandNode:
                    if (commandNode.Parent is null)
                    {
                        VisitRootCommandNode(commandNode);
                    }
                    else
                    {
                        VisitCommandNode(commandNode);
                    }

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
                (_optionResults ??= new()).Add(result);
            }
        }

        private void AddToResult(ArgumentResult result)
        {
            _innermostCommandResult?.AddChild(result);
            if (_symbolResults.TryAdd(result.Argument, result))
            {
                (_argumentResults ??= new()).Add(result);
            }
        }

        private void VisitRootCommandNode(CommandNode rootCommandNode)
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

            var token = argumentNode.Token.Symbol is null
                            ? new Token(argumentNode.Token.Value, TokenType.Argument, argumentResult.Argument)
                            : argumentNode.Token;

            argumentResult.AddToken(token);

            _innermostCommandResult?.AddToken(token);
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
            if (_directives is null || !_directives.TryGetValue(directiveNode.Name, out var values))
            {
                values = new List<string>();

                (_directives ??= new()).Add(directiveNode.Name, values);
            }

            if (directiveNode.Value is not null)
            {
                ((List<string>)values).Add(directiveNode.Value);
            }
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

            if (_optionResults is not null)
            {
                foreach (var optionResult in _optionResults)
                {
                    ValidateAndConvertOptionResult(optionResult);
                }
            }

            if (_argumentResults is not null)
            {
                ValidateAndConvertArgumentResults(_innermostCommandResult!.Command.Arguments, _argumentResults);
            }
        }

        private void ValidateAndConvertArgumentResults(IList<Argument> arguments, List<ArgumentResult> argumentResults)
        {
            int commandArgumentResultCount = argumentResults.Count;

            for (var i = 0; i < arguments.Count; i++)
            {
                if (i > 0 && argumentResults.Count > i)
                {
                    var previousArgumentResult = argumentResults[i - 1];

                    var passedOnTokensCount = previousArgumentResult.PassedOnTokens?.Count;

                    if (passedOnTokensCount > 0)
                    {
                        ShiftPassedOnTokensToNextResult(previousArgumentResult, argumentResults[i], passedOnTokensCount);
                    }
                }

                // If this is the current last result but there are more arguments, see if we can shift tokens to the next argument
                if (commandArgumentResultCount == i)
                {
                    var nextArgument = arguments[i];
                    var nextArgumentResult = new ArgumentResult(
                        nextArgument,
                        _innermostCommandResult);

                    var previousArgumentResult = argumentResults[i - 1];

                    var passedOnTokensCount = _innermostCommandResult?.Tokens.Count;

                    ShiftPassedOnTokensToNextResult(previousArgumentResult, nextArgumentResult, passedOnTokensCount);

                    argumentResults.Add(nextArgumentResult);

                    if (previousArgumentResult.Parent is CommandResult)
                    {
                        AddToResult(nextArgumentResult);
                    }

                    _symbolResults.TryAdd(nextArgumentResult.Symbol, nextArgumentResult);
                }

                if (commandArgumentResultCount >= argumentResults.Count)
                {
                    var argumentResult = argumentResults[i];

                    ValidateAndConvertArgumentResult(argumentResult);

                    if (argumentResult.PassedOnTokens is { } &&
                        i == arguments.Count - 1)
                    {
                        _unmatchedTokens ??= new List<Token>();
                        _unmatchedTokens.AddRange(argumentResult.PassedOnTokens);
                    }
                }
            }

            if (argumentResults.Count > arguments.Count)
            {
                for (var i = arguments.Count; i < argumentResults.Count - 1; i++)
                {
                    var result = argumentResults[i];

                    if (result.Parent is CommandResult)
                    {
                        ValidateAndConvertArgumentResult(result);
                    }
                }
            }

            void ShiftPassedOnTokensToNextResult(
                ArgumentResult previous, 
                ArgumentResult next, 
                int? numberOfTokens)
            {
                for (var j = previous.Tokens.Count; j < numberOfTokens; j++)
                {
                    if (next.IsArgumentLimitReached)
                    {
                        break;
                    }

                    if (_innermostCommandResult is not null)
                    {
                        next.AddToken(_innermostCommandResult.Tokens[j]);
                    }
                }
            }
        }

        private void ValidateCommandResult()
        {
            var command = _innermostCommandResult!.Command;

            if (command.HasValidators && UseValidators(command, _innermostCommandResult))
            {
                return;
            }

            bool checkOnlyGlobalOptions = false;
            Command? currentCommand = command;
            while (currentCommand is not null)
            {
                if (currentCommand.HasOptions)
                {
                    var options = currentCommand.Options;
                    for (var i = 0; i < options.Count; i++)
                    {
                        var option = options[i];
                        if (option.IsRequired && (!checkOnlyGlobalOptions || (checkOnlyGlobalOptions && option.IsGlobal)))
                        {
                            if (_rootCommandResult!.FindResultFor(option) is null)
                            {
                                AddErrorToResult(
                                    _innermostCommandResult,
                                    new ParseError(
                                        _rootCommandResult.LocalizationResources.RequiredOptionWasNotProvided(option),
                                        _innermostCommandResult));
                            }
                        }
                    }
                }

                currentCommand = currentCommand.FirstParent?.Symbol as Command;
                checkOnlyGlobalOptions = true;
            }

            if (command.HasArguments)
            {
                ValidateArguments(command.Arguments, _innermostCommandResult);
            }
        }

        private bool UseValidators(Command command, CommandResult innermostCommandResult)
        {
            for (var i = 0; i < command.Validators.Count; i++)
            {
                var validateSymbolResult = command.Validators[i];
                validateSymbolResult(innermostCommandResult);

                if (!string.IsNullOrWhiteSpace(innermostCommandResult.ErrorMessage))
                {
                    AddErrorToResult(
                        innermostCommandResult,
                        new ParseError(innermostCommandResult.ErrorMessage!, _innermostCommandResult));

                    return true;
                }
            }
            return false;
        }

        private void ValidateArguments(IList<Argument> arguments, CommandResult innermostCommandResult)
        {
            for (var i = 0; i < arguments.Count; i++)
            {
                var symbol = arguments[i];

                var arityFailure = ArgumentArity.Validate(
                    innermostCommandResult,
                    symbol,
                    symbol.Arity.MinimumNumberOfValues,
                    symbol.Arity.MaximumNumberOfValues);

                if (arityFailure is not null)
                {
                    AddErrorToResult(innermostCommandResult, new ParseError(arityFailure.ErrorMessage!, innermostCommandResult));
                }
            }
        }

        private void ValidateCommandHandler()
        {
            if (_innermostCommandResult!.Command is not { Handler: null } cmd)
            {
                return;
            }

            if (!cmd.HasSubcommands)
            {
                return;
            }

            (_errors ??= new()).Insert(
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
                return;
            }

            if (optionResult.Option.HasValidators)
            {
                for (var i = 0; i < optionResult.Option.Validators.Count; i++)
                {
                    var validate = optionResult.Option.Validators[i];
                    validate(optionResult);

                    if (!string.IsNullOrWhiteSpace(optionResult.ErrorMessage))
                    {
                        AddErrorToResult(optionResult, new ParseError(optionResult.ErrorMessage!, optionResult));

                        return;
                    }
                }
            }

            if (optionResult.Children.Count == 0)
            {
                if (optionResult.Option.Argument is { HasCustomParser: true })
                {
                    if (optionResult.Option is { } opt)
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
            var argument = argumentResult.Argument;

            var parseError = argumentResult.CustomError(argument);

            if (parseError is { })
            {
                AddErrorToResult(argumentResult, parseError);
                return;
            }

            var argumentConversionResult = argumentResult.GetArgumentConversionResult();

            if (argumentConversionResult.Result >= ArgumentConversionResultType.Failed && 
                argumentConversionResult.Result != ArgumentConversionResultType.FailedArity)
            {
                if (argument.FirstParent?.Symbol is Option option)
                {
                    var completions = option.GetCompletions(CompletionContext.Empty).ToArray();

                    if (completions.Length > 0)
                    {
                        argumentConversionResult = ArgumentConversionResult.Failure(
                            argumentConversionResult.Argument,
                            argumentConversionResult.ErrorMessage + " Did you mean one of the following?" + Environment.NewLine + string.Join(Environment.NewLine, completions.Select(c => c.Label)),
                            argumentConversionResult.Result);
                    }
                }

                AddErrorToResult(argumentResult, new ParseError(argumentConversionResult.ErrorMessage!, argumentResult));
            }
        }

        private void PopulateDefaultValues()
        {
            var commandResult = _innermostCommandResult;
            
            while (commandResult is not null)
            {
                if (commandResult.Command.HasOptions)
                {
                    var options = commandResult.Command.Options;
                    for (var i = 0; i < options.Count; i++)
                    {
                        Symbol symbol = options[i];
                        Handle(_rootCommandResult!.FindResultForSymbol(symbol), symbol);
                    }
                }

                if (commandResult.Command.HasArguments)
                {
                    var arguments = commandResult.Command.Arguments;
                    for (var i = 0; i < arguments.Count; i++)
                    {
                        Symbol symbol = arguments[i];
                        Handle(_rootCommandResult!.FindResultForSymbol(symbol), symbol);
                    }
                }

                commandResult = commandResult.Parent as CommandResult;
            }

            void Handle(SymbolResult? symbolResult, Symbol symbol)
            {
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
                                    (_optionResults ??= new()).Add(optionResult);
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
        }

        private void AddErrorToResult(SymbolResult symbolResult, ParseError parseError)
        {
            symbolResult.ErrorMessage ??= parseError.Message;

            if (symbolResult.Parent is OptionResult optionResult)
            {
                optionResult.ErrorMessage ??= symbolResult.ErrorMessage;
            }

            (_errors ??= new()).Add(parseError);
        }

        public ParseResult GetResult() =>
            new(_parser,
                _rootCommandResult ?? throw new InvalidOperationException("No root command was found"),
                _innermostCommandResult ?? throw new InvalidOperationException("No command was found"),
                _directives,
                _tokens,
                _unmatchedTokens,
                _errors,
                _rawInput);
    }
}
