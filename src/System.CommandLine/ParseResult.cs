// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class ParseResult
    {
        private readonly List<ParseError> _errors = new List<ParseError>();

        internal ParseResult(
            Command rootCommand,
            Command command,
            IReadOnlyCollection<string> tokens,
            IReadOnlyCollection<string> unparsedTokens,
            IReadOnlyCollection<string> unmatchedTokens,
            IReadOnlyCollection<ParseError> errors,
            string rawInput)
        {
            RootCommand = rootCommand;
            Command = command;
            Tokens = tokens;
            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;
            RawInput = rawInput;

            if (errors != null)
            {
                _errors.AddRange(errors);
            }

            CheckForErrors();
        }

        public Command Command { get; }

        public Command RootCommand { get; }

        public IReadOnlyCollection<ParseError> Errors => _errors;

        public IReadOnlyCollection<string> Tokens { get; }

        public IReadOnlyCollection<string> UnmatchedTokens { get; }

        internal string RawInput { get; }

        public IReadOnlyCollection<string> UnparsedTokens { get; }

        public CommandDefinition CommandDefinition => Command.Definition;

        private void CheckForErrors()
        {
            foreach (var option in RootCommand.AllSymbols())
            {
                var error = option.Validate();

                if (error != null)
                {
                    _errors.Add(error);
                }
            }

            var commandDefinition = CommandDefinition;

            if (commandDefinition != null &&
                commandDefinition.SymbolDefinitions.OfType<CommandDefinition>().Any())
            {
                var symbol = Command;
                _errors.Insert(0, new ParseError(
                                  symbol.ValidationMessages.RequiredCommandWasNotProvided(),
                                  symbol));
            }
        }

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

            return this[alias].GetValueOrDefault<T>();
        }

        public Symbol this[string alias] => Command.Children[alias];
    }
}
