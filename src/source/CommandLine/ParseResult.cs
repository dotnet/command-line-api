// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Microsoft.DotNet.Cli.CommandLine.ValidationMessages;

namespace Microsoft.DotNet.Cli.CommandLine
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public abstract class ParseResult
    {
        private readonly ParserConfiguration configuration;
        private readonly List<OptionError> errors = new List<OptionError>();
        private Command command;

        internal ParseResult(
            IReadOnlyCollection<string> tokens,
            ParsedSymbolSet parsedOptions,
            ParserConfiguration configuration,
            IReadOnlyCollection<string> unparsedTokens = null,
            IReadOnlyCollection<string> unmatchedTokens = null,
            IReadOnlyCollection<OptionError> errors = null,
            string rawInput = null)
        {
            Tokens = tokens ??
                     throw new ArgumentNullException(nameof(tokens));
            ParsedSymbol = parsedOptions ??
                            throw new ArgumentNullException(nameof(parsedOptions));
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

        public ParsedSymbolSet ParsedSymbol { get; }

        public IReadOnlyCollection<OptionError> Errors => errors;

        public IReadOnlyCollection<string> Tokens { get; }

        public IReadOnlyCollection<string> UnmatchedTokens { get; }

        internal string RawInput { get; }

        public IReadOnlyCollection<string> UnparsedTokens { get; }

        public Command Command() =>
            command ??
            (command = configuration.RootCommandIsImplicit
                           ? configuration.DefinedSymbols.OfType<Command>().Single()
                           : ParsedSymbol.Command());

        private void CheckForErrors()
        {
            foreach (var option in ParsedSymbol.FlattenBreadthFirst())
            {
                var error = option.Validate();

                if (error != null)
                {
                    errors.Add(error);
                }
            }

            var command = Command();

            if (command != null &&
                command.DefinedSymbols.OfType<Command>().Any())
            {
                ParsedCommand parsedCommand = null;

                if (this is CommandParseResult commandParseResult)
                {
                    // FIX: (CheckForErrors) this is ugly
                    parsedCommand = commandParseResult.ParsedCommand();
                }

                errors.Insert(0, new OptionError(
                                  RequiredCommandWasNotProvided(),
                                  command.Name,
                                  parsedCommand));
            }
        }

        public override string ToString() => this.Diagram();
    }
}
