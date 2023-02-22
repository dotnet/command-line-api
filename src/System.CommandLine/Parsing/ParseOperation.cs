// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Help;
using System.CommandLine.Invocation;

namespace System.CommandLine.Parsing
{
    internal sealed class ParseOperation
    {
        private readonly List<Token> _tokens;
        private readonly CommandLineConfiguration _configuration;
        private readonly string? _rawInput;
        private readonly SymbolResultTree _symbolResultTree;
        private readonly CommandResult _rootCommandResult;

        private int _index;
        private CommandResult _innermostCommandResult;
        private bool _isHelpRequested, _isParseRequested;
        private ICommandHandler? _handler;
        private Symbol? _symbol;

        public ParseOperation(
            List<Token> tokens,
            CommandLineConfiguration configuration,
            List<string>? tokenizeErrors,
            string? rawInput)
        {
            _tokens = tokens;
            _configuration = configuration;
            _rawInput = rawInput;
            _symbolResultTree = new(configuration.LocalizationResources, tokenizeErrors);
            _innermostCommandResult = _rootCommandResult = new CommandResult(
                _configuration.RootCommand,
                CurrentToken,
                _symbolResultTree);

            Advance();
        }

        private Token CurrentToken => _tokens[_index];

        private void Advance() => _index++;

        private bool More(out TokenType currentTokenType)
        {
            bool result = _index < _tokens.Count;
            currentTokenType = result ? _tokens[_index].Type : (TokenType)(-1);
            return result;
        }

        internal ParseResult Parse(Parser parser)
        {
            ParseDirectives();

            ParseCommandChildren();

            if (!_isHelpRequested)
            {
                Validate();
            }

            if (_handler is null)
            {
                if (_configuration.ParseErrorReportingExitCode.HasValue && _symbolResultTree.ErrorCount > 0)
                {
                    _handler = new AnonymousCommandHandler(ParseErrorResult.Apply);
                }
                else if (_configuration.MaxLevenshteinDistance > 0 && _rootCommandResult.Command.TreatUnmatchedTokensAsErrors
                    && _symbolResultTree.UnmatchedTokens is not null)
                {
                    _handler = new AnonymousCommandHandler(TypoCorrection.ProvideSuggestions);
                }
            }

            return new (
                parser,
                _symbol ?? _innermostCommandResult.Command,
                _rootCommandResult,
                _innermostCommandResult,
                _tokens,
                _symbolResultTree.UnmatchedTokens,
                _symbolResultTree.Errors,
                _rawInput,
                _handler);
        }

        private void ParseSubcommand()
        {
            Command command = (Command)CurrentToken.Symbol!;

            _innermostCommandResult = new CommandResult(
                command,
                CurrentToken,
                _symbolResultTree,
                _innermostCommandResult);

            _symbolResultTree.Add(command, _innermostCommandResult);

            Advance();

            ParseCommandChildren();
        }

        private void ParseCommandChildren()
        {
            int currentArgumentCount = 0;
            int currentArgumentIndex = 0;

            while (More(out TokenType currentTokenType))
            {
                if (currentTokenType == TokenType.Command)
                {
                    ParseSubcommand();
                }
                else if (currentTokenType == TokenType.Option)
                {
                    ParseOption();
                }
                else if (currentTokenType == TokenType.Argument)
                {
                    ParseCommandArguments(ref currentArgumentCount, ref currentArgumentIndex);
                }
                else
                {
                    AddCurrentTokenToUnmatched();
                    Advance();
                }
            }
        }

