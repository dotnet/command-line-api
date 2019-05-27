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

        private CommandResult _rootCommandResult;
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
            _rootCommandResult = new CommandResult(
                rootCommandNode.Command,
                rootCommandNode.Token);

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
                             .OfType<ArgumentResult2>()
                             .SingleOrDefault(r => r.Symbol == argumentNode.Argument);

            if (argumentResult == null)
            {
                argumentResult =
                    new ArgumentResult2(
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
            if (_innermostCommandResult.OptionResult(optionNode.Option.Name) == null)
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

            var optionResult = _innermostCommandResult.OptionResult(option.Name);

            var argumentResult =
                (ArgumentResult2)optionResult.Children.GetByAlias(argumentNode.Argument.Name);

            if (argumentResult == null)
            {
                argumentResult =
                    new ArgumentResult2(
                        argumentNode.Argument,
                        argumentNode.Token,
                        optionResult);
                optionResult.Children.Add(argumentResult);
                optionResult.AddToken(argumentNode.Token);
            }
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
            foreach (var commandResult in _innermostCommandResult.RecurseWhileNotNull(c => c.ParentCommandResult))
            {
                foreach (var symbol in commandResult.Command.Children)
                {
                    PopulateDefaultValues(commandResult, symbol);
                }

                foreach (var result in commandResult.Children)
                {
                    switch (result)
                    {
                        case ArgumentResult2 argumentResult:

                            ValidateArgument(argumentResult);

                            break;

                        case OptionResult optionResult:

                            ArgumentResult2[] argumentResult2s = optionResult.Children.OfType<ArgumentResult2>().ToArray();

                            foreach (var a in argumentResult2s)
                            {
                                ValidateArgument(a);
                            }

                            break;
                    }
                }
            }
        }

        private void ValidateArgument(ArgumentResult2 argumentResult2)
        {
            var failure = ArgumentArity.Validate(argumentResult2);

            if (failure != null)
            {
                _errors.Add(new ParseError(failure.ErrorMessage));
            }


        }

        private static void PopulateDefaultValues(
            CommandResult commandResult, 
            ISymbol symbol)
        {
            if (commandResult.Children.ResultFor(symbol) == null)
            {
                switch (symbol)
                {
                    case Option option when option.Argument.HasDefaultValue:

                        var optionResult = new OptionResult(
                            option,
                            option.CreateImplicitToken());

                        optionResult.Children.Add(
                            new ArgumentResult2(
                                option.Argument,
                                new ImplicitToken(option.Argument.GetDefaultValue(),
                                                  TokenType.Argument),
                                optionResult));

                        commandResult.Children.Add(optionResult);

                        break;

                    case Argument argument when argument.HasDefaultValue:
                        var result = new ArgumentResult2(
                            argument,
                            new ImplicitToken(argument.GetDefaultValue(), TokenType.Argument),
                            commandResult);

                        commandResult.Children.Add(result);

                        break;
                }
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
