// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal sealed class ParseOperation
    {
        private readonly List<CliToken> _tokens;
        private readonly CliConfiguration _configuration;
        private readonly string? _rawInput;
        private readonly SymbolResultTree _symbolResultTree;
        private readonly CliCommandResultInternal _rootCommandResult;

        private int _index;
        private CliCommandResultInternal _innermostCommandResult;
        /*
        private bool _isHelpRequested;
        private bool _isTerminatingDirectiveSpecified;
        */
// TODO: invocation
/*
        private CliAction? _primaryAction;
        private List<CliAction>? _preActions;
*/
        public ParseOperation(
            List<CliToken> tokens,
            CliCommand rootCommand,
            CliConfiguration configuration,
            List<string>? tokenizationErrors,
            string? rawInput)
        {
            _tokens = tokens;
            _configuration = configuration;
            _rawInput = rawInput;
            _symbolResultTree = new(rootCommand, tokenizationErrors);

            _innermostCommandResult = _rootCommandResult = new CliCommandResultInternal(
                rootCommand,
                CurrentToken,
                _symbolResultTree);
            _symbolResultTree.Add(rootCommand, _rootCommandResult);

            Advance();
        }

        private CliToken CurrentToken => _tokens[_index];

        private void Advance() => _index++;

        private bool More(out CliTokenType currentTokenType)
        {
            bool result = _index < _tokens.Count;
            currentTokenType = result ? _tokens[_index].Type : (CliTokenType)(-1);
            return result;
        }

        internal ParseResult Parse()
        {
// TODO: directives
/*
            ParseDirectives();
*/
            ParseCommandChildren();
            /*
            if (!_isHelpRequested)
            {
                Validate();
            }
            */

// TODO: invocation
/*
            if (_primaryAction is null)
            {
                if (_symbolResultTree.ErrorCount > 0)
                {
                    _primaryAction = new CliDiagnosticAction();
                }
            }
*/

            return new(
                _configuration,
                _rootCommandResult,
                _innermostCommandResult,
                _rootCommandResult.SymbolResultTree.BuildValueResultDictionary(),
                /*
                _tokens,
                */
// TODO: unmatched tokens
//                _symbolResultTree.UnmatchedTokens,
                _symbolResultTree.Errors,
                _rawInput
// TODO: invocation
/*
                _primaryAction,
                _preActions);
*/
                );
        }

        private void ParseSubcommand()
        {
            CliCommand command = (CliCommand)CurrentToken.Symbol!;

            _innermostCommandResult = new CliCommandResultInternal(
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

            while (More(out CliTokenType currentTokenType))
            {
                if (currentTokenType == CliTokenType.Command)
                {
                    ParseSubcommand();
                }
                else if (currentTokenType == CliTokenType.Option)
                {
                    ParseOption();
                }
                else if (currentTokenType == CliTokenType.Argument)
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
            while (More(out CliTokenType currentTokenType) && currentTokenType == CliTokenType.Argument)
            {
                while (_innermostCommandResult.Command.HasArguments && currentArgumentIndex < _innermostCommandResult.Command.Arguments.Count)
                {
                    CliArgument argument = _innermostCommandResult.Command.Arguments[currentArgumentIndex];

                    if (currentArgumentCount < argument.Arity.MaximumNumberOfValues)
                    {
                        if (CurrentToken.Symbol is null)
                        {
                            // update the token with missing information now, so later stages don't need to modify it
                            CurrentToken.Symbol = argument;
                        }

                        if (!(_symbolResultTree.TryGetValue(argument, out var symbolResult)
                            && symbolResult is CliArgumentResultInternal argumentResult))
                        {
                            argumentResult =
                                new CliArgumentResultInternal(
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
            CliOption option = (CliOption)CurrentToken.Symbol!;
            CliOptionResultInternal optionResult;

            if (!_symbolResultTree.TryGetValue(option, out CliSymbolResultInternal? symbolResult))
            {
// TODO: invocation, directives, help
/*
                if (option.Action is not null)
                {
                    // directives have a precedence over --help and --version
                    if (!_isTerminatingDirectiveSpecified)
                    {
                        if (option is HelpOption)
                        {
                            _isHelpRequested = true;
                        }

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
*/
                optionResult = new CliOptionResultInternal(
                    option,
                    _symbolResultTree,
                    CurrentToken,
                    _innermostCommandResult);

                _symbolResultTree.Add(option, optionResult);
            }
            else
            {
                optionResult = (CliOptionResultInternal)symbolResult;
            }

// TODO: IdentifierTokenCount
//            optionResult.IdentifierTokenCount++;

            Advance();

            ParseOptionArguments(optionResult);
        }

        private void ParseOptionArguments(CliOptionResultInternal optionResult)
        {
            var argument = optionResult.Option.Argument;

            var contiguousTokens = 0;
            int argumentCount = 0;

            while (More(out CliTokenType currentTokenType) && currentTokenType == CliTokenType.Argument)
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

                if (!(_symbolResultTree.TryGetValue(argument, out CliSymbolResultInternal? symbolResult)
                    && symbolResult is CliArgumentResultInternal argumentResult))
                {
                    argumentResult = new CliArgumentResultInternal(
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
                    var argumentResult = new CliArgumentResultInternal(argument, _symbolResultTree, optionResult);
                    _symbolResultTree.Add(argument, argumentResult);
                }
            }
        }
// TODO: directives
/*
        private void ParseDirectives()
        {
            while (More(out CliTokenType currentTokenType) && currentTokenType == CliTokenType.Directive)
            {
                if (_configuration.HasDirectives)
                {
                    ParseDirective(); // kept in separate method to avoid JIT
                }

                Advance();
            }

            void ParseDirective()
            {
                var token = CurrentToken;

                if (token.Symbol is not CliDirective directive)
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

        private void AddPreAction(CliAction action)
        {
            if (_preActions is null)
            {
                _preActions = new();
            }

            _preActions.Add(action);
        }
*/

        private void AddCurrentTokenToUnmatched()
        {
            if (CurrentToken.Type == CliTokenType.DoubleDash)
            {
                return;
            }

            _symbolResultTree.AddUnmatchedToken(CurrentToken, _innermostCommandResult, _rootCommandResult);
        }

        // TODO: Validation
        /*
        private void Validate()
        {
            // Only the inner most command goes through complete validation,
            // for other commands only a subset of options is checked.
            _innermostCommandResult.Validate(completeValidation: true);

            CliCommandResultInternal? currentResult = _innermostCommandResult.Parent as CliCommandResultInternal;
            while (currentResult is not null)
            {
                currentResult.Validate(completeValidation: false);

                currentResult = currentResult.Parent as CliCommandResultInternal;
            }
        }
        */
    }
}
