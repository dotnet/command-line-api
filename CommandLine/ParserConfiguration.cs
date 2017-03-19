using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParserConfiguration
    {
        public ParserConfiguration(
            IReadOnlyCollection<Option> definedOptions,
            IReadOnlyCollection<char> argumentDelimiters = null)
        {
            if (definedOptions == null)
            {
                throw new ArgumentNullException(nameof(definedOptions));
            }

            if (!definedOptions.Any())
            {
                throw new ArgumentException("You must specify at least one option.");
            }

            DefinedOptions.AddRange(definedOptions);
            ArgumentDelimiters = argumentDelimiters ?? new[] { ':', '=' };
        }

        public OptionSet DefinedOptions { get; } = new OptionSet();

        public IReadOnlyCollection<char> ArgumentDelimiters { get; }
    }
}