// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
        private readonly SymbolResultTree _symbolResultTree;
        private readonly CommandResult _rootCommandResult;

        private Dictionary<string, IReadOnlyList<string>>? _directives;
        private List<Token>? _unmatchedTokens;
        private List<ArgumentResult>? _argumentResults;
        private CommandResult _innermostCommandResult;
        private bool _isHelpRequested;

        internal ParseResultVisitor(
            Parser parser,
            List<Token> tokens,
            List<string>? tokenizeErrors,
            List<Token>? unmatchedTokens,
            string? rawInput,
            CommandNode rootCommandNode)
        {
            _parser = parser;
            _tokens = tokens;
            _unmatchedTokens = unmatchedTokens;
            _rawInput = rawInput;
            _symbolResultTree = new(_parser.Configuration.LocalizationResources, tokenizeErrors);
            _innermostCommandResult = _rootCommandResult = new CommandResult(
                rootCommandNode.Command,
                rootCommandNode.Token,
                _symbolResultTree);
        }

        internal void Visit(CommandNode rootCommandNode)
        {
            VisitChildren(rootCommandNode);

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
                    VisitCommandNode(commandNode);

                    VisitChildren(commandNode);

                    break;

                case OptionNode optionNode:
                    VisitOptionNode(optionNode);

                    VisitChildren(optionNode);

                    break;

                case CommandArgumentNode commandArgumentNode:
                    VisitCommandArgumentNode(commandArgumentNode);

                    break;

                case OptionArgumentNode optionArgumentNode:
                    VisitOptionArgumentNode(optionArgumentNode);

                    break;
            }
        }

        private void AddToResult(ArgumentResult result)
        {
            if (_symbolResultTree.TryAdd(result.Argument, result))
            {
                (_argumentResults ??= new()).Add(result);
            }
        }

        private void VisitCommandNode(CommandNode commandNode)
        {
            var commandResult = new CommandResult(
                commandNode.Command,
                commandNode.Token,
                _symbolResultTree,
                _innermostCommandResult);

            _symbolResultTree.Add(commandNode.Command, commandResult);

            _innermostCommandResult = commandResult;
        }

        private void VisitCommandArgumentNode(CommandArgumentNode argumentNode)
        {
            if (!(_symbolResultTree.TryGetValue(argumentNode.Argument, out var symbolResult)
                    && symbolResult is ArgumentResult argumentResult))
            {
                argumentResult =
                    new ArgumentResult(
                        argumentNode.Argument,
                        _symbolResultTree,
                        _innermostCommandResult);

                AddToResult(argumentResult);
            }

            argumentResult.AddToken(argumentNode.Token);
            _innermostCommandResult.AddToken(argumentNode.Token);
        }

        private void VisitOptionNode(OptionNode optionNode)
        {
            if (!_symbolResultTree.ContainsKey(optionNode.Option))
            {
                if (optionNode.Option is HelpOption)
                {
                    _isHelpRequested = true;
                }

                var optionResult = new OptionResult(
                    optionNode.Option,
                    _symbolResultTree,
                    optionNode.Token,
                    _innermostCommandResult);

                _symbolResultTree.Add(optionNode.Option, optionResult);

                if (optionNode.Children is null) // no Arguments
                {
                    if (optionResult.Option.Argument.HasCustomParser)
                    {
                        ArgumentResult argumentResult = new (optionResult.Option.Argument, _symbolResultTree, optionResult);
                        _symbolResultTree.Add(optionResult.Option.Argument, argumentResult);
                    }
                }
            }
        }

        private void VisitOptionArgumentNode(
            OptionArgumentNode argumentNode)
        {
            OptionResult optionResult = (OptionResult)_symbolResultTree[argumentNode.ParentOptionNode.Option];

            var argument = argumentNode.Argument;

            if (!(_symbolResultTree.TryGetValue(argument, out SymbolResult? symbolResult)
                    && symbolResult is ArgumentResult argumentResult))
            {
                argumentResult =
                    new ArgumentResult(
                        argumentNode.Argument,
                        _symbolResultTree,
                        optionResult);
                _symbolResultTree.TryAdd(argument, argumentResult);
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

        private void VisitChildren(NonterminalSyntaxNode parentNode)
        {
            if (parentNode.Children is not null)
            {
                for (var i = 0; i < parentNode.Children.Count; i++)
                {
                    VisitInternal(parentNode.Children[i]);
                }
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

            foreach (var symbolPair in _symbolResultTree)
            {
                if (symbolPair.Value is OptionResult optionResult)
                {
                    ValidateAndConvertOptionResult(optionResult);
                }
            }

            if (_argumentResults is not null)
            {
                ValidateAndConvertArgumentResults(_innermostCommandResult.Command.Arguments, _argumentResults);
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
                        _symbolResultTree,
                        _innermostCommandResult);

                    var previousArgumentResult = argumentResults[i - 1];

                    var passedOnTokensCount = _innermostCommandResult.Tokens.Count;

                    ShiftPassedOnTokensToNextResult(previousArgumentResult, nextArgumentResult, passedOnTokensCount);

                    argumentResults.Add(nextArgumentResult);

                    if (previousArgumentResult.Parent is CommandResult)
                    {
                        AddToResult(nextArgumentResult);
                    }

                    _symbolResultTree.TryAdd(nextArgumentResult.Argument, nextArgumentResult);
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

                    next.AddToken(_innermostCommandResult.Tokens[j]);
                }
            }
        }

        private void ValidateCommandResult()
        {
            var command = _innermostCommandResult.Command;

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
                            if (_rootCommandResult.FindResultFor(option) is null)
                            {
                                _innermostCommandResult.AddError(_rootCommandResult.LocalizationResources.RequiredOptionWasNotProvided(option));
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
            if (!command.HasValidators)
            {
                return false;
            }

            int errorCountBefore = _symbolResultTree.ErrorCount;
            for (var i = 0; i < command.Validators.Count; i++)
            {
                command.Validators[i](innermostCommandResult);
            }
            return _symbolResultTree.ErrorCount != errorCountBefore;
        }

        private void ValidateArguments(IList<Argument> arguments, CommandResult innermostCommandResult)
        {
            for (var i = 0; i < arguments.Count; i++)
            {
                ArgumentResult argumentResult = _symbolResultTree.TryGetValue(arguments[i], out SymbolResult? symbolResult)
                    ? (ArgumentResult)symbolResult
                    : new ArgumentResult(arguments[i], _symbolResultTree, innermostCommandResult);

                ArgumentConversionResult? arityFailure = ArgumentArity.Validate(argumentResult);

                if (arityFailure is not null)
                {
                    innermostCommandResult.AddError(arityFailure.ErrorMessage!);
                }
            }
        }

        private void ValidateCommandHandler()
        {
            if (_innermostCommandResult.Command is not { Handler: null } cmd)
            {
                return;
            }

            if (!cmd.HasSubcommands)
            {
                return;
            }

            _symbolResultTree.InsertError(
                0,
                new ParseError(
                    _innermostCommandResult.LocalizationResources.RequiredCommandWasNotProvided(),
                    _innermostCommandResult));
        }

        private void ValidateAndConvertOptionResult(OptionResult optionResult)
        {
            var argument = optionResult.Option.Argument;

            ArgumentResult argumentResult = _symbolResultTree.TryGetValue(argument, out SymbolResult? symbolResult)
                ? (ArgumentResult)symbolResult
                : new ArgumentResult(argument, _symbolResultTree, optionResult);

            ArgumentConversionResult? arityFailure = ArgumentArity.Validate(argumentResult);
            if (arityFailure is not null)
            {
                optionResult.AddError(arityFailure.ErrorMessage!);
                return;
            }

            if (optionResult.Option.HasValidators)
            {
                int errorsBefore = _symbolResultTree.ErrorCount;

                for (var i = 0; i < optionResult.Option.Validators.Count; i++)
                {
                    optionResult.Option.Validators[i](optionResult);
                }

                if (errorsBefore != _symbolResultTree.ErrorCount)
                {
                    return;
                }
            }

            foreach (var pair in _symbolResultTree)
            {
                if (ReferenceEquals(pair.Value.Parent, optionResult))
                {
                    ValidateAndConvertArgumentResult((ArgumentResult)pair.Value);
                }
            }
        }

        private void ValidateAndConvertArgumentResult(ArgumentResult argumentResult)
        {
            var argument = argumentResult.Argument;

            if (argument.HasValidators)
            {
                int errorsBefore = _symbolResultTree.ErrorCount;
                for (var i = 0; i < argument.Validators.Count; i++)
                {
                    argument.Validators[i](argumentResult);
                }

                if (errorsBefore != _symbolResultTree.ErrorCount)
                {
                    return;
                }
            }

            _ = argumentResult.GetArgumentConversionResult();
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
                        Option option = options[i];
                        Handle(_rootCommandResult.FindResultFor(option), option);
                    }
                }

                if (commandResult.Command.HasArguments)
                {
                    var arguments = commandResult.Command.Arguments;
                    for (var i = 0; i < arguments.Count; i++)
                    {
                        Argument argument = arguments[i];
                        Handle(_rootCommandResult.FindResultFor(argument), argument);
                    }
                }

                commandResult = commandResult.Parent as CommandResult;
            }

            void Handle(SymbolResult? symbolResult, Symbol symbol)
            {
                switch (symbolResult)
                {
                    case OptionResult o:

                        if (o.Option.Argument.ValueType == typeof(bool)
                            && !_symbolResultTree.ContainsKey(o.Option.Argument))
                        {
                            _symbolResultTree.Add(o.Option.Argument, new ArgumentResult(o.Option.Argument, _symbolResultTree, o));
                        }

                        break;

                    case null:
                        switch (symbol)
                        {
                            case Option option when option.Argument.HasDefaultValue:

                                var optionResult = new OptionResult(
                                    option,
                                    _symbolResultTree,
                                    null,
                                    commandResult);

                                if (_symbolResultTree.TryAdd(optionResult.Option, optionResult))
                                {
                                    ArgumentResult argumentResult = new (optionResult.Option.Argument, _symbolResultTree, optionResult);
                                    _symbolResultTree.Add(optionResult.Option.Argument, argumentResult);
                                }

                                break;

                            case Argument { HasDefaultValue: true } argument:

                                if (!_symbolResultTree.ContainsKey(argument))
                                {
                                    AddToResult(new ArgumentResult(argument, _symbolResultTree, commandResult));
                                }
                                
                                break;
                        }

                        break;
                }
            }
        }

        public ParseResult GetResult() =>
            new(_parser,
                _rootCommandResult,
                _innermostCommandResult,
                _directives,
                _tokens,
                _unmatchedTokens,
                _symbolResultTree.Errors,
                _rawInput);
    }
}
