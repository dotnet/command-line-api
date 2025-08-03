// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine.Parsing
{
    internal sealed class ParseOperation
    {
        private readonly List<Token> _tokens;
        private readonly ParserConfiguration _configuration;
        private readonly string? _rawInput;
        private readonly SymbolResultTree _symbolResultTree;
        private readonly CommandResult _rootCommandResult;

        private int _index;
        private CommandResult _innermostCommandResult;
        private bool _isTerminatingDirectiveSpecified;
        private CommandLineAction? _primaryAction;
        private List<CommandLineAction>? _preActions;
        private readonly Command _rootCommand;

        public ParseOperation(
            List<Token> tokens,
            Command rootCommand,
            ParserConfiguration configuration,
            List<string>? tokenizeErrors,
            string? rawInput)
        {
            _tokens = tokens;
            _configuration = configuration;
            _rootCommand = rootCommand;
            _rawInput = rawInput;
            _symbolResultTree = new(_rootCommand, tokenizeErrors);
            _innermostCommandResult = _rootCommandResult = new CommandResult(
                _rootCommand,
                CurrentToken,
                _symbolResultTree);
            _symbolResultTree.Add(_rootCommand, _rootCommandResult);

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

        internal ParseResult Parse()
        {
            ParseDirectives();

            ParseCommandChildren();

            ValidateAndAddDefaultResults();

            return new(
                _configuration,
                _rootCommandResult,
                _innermostCommandResult,
                _tokens,
                _symbolResultTree.UnmatchedTokens,
                _symbolResultTree.Errors,
                _rawInput,
                _primaryAction,
                _preActions);
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
                if (option.Action is not null)
                {
                    // directives have a precedence over --help and --version
                    if (!_isTerminatingDirectiveSpecified)
                    {
                        if (option.Action.Terminating)
                        {
                            _primaryAction = option.Action;
                        }
                        else
                        {
                            AddPreAction(option.Action);
                        }
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

            optionResult.IdentifierTokenCount++;

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
                else if (argument.IsBoolean() && !bool.TryParse(CurrentToken.Value, out _))
                {
                    // Don't greedily consume the following token for bool. The presence of the option token (i.e. a flag) is sufficient.
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
                if (!_symbolResultTree.ContainsKey(argument))
                {
                    var argumentResult = new ArgumentResult(argument, _symbolResultTree, optionResult);
                    _symbolResultTree.Add(argument, argumentResult);
                }
            }
        }

        private void ParseDirectives()
        {
            while (More(out TokenType currentTokenType) && currentTokenType == TokenType.Directive)
            {
                if (_rootCommand is RootCommand { Directives.Count: > 0 })
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
                    AddCurrentTokenToUnmatched();
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

                if (directive.Action is not null)
                {
                    if (directive.Action.Terminating)
                    {
                        _primaryAction = directive.Action;
                        _isTerminatingDirectiveSpecified = true;
                    }
                    else
                    {
                        AddPreAction(directive.Action);
                    }
                }
            }
        }

        private void AddPreAction(CommandLineAction action)
        {
            if (_preActions is null)
            {
                _preActions = new();
            }

            _preActions.Add(action);
        }

        private void AddCurrentTokenToUnmatched()
        {
            if (CurrentToken.Type == TokenType.DoubleDash)
            {
                return;
            }

            _symbolResultTree.AddUnmatchedToken(CurrentToken, _innermostCommandResult, _rootCommandResult);
        }

        private void ValidateAndAddDefaultResults()
        {
            // Only the innermost command goes through complete validation,
            // for other commands only a subset of options is checked.
            _innermostCommandResult.Validate(isInnermostCommand: true);

            CommandResult? currentResult = _innermostCommandResult.Parent as CommandResult;
            while (currentResult is not null)
            {
                currentResult.Validate(isInnermostCommand: false);

                currentResult = currentResult.Parent as CommandResult;
            }

            if (_primaryAction is null)
            {
                if (_innermostCommandResult is { Command: { Action: null, HasSubcommands: true } })
                {
                    _symbolResultTree.InsertFirstError(
                        new ParseError(LocalizationResources.RequiredCommandWasNotProvided(), _innermostCommandResult));
                }

                if (_innermostCommandResult is { Command.Action.ClearsParseErrors: true } &&
                    _symbolResultTree.Errors is not null)
                {
                    var errorsNotUnderInnermostCommand = _symbolResultTree
                                                         .Errors
                                                         .Where(e => e.SymbolResult != _innermostCommandResult)
                                                         .ToList();

                    _symbolResultTree.Errors = errorsNotUnderInnermostCommand;
                }
                else if (_symbolResultTree.ErrorCount > 0)
                {
                    _primaryAction = new ParseErrorAction();
                }
            }
            else
            {
                if (_symbolResultTree.ErrorCount > 0 &&
                    _primaryAction.ClearsParseErrors &&
                    _symbolResultTree.Errors is not null)
                {
                    foreach (var kvp in _symbolResultTree)
                    {
                        var symbol = kvp.Key;
                        if (symbol is Option { Action: { } optionAction } option)
                        {
                            if (_primaryAction == optionAction)
                            {
                                var errorsForPrimarySymbol = _symbolResultTree
                                                             .Errors
                                                             .Where(e => e.SymbolResult is OptionResult r && r.Option == option)
                                                             .ToList();

                                _symbolResultTree.Errors = errorsForPrimarySymbol;

                                return;
                            }
                        }

                        if (symbol is Command { Action: { } commandAction } command)
                        {
                            if (_primaryAction == commandAction)
                            {
                                var errorsForPrimarySymbol = _symbolResultTree
                                                             .Errors
                                                             .Where(e => e.SymbolResult is CommandResult r && r.Command == command)
                                                             .ToList();

                                _symbolResultTree.Errors = errorsForPrimarySymbol;

                                return;
                            }
                        }
                    }

                    if (_symbolResultTree.ErrorCount > 0)
                    {
                        _symbolResultTree.Errors?.Clear();
                    }
                }
            }
        }
    }
}