﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public class ParseResult
    {
        private readonly List<ParseError> _errors;
        private readonly RootCommandResult _rootCommandResult;

        internal ParseResult(
            Parser parser,
            RootCommandResult rootCommandResult,
            CommandResult commandResult,
            IDirectiveCollection directives,
            TokenizeResult tokenizeResult,
            IReadOnlyCollection<string> unparsedTokens,
            IReadOnlyCollection<string> unmatchedTokens,
            List<ParseError> errors = null,
            string rawInput = null)
        {
            Parser = parser;
            _rootCommandResult = rootCommandResult;
            CommandResult = commandResult;
            Directives = directives;

            // skip the root command
            Tokens = tokenizeResult.Tokens.Skip(1).ToArray();

            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;

            RawInput = rawInput;

            _errors = errors ?? new List<ParseError>();

            if (parser.Configuration.RootCommand.TreatUnmatchedTokensAsErrors)
            {
                _errors.AddRange(
                    unmatchedTokens.Select(token =>
                                               new ParseError(parser.Configuration.ValidationMessages.UnrecognizedCommandOrArgument(token))));
            }
        }

        public CommandResult CommandResult { get; }

        public Parser Parser { get; }

        public CommandResult RootCommandResult => _rootCommandResult;

        public IReadOnlyCollection<ParseError> Errors => _errors;

        public IDirectiveCollection Directives { get; }

        public IReadOnlyList<Token> Tokens { get; }

        public IReadOnlyCollection<string> UnmatchedTokens { get; }

        internal string RawInput { get; }

        public IReadOnlyCollection<string> UnparsedTokens { get; }

        public object ValueForOption(
            string alias) =>
            ValueForOption<object>(alias);

        public T ValueForOption<T>(
            string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
            }

            if (this[alias] is OptionResult optionResult)
            {
                return optionResult.GetValueOrDefault<T>();
            }
            else
            {
                return default;
            }
        }

        public SymbolResult this[string alias] => CommandResult.Children[alias];

        public override string ToString() => $"{nameof(ParseResult)}: {this.Diagram()}";

        public ArgumentResult FindResultFor(IArgument argument) =>
            _rootCommandResult.FindResultFor(argument);
            
        public CommandResult FindResultFor(ICommand command) =>
            _rootCommandResult.FindResultFor(command);

        public OptionResult FindResultFor(IOption option) =>
            _rootCommandResult.FindResultFor(option);
    }
}
