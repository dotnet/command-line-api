// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;

namespace System.CommandLine.Parsing
{
    internal class ParseOperation
    {
        private readonly TokenizeResult _tokenizeResult;
        private readonly CommandLineConfiguration _configuration;
        private int _index;
        private readonly Dictionary<Argument, int> _argumentCounts = new();

        public ParseOperation(
            TokenizeResult tokenizeResult,
            CommandLineConfiguration configuration)
        {
            _tokenizeResult = tokenizeResult;
            _configuration = configuration;
        }

        private Token CurrentToken => _tokenizeResult.Tokens[_index];

        public List<ParseError> Errors { get; } = new();

        public RootCommandNode? RootCommandNode { get; private set; }

        public List<Token>? UnmatchedTokens { get; private set; }

        public List<Token>? UnparsedTokens { get; private set; } 

        private void Advance() => _index++;

        private void IncrementCount(Argument argument, int count)
        {
            _argumentCounts[argument] = count + 1;
        }

        private bool IsFull(Argument argument, out int count)
        {
            if (!_argumentCounts.TryGetValue(argument, out count))
            {
                count = 0;
            }

            return count >= argument.Arity.MaximumNumberOfValues;
        }

        private bool More()
        {
            return _index < _tokenizeResult.Tokens.Count;
        }

        public void Parse()
        {
            RootCommandNode = ParseRootCommand();
        }

        private RootCommandNode ParseRootCommand()
        {
            var rootCommandNode = new RootCommandNode(
                CurrentToken,
                _configuration.RootCommand);

            Advance();

            ParseDirectives(rootCommandNode);

            ParseCommandChildren(rootCommandNode);

            ParseRemainingTokens();

            return rootCommandNode;
        }

        private CommandNode? ParseSubcommand(CommandNode parentNode)
        {
            if (CurrentToken.Type != TokenType.Command)
            {
                return null;
            }

            var commandNode = new CommandNode(CurrentToken, (Command)CurrentToken.Symbol!, parentNode);

            Advance();

            ParseCommandChildren(commandNode);

            return commandNode;
        }

        private void ParseCommandChildren(CommandNode parent)
        {
            while (More())
            {
                if (_configuration.EnableLegacyDoubleDashBehavior &&
                    CurrentToken.Type == TokenType.DoubleDash)
                {
                    return;
                }

                var child = ParseSubcommand(parent) ??
                            (SyntaxNode?)ParseOption(parent) ??
                            ParseCommandArgument(parent);

                if (child is null)
                {
                    AddCurrentTokenToUnmatched();
                    Advance();
                }
                else
                {
                    parent.AddChildNode(child);
                }
            }
        }

        private CommandArgumentNode? ParseCommandArgument(CommandNode commandNode)
        {
            if (CurrentToken.Type != TokenType.Argument)
            {
                return null;
            }

            for (var i = 0; i < commandNode.Command.Arguments.Count; i++)
            {
                Argument arg = commandNode.Command.Arguments[i];
                if (!IsFull(arg, out int count))
                {
                    var argumentNode = new CommandArgumentNode(
                       CurrentToken,
                       arg,
                       commandNode);

                    IncrementCount(arg, count);

                    Advance();

                    return argumentNode;
                }
            }

            return null;
        }

        private OptionNode? ParseOption(CommandNode parent)
        {
            if (CurrentToken.Type != TokenType.Option)
            {
                return null;
            }

            OptionNode optionNode = new OptionNode(
                CurrentToken,
                (Option)CurrentToken.Symbol!,
                parent);

            Advance();

            ParseOptionArguments(optionNode);

            return optionNode;
        }

        private void ParseOptionArguments(OptionNode optionNode)
        {
            var argument = optionNode.Option.Argument;

            var contiguousTokens = 0;
            var continueProcessing = true;

            while (More() &&
                   CurrentToken.Type == TokenType.Argument &&
                   continueProcessing)
            {
                if (IsFull(argument, out int count))
                {
                    if (contiguousTokens > 0)
                    {
                        return;
                    }

                    if (argument.Arity.MaximumNumberOfValues == 0)
                    {
                        return;
                    }
                }
                else if (argument.ValueType == typeof(bool))
                {
                    if (ArgumentConverter.ConvertObject(
                            argument,
                            argument.ValueType,
                            CurrentToken.Value,
                            _configuration.LocalizationResources) is FailedArgumentTypeConversionResult)
                    {
                        return;
                    }
                }

                optionNode.AddChildNode(
                    new OptionArgumentNode(
                        CurrentToken,
                        argument,
                        optionNode));

                IncrementCount(argument, count);

                contiguousTokens++;

                Advance();

                continueProcessing = optionNode.Option.AllowMultipleArgumentsPerToken;
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

        private void ParseRemainingTokens()
        {
            var foundEndOfArguments = false;

            while (More())
            {
                if (CurrentToken.Type == TokenType.DoubleDash)
                {
                    foundEndOfArguments = true;
                }
                else if (foundEndOfArguments)
                {
                    AddCurrentTokenToUnparsed();
                }

                Advance();
            }
        }

        private void AddCurrentTokenToUnmatched() => (UnmatchedTokens ??= new()).Add(CurrentToken);

        private void AddCurrentTokenToUnparsed() => (UnparsedTokens ??= new()).Add(CurrentToken);
    }
}