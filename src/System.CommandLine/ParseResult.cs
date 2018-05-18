// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.CommandLine.ValidationMessages;

namespace System.CommandLine
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class ParseResult
    {
        private readonly ParserConfiguration configuration;
        private readonly List<ParseError> errors = new List<ParseError>();
        private CommandDefinition _commandDefinition;

        internal ParseResult(
            IReadOnlyCollection<string> tokens,
            SymbolSet symbols,
            ParserConfiguration configuration,
            IReadOnlyCollection<string> unparsedTokens = null,
            IReadOnlyCollection<string> unmatchedTokens = null,
            IReadOnlyCollection<ParseError> errors = null,
            string rawInput = null)
        {
            Tokens = tokens ??
                     throw new ArgumentNullException(nameof(tokens));
            Symbols = symbols ??
                            throw new ArgumentNullException(nameof(symbols));
            this.configuration = configuration ??
                                 throw new ArgumentNullException(nameof(configuration));

            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;
            RawInput = rawInput;

            if (errors != null)
            {
                this.errors.AddRange(errors);
            }

            CheckForErrors();
        }

        public SymbolSet Symbols { get; }

        public IReadOnlyCollection<ParseError> Errors => errors;

        public IReadOnlyCollection<string> Tokens { get; }

        public IReadOnlyCollection<string> UnmatchedTokens { get; }

        internal string RawInput { get; }

        public IReadOnlyCollection<string> UnparsedTokens { get; }

        public CommandDefinition SpecifiedCommandDefinition() =>
            _commandDefinition ??
            (_commandDefinition = configuration.RootCommandIsImplicit
                           ? configuration.SymbolDefinitions.OfType<CommandDefinition>().Single()
                           : Symbols.CommandDefinition());

        private void CheckForErrors()
        {
            foreach (var option in Symbols.FlattenBreadthFirst())
            {
                var error = option.Validate();

                if (error != null)
                {
                    errors.Add(error);
                }
            }

            var commandDefinition = SpecifiedCommandDefinition();

            if (commandDefinition != null &&
                commandDefinition.SymbolDefinitions.OfType<CommandDefinition>().Any())
            {
                errors.Insert(0, new ParseError(
                                  RequiredCommandWasNotProvided(),
                                  this.SpecifiedCommand()));
            }
        }

        public override string ToString() => this.Diagram();

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

            return Symbols[alias].GetValueOrDefault<T>();
        }

        public Symbol this[string alias] => Symbols[alias]; 
    }
}
