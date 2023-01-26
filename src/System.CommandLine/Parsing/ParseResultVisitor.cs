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
            _rawInput = rawInput;
            _symbolResultTree = new(_parser.Configuration.LocalizationResources, tokenizeErrors, unmatchedTokens);
            _innermostCommandResult = _rootCommandResult = new CommandResult(
                rootCommandNode.Command,
                rootCommandNode.Token,
                _symbolResultTree);
        }

        internal void Visit(CommandNode rootCommandNode)
        {
            VisitChildren(rootCommandNode);

            if (!_isHelpRequested)
            {
                Validate();
            }
        }

        internal ParseResult CreateResult() =>
            new(_parser,
                _rootCommandResult,
                _innermostCommandResult,
                _directives,
                _tokens,
                _symbolResultTree.UnmatchedTokens,
                _symbolResultTree.Errors,
                _rawInput);

        private void VisitSyntaxNode(SyntaxNode node)
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
                if (optionNode.Option.DisallowBinding && optionNode.Option is HelpOption)
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
                    VisitSyntaxNode(parentNode.Children[i]);
                }
            }
        }

        private void Validate()
        {
            // Only the inner most command goes through complete validation,
            // for other commands only a subset of options is checked.
            _innermostCommandResult.Validate(completeValidation: true);

            CommandResult? currentResult = _innermostCommandResult.Parent as CommandResult;
            while (currentResult is not null)
            {
                currentResult.Validate(completeValidation: false);

                currentResult = currentResult.Parent as CommandResult;
            }
        }
    }
}
