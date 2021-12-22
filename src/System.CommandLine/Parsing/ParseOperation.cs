﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
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

        private bool More(out TokenType currentTokenType)
        {
            bool result = _index < _tokenizeResult.Tokens.Count;
            currentTokenType = result ? _tokenizeResult.Tokens[_index].Type : (TokenType)(-1);
            return result;
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

        private void ParseSubcommand(CommandNode parentNode)
        {
            var commandNode = new CommandNode(CurrentToken, (Command)CurrentToken.Symbol!, parentNode);

            Advance();

            ParseCommandChildren(commandNode);

            parentNode.AddChildNode(commandNode);
        }

        private void ParseCommandChildren(CommandNode parent)
        {
            int currentArgumentCount = 0;
            int currentArgumentIndex = 0;

            while (More(out TokenType currentTokenType))
            {
                if (_configuration.EnableLegacyDoubleDashBehavior &&
                    currentTokenType == TokenType.DoubleDash)
                {
                    return;
                }

                if (currentTokenType == TokenType.Command)
                {
                    ParseSubcommand(parent);
                }
                else if (currentTokenType == TokenType.Option)
                {
                    ParseOption(parent);
                }
                else if (currentTokenType == TokenType.Argument)
                {
                    ParseCommandArguments(parent, ref currentArgumentCount, ref currentArgumentIndex);
                }
                else
                {
                    AddCurrentTokenToUnmatched();
                    Advance();
                }
            }
        }

        private void ParseCommandArguments(CommandNode commandNode, ref int currentArgumentCount, ref int currentArgumentIndex)
        {
            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Argument)
            {
                while (currentArgumentIndex < commandNode.Command.Arguments.Count)
                {
                    Argument argument = commandNode.Command.Arguments[currentArgumentIndex];

                    if (currentArgumentCount < argument.Arity.MaximumNumberOfValues)
                    {
                        var argumentNode = new CommandArgumentNode(
                            CurrentToken,
                            argument,
                            commandNode);

                        commandNode.AddChildNode(argumentNode);

                        currentArgumentCount++;

                        Advance();

                        break;
                    }
                    else
                    {
                        currentArgumentCount = 0;
                        currentArgumentIndex++;
                    }
                }

                if (currentArgumentCount == 0) // no matching arguments found
                {
                    AddCurrentTokenToUnmatched();
                    Advance();
                }
            }
        }

        private void ParseOption(CommandNode parent)
        {
            OptionNode optionNode = new OptionNode(
                CurrentToken,
                (Option)CurrentToken.Symbol!,
                parent);

            Advance();

            ParseOptionArguments(optionNode);

            parent.AddChildNode(optionNode);
        }

        private void ParseOptionArguments(OptionNode optionNode)
        {
            var argument = optionNode.Option.Argument;

            var contiguousTokens = 0;
            int argumentCount = 0;

            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Argument)
            {
                if (argumentCount >= argument.Arity.MaximumNumberOfValues)
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

                argumentCount++;

                contiguousTokens++;

                Advance();

                if (!optionNode.Option.AllowMultipleArgumentsPerToken)
                {
                    return;
                }
            }
        }

        private void ParseDirectives(RootCommandNode parent)
        {
            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Directive)
            {
                var token = CurrentToken;
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

            while (More(out TokenType currentTokenType))
            {
                if (currentTokenType == TokenType.DoubleDash)
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