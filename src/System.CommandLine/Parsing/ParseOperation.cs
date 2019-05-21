// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal class ParseOperation
    {
        private readonly TokenizeResult _tokenizeResult;
        private readonly CommandLineConfiguration _configuration;
        private int _index;
        private readonly Dictionary<IArgument, int> _argumentCounts = new Dictionary<IArgument, int>();

        public ParseOperation(
            TokenizeResult tokenizeResult,
            CommandLineConfiguration configuration)
        {
            _tokenizeResult = tokenizeResult;
            _configuration = configuration;
        }

        public IReadOnlyList<ParseError> Errors { get; } = new List<ParseError>();

        private Token CurrentToken => _tokenizeResult.Tokens[_index];

        public void Parse()
        {
            RootCommandNode = ParseRootCommand();
        }

        public RootCommandNode RootCommandNode { get; private set; }

        private void Advance()
        {
            _index++;
        }

        private bool More()
        {
            return _index < _tokenizeResult.Tokens.Count;
        }

        private void IncrementCount(IArgument argument)
        {
            if (!_argumentCounts.TryGetValue(argument, out var count))
            {
                count = 0;
            }

            _argumentCounts[argument] = count + 1;
        }

        private bool IsFull(IArgument argument)
        {
            if (!_argumentCounts.TryGetValue(argument, out var count))
            {
                count = 0;
            }

            return count >= argument.Arity.MaximumNumberOfValues;
        }

        private void ParseCommandArguments(CommandNode commandNode)
        {
            while (More())
            {
                var argument = commandNode.Command.Arguments.FirstOrDefault(a => !IsFull(a));

                if (argument != null)
                {
                    var argumentNode = new ArgumentNode(CurrentToken, commandNode);
                    commandNode.AddChildNode(argumentNode);
                    IncrementCount(argument);
                    Advance();
                }
                else
                {
                    return;
                }
            }
        }

        private void ParseOptionArguments(OptionNode optionNode)
        {
            while (More() &&
                   !IsFull(optionNode.Option.Argument))
            {
                optionNode.AddChildNode(new ArgumentNode(CurrentToken, optionNode));

                Advance();
            }
        }

        private void ParseDirectives(RootCommandNode parent)
        {
            while (More())
            {
                var token = CurrentToken;

                if (token.Type != TokenType.Directive)
                {
                    return;
                }

                var withoutBrackets = token.Value.Substring(1, token.Value.Length - 2);
                var keyAndValue = withoutBrackets.Split(new[]
                {
                    ':'
                }, 2);

                var key = keyAndValue[0];
                var value = keyAndValue.Length == 2
                                ? keyAndValue[1]
                                : null;

                var directiveNode = new DirectiveNode(token, parent, key, value);

                parent.AddChildNode(directiveNode);

                Advance();
            }
        }

        private OptionNode ParseOption(CommandNode parent)
        {
            OptionNode optionNode = null;

            if (CurrentToken.Type == TokenType.Option &&
                parent.Command.Children.GetByAlias(CurrentToken.Value) is IOption option)
            {
                optionNode = new OptionNode(
                    CurrentToken,
                    option,
                    parent);

                ParseOptionArguments(optionNode);
            }

            return optionNode;
        }

        private RootCommandNode ParseRootCommand()
        {
            var rootCommandNode = new RootCommandNode(
                CurrentToken,
                _configuration.RootCommand);

            Advance();

            ParseDirectives(rootCommandNode);

            ParseCommandChildren(rootCommandNode);

            ParseUnmatchedTokens(rootCommandNode);

            return rootCommandNode;
        }

        private void ParseUnmatchedTokens(RootCommandNode rootCommandNode)
        {
            while (More())
            {
                rootCommandNode.AddChildNode(new UnmatchedTokenNode(CurrentToken, rootCommandNode));

                Advance();
            }
        }

        private void ParseCommandChildren(CommandNode parent)
        {
            while (More())
            {
                var child = ParseSubcommand(parent) ??
                            (SyntaxNode)ParseOption(parent);

                if (child == null)
                {
                    return;
                }

                parent.AddChildNode(child);

                Advance();

                ParseCommandArguments(parent);
            }
        }

        private CommandNode ParseSubcommand(CommandNode parentNode)
        {
            var command = parentNode.Command
                                    .Children
                                    .GetByAlias(CurrentToken.Value) as ICommand;

            if (command == null)
            {
                return null;
            }

            var commandNode = new CommandNode(CurrentToken, command, parentNode);

            ParseCommandChildren(commandNode);

            return commandNode;
        }
    }
}
