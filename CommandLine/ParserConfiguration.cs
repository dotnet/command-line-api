using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParserConfiguration
    {
        public ParserConfiguration(
            IReadOnlyCollection<Option> definedOptions,
            IReadOnlyCollection<char> argumentDelimiters = null,
            bool allowUnbundling = true)
        {
            if (definedOptions == null)
            {
                throw new ArgumentNullException(nameof(definedOptions));
            }

            if (!definedOptions.Any())
            {
                throw new ArgumentException("You must specify at least one option.");
            }

            if (definedOptions.All(o => !o.IsCommand))
            {
                RootCommand = Create.RootCommand(definedOptions.ToArray());
                DefinedOptions.Add(RootCommand);
            }
            else
            {
                DefinedOptions.AddRange(definedOptions);
            }

            ArgumentDelimiters = argumentDelimiters ?? new[] { ':', '=' };
            AllowUnbundling = allowUnbundling;
        }

        public OptionSet DefinedOptions { get; } = new OptionSet();

        public IReadOnlyCollection<char> ArgumentDelimiters { get; }

        public bool AllowUnbundling { get; }

        internal Option RootCommand { get; }

        internal bool RootCommandIsImplicit => RootCommand != null;
    }
}
