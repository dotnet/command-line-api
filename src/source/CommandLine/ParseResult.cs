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
    public class ParseResult
    {
        private readonly ParserConfiguration configuration;
        private readonly List<OptionError> errors = new List<OptionError>();
        private Command command;

        internal ParseResult(
            IReadOnlyCollection<string> tokens,
            AppliedOptionSet appliedOptions,
            bool isProgressive,
            ParserConfiguration configuration,
            IReadOnlyCollection<string> unparsedTokens = null,
            IReadOnlyCollection<string> unmatchedTokens = null,
            IReadOnlyCollection<OptionError> errors = null)
        {
            Tokens = tokens ??
                     throw new ArgumentNullException(nameof(tokens));
            AppliedOptions = appliedOptions ??
                             throw new ArgumentNullException(nameof(appliedOptions));
            this.configuration = configuration ??
                                 throw new ArgumentNullException(nameof(configuration));

            IsProgressive = isProgressive;
            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;

            if (errors != null)
            {
                this.errors.AddRange(errors);
            }

            CheckForErrors();
        }

        public AppliedOptionSet AppliedOptions { get; }

        public IEnumerable<OptionError> Errors => errors;

        public bool IsProgressive { get; }

        public IReadOnlyCollection<string> Tokens { get; }

        public IReadOnlyCollection<string> UnmatchedTokens { get; }

        public IReadOnlyCollection<string> UnparsedTokens { get; }

        public AppliedOption this[string alias] => AppliedOptions[alias];

        public Command Command() =>
            command ??
            (command = configuration.RootCommandIsImplicit
                           ? configuration.DefinedOptions.OfType<Command>().Single()
                           : AppliedOptions.Command());

        private void CheckForErrors()
        {
            foreach (var option in AppliedOptions.FlattenBreadthFirst())
            {
                var error = option.Validate();

                if (error != null)
                {
                    errors.Add(error);
                }
            }

            var command = Command();

            if (command != null &&
                command.DefinedOptions.Any(o => o.IsCommand))
            {
                errors.Insert(0, new OptionError(
                                  RequiredCommandWasNotProvided(),
                                  command.Name,
                                  this.AppliedCommand()));
            }
        }

        public override string ToString() => this.Diagram();
    }
}
