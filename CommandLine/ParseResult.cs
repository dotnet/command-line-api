using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParseResult
    {
        private readonly Parser parser;
        private readonly List<OptionError> errors = new List<OptionError>();

        internal ParseResult(
            Parser parser,
            IReadOnlyCollection<string> tokens,
            OptionSet<AppliedOption> appliedOptions,
            bool isProgressive,
            IReadOnlyCollection<string> unparsedTokens = null,
            IReadOnlyCollection<string> unmatchedTokens = null,
            IReadOnlyCollection<OptionError> errors = null)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            if (appliedOptions == null)
            {
                throw new ArgumentNullException(nameof(appliedOptions));
            }

            this.parser = parser;

            Tokens = tokens;
            AppliedOptions = appliedOptions;
            IsProgressive = isProgressive;
            UnparsedTokens = unparsedTokens;
            UnmatchedTokens = unmatchedTokens;

            if (errors != null)
            {
                this.errors.AddRange(errors);
            }

            CheckForOptionErrors();
        }

        public OptionSet<AppliedOption> AppliedOptions { get; }

        public IEnumerable<OptionError> Errors => errors;

        public bool IsProgressive { get; }

        public IReadOnlyCollection<string> Tokens { get; }

        public IReadOnlyCollection<string> UnmatchedTokens { get; }

        public IReadOnlyCollection<string> UnparsedTokens { get; }

        public AppliedOption this[string alias] => AppliedOptions[alias];

        public bool HasOption(string alias) => AppliedOptions.Contains(alias);

        private void CheckForOptionErrors()
        {
            foreach (var option in AppliedOptions.FlattenBreadthFirst())
            {
                var error = option.Validate();

                if (error != null)
                {
                    errors.Add(error);
                }
            }
        }

        public override string ToString() => this.Diagram();
    }
}