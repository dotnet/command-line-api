// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Help;

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
            if (node is OptionNode optionNode)
                VisitOptionNode(optionNode);
            else if (node is OptionArgumentNode optionArgumentNode)
                VisitOptionArgumentNode(optionArgumentNode);
            else if (node is CommandArgumentNode commandArgumentNode)
                VisitCommandArgumentNode(commandArgumentNode);
            else if (node is CommandNode commandNode)
                VisitCommandNode(commandNode);
            else if (node is DirectiveNode directiveNode)
                VisitDirectiveNode(directiveNode);
        }

        private void VisitCommandNode(CommandNode commandNode)
        {
            _symbolResultTree.Add(commandNode.Command, _innermostCommandResult = new CommandResult(
                commandNode.Command,
                commandNode.Token,
                _symbolResultTree,
                _innermostCommandResult));

            VisitChildren(commandNode);
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

                _symbolResultTree.Add(argumentNode.Argument, argumentResult);
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
                    ArgumentResult argumentResult = new (optionResult.Option.Argument, _symbolResultTree, optionResult);
                    _symbolResultTree.Add(optionResult.Option.Argument, argumentResult);
                }
            }

            VisitChildren(optionNode);
        }

        private void VisitOptionArgumentNode(OptionArgumentNode argumentNode)
        {
            OptionResult optionResult = (OptionResult)_symbolResultTree[argumentNode.ParentOptionNode.Option];

            var argument = argumentNode.Argument;

            if (!(_symbolResultTree.TryGetValue(argument, out SymbolResult? symbolResult)
                    && symbolResult is ArgumentResult argumentResult))
            {
                argumentResult = new ArgumentResult(
                        argumentNode.Argument,
                        _symbolResultTree,
                        optionResult);

                _symbolResultTree.Add(argument, argumentResult);
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

            ValidateCommandResults();
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

        private void ValidateCommandResults()
        {
            CommandResult? currentResult = _innermostCommandResult;
            while (currentResult is not null)
            {
                Command command = currentResult.Command;
                // Only the inner most command goes through full check, for other commands only global options are checked
                bool performFullCheck = currentResult == _innermostCommandResult;
                
                if (performFullCheck && command.HasValidators)
                {
                    if (ValidateCommand(currentResult))
                    {
                        return;
                    }
                }

                if (command.HasOptions)
                {
                    ValidateCommandOptions(currentResult, performFullCheck);
                }

                if (command.HasArguments)
                {
                    ValidateCommandArguments(currentResult, performFullCheck);
                }

                currentResult = currentResult.Parent as CommandResult;
            }
        }

        private bool ValidateCommand(CommandResult commandResult)
        {
            int errorCountBefore = _symbolResultTree.ErrorCount;
            for (var i = 0; i < commandResult.Command.Validators.Count; i++)
            {
                commandResult.Command.Validators[i](commandResult);
            }
            return _symbolResultTree.ErrorCount != errorCountBefore;
        }

        private void ValidateCommandOptions(CommandResult commandResult, bool performFullCheck)
        {
            var options = commandResult.Command.Options;
            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];

                if (!performFullCheck && !(option.IsGlobal || option.Argument.HasDefaultValue || option.DisallowBinding))
                {
                    continue;
                }

                OptionResult optionResult;
                ArgumentResult argumentResult;

                if (!_symbolResultTree.TryGetValue(option, out SymbolResult? symbolResult))
                {
                    if (option.IsRequired)
                    {
                        commandResult.AddError(commandResult.LocalizationResources.RequiredOptionWasNotProvided(option));
                        continue;
                    }
                    else if (option.Argument.HasDefaultValue)
                    {
                        optionResult = new (option, _symbolResultTree, null, commandResult);
                        _symbolResultTree.Add(optionResult.Option, optionResult);

                        argumentResult = new (optionResult.Option.Argument, _symbolResultTree, optionResult);
                        _symbolResultTree.Add(optionResult.Option.Argument, argumentResult);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    optionResult = (OptionResult)symbolResult;
                    argumentResult = (ArgumentResult)_symbolResultTree[option.Argument];
                }

                // When_there_is_an_arity_error_then_further_errors_are_not_reported
                if (!ArgumentArity.Validate(argumentResult, out var error))
                {
                    optionResult.AddError(error.ErrorMessage!);
                    continue;
                }

                if (optionResult.Option.HasValidators)
                {
                    int errorsBefore = _symbolResultTree.ErrorCount;

                    for (var j = 0; j < optionResult.Option.Validators.Count; j++)
                    {
                        optionResult.Option.Validators[j](optionResult);
                    }

                    if (errorsBefore != _symbolResultTree.ErrorCount)
                    {
                        break;
                    }
                }

                _ = argumentResult.GetArgumentConversionResult();
            }
        }

        private void ValidateCommandArguments(CommandResult commandResult, bool performFullCheck)
        {
            var arguments = commandResult.Command.Arguments;
            for (var i = 0; i < arguments.Count; i++)
            {
                Argument argument = arguments[i];

                if (!performFullCheck && !argument.HasDefaultValue)
                {
                    continue;
                }

                ArgumentResult? argumentResult;
                if (_symbolResultTree.TryGetValue(argument, out SymbolResult? symbolResult))
                {
                    argumentResult = (ArgumentResult)symbolResult;
                }
                else if (argument.HasDefaultValue)
                {
                    argumentResult = new ArgumentResult(argument, _symbolResultTree, commandResult);
                    _symbolResultTree[argument] = argumentResult;
                }
                else if (argument.Arity.MinimumNumberOfValues > 0)
                {
                    commandResult.AddError(commandResult.LocalizationResources.RequiredArgumentMissing(commandResult));
                    continue;
                }
                else
                {
                    continue;
                }

                _ = argumentResult.GetArgumentConversionResult();
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
