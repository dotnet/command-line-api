using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParserConfiguration
    {
        public ParserConfiguration(
            IReadOnlyCollection<Symbol> definedSymbols,
            IReadOnlyCollection<char> argumentDelimiters = null,
            bool allowUnbundling = true)
        {
            if (definedSymbols == null)
            {
                throw new ArgumentNullException(nameof(definedSymbols));
            }

            if (!definedSymbols.Any())
            {
                throw new ArgumentException("You must specify at least one option.");
            }

            if (!definedSymbols.OfType<Command>().Any())
            {
                RootCommand = Create.RootCommand(definedSymbols.ToArray());
                DefinedSymbols.Add(RootCommand);
            }
            else
            {
                DefinedSymbols.AddRange(definedSymbols);
            }

            ArgumentDelimiters = argumentDelimiters ?? new[] { ':', '=' };
            AllowUnbundling = allowUnbundling;
        }

        public SymbolSet DefinedSymbols { get; } = new SymbolSet();

        public IReadOnlyCollection<char> ArgumentDelimiters { get; }

        public bool AllowUnbundling { get; }

        internal Command RootCommand { get; }

        internal bool RootCommandIsImplicit => RootCommand != null;
    }
}