        private void ParseCommandArguments(ref int currentArgumentCount, ref int currentArgumentIndex)
        {
            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Argument)
            {
                while (_innermostCommandResult.Command.HasArguments && currentArgumentIndex < _innermostCommandResult.Command.Arguments.Count)
                {
                    Argument argument = _innermostCommandResult.Command.Arguments[currentArgumentIndex];

                    if (currentArgumentCount < argument.Arity.MaximumNumberOfValues)
                    {
                        if (CurrentToken.Symbol is null)
                        {
                            // update the token with missing information now, so later stages don't need to modify it
                            CurrentToken.Symbol = argument;
                        }

                        if (!(_symbolResultTree.TryGetValue(argument, out var symbolResult)
                            && symbolResult is ArgumentResult argumentResult))
                        {
                            argumentResult =
                                new ArgumentResult(
                                    argument,
                                    _symbolResultTree,
                                    _innermostCommandResult);

                            _symbolResultTree.Add(argument, argumentResult);
                        }

                        argumentResult.AddToken(CurrentToken);
                        _innermostCommandResult.AddToken(CurrentToken);

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

        private void ParseOption()
        {
            Option option = (Option)CurrentToken.Symbol!;
            OptionResult optionResult;

            if (!_symbolResultTree.TryGetValue(option, out SymbolResult? symbolResult))
            {
                // parse directive has a precedence over --help and --version
                if (!_isParseRequested)
                {
                    if (option is HelpOption)
                    {
                        _isHelpRequested = true;

                        _handler = new AnonymousCommandHandler(HelpOption.Handler);
                    }
                    else if (option is VersionOption)
                    {
                        _handler = new AnonymousCommandHandler(VersionOption.Handler);
                    }
                }

                optionResult = new OptionResult(
                    option,
                    _symbolResultTree,
                    CurrentToken,
                    _innermostCommandResult);

                _symbolResultTree.Add(option, optionResult);
            }
            else
            {
                optionResult = (OptionResult)symbolResult;
            }

            Advance();

            ParseOptionArguments(optionResult);
        }

        private void ParseOptionArguments(OptionResult optionResult)
        {
            var argument = optionResult.Option.Argument;

            var contiguousTokens = 0;
            int argumentCount = 0;

            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Argument)
            {
                if (argumentCount >= argument.Arity.MaximumNumberOfValues)
                {
                    if (contiguousTokens > 0)
                    {
                        break;
                    }

                    if (argument.Arity.MaximumNumberOfValues == 0)
                    {
                        break;
                    }
                }
                else if (argument.ValueType == typeof(bool) && !bool.TryParse(CurrentToken.Value, out _))
                {
                    break;
                }

                if (!(_symbolResultTree.TryGetValue(argument, out SymbolResult? symbolResult)
                    && symbolResult is ArgumentResult argumentResult))
                {
                    argumentResult = new ArgumentResult(
                            argument,
                            _symbolResultTree,
                            optionResult);

                    _symbolResultTree.Add(argument, argumentResult);
                }

                argumentResult.AddToken(CurrentToken);
                optionResult.AddToken(CurrentToken);

                argumentCount++;

                contiguousTokens++;

                Advance();

                if (!optionResult.Option.AllowMultipleArgumentsPerToken)
                {
                    return;
                }
            }

            if (argumentCount == 0)
            {
                ArgumentResult argumentResult = new(optionResult.Option.Argument, _symbolResultTree, optionResult);
                _symbolResultTree.Add(optionResult.Option.Argument, argumentResult);
            }
        }

        private void ParseDirectives()
        {
            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Directive)
            {
                if (_configuration.Directives.Count > 0)
                {
                    ParseDirective(); // kept in separate method to avoid JIT
                }

                Advance();
            }

            void ParseDirective()
            {
                var token = CurrentToken;

                if (token.Symbol is not Directive directive)
                {
                    // Directives_should_not_be_considered_as_unmatched_tokens
                    return;
                }

                DirectiveResult result;
                if (_symbolResultTree.TryGetValue(directive, out var directiveResult))
                {
                    result = (DirectiveResult)directiveResult;
                    result.AddToken(token);
                }
                else
                {
                    result = new (directive, token, _symbolResultTree);
                    _symbolResultTree.Add(directive, result);
                }

                ReadOnlySpan<char> withoutBrackets = token.Value.AsSpan(1, token.Value.Length - 2);
                int indexOfColon = withoutBrackets.IndexOf(':');
                if (indexOfColon > 0)
                {
                    result.AddValue(withoutBrackets.Slice(indexOfColon + 1).ToString());
                }

                _handler = directive.Handler;
                _symbol = directive;

                if (directive is ParseDirective)
                {
                    _isParseRequested = true;
                }
            }
        }

        private void AddCurrentTokenToUnmatched()
        {
            if (CurrentToken.Type == TokenType.DoubleDash)
            {
                return;
            }

            _symbolResultTree.AddUnmatchedToken(CurrentToken,
                _rootCommandResult.Command.TreatUnmatchedTokensAsErrors ? _rootCommandResult : null);
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