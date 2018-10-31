// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine
{
    public class Command : Symbol, ICommand, ISuggestionSource
    {
        private readonly IHelpBuilder _helpBuilder;

        public Command(
            string name,
            string description = "",
            IReadOnlyCollection<Symbol> symbols = null,
            Argument argument = null,
            bool treatUnmatchedTokensAsErrors = true,
            ICommandHandler handler = null,
            IHelpBuilder helpBuilder = null) :
            base(new[] { name }, description)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
            Handler = handler;
            _helpBuilder = helpBuilder;
            symbols = symbols ?? Array.Empty<Symbol>();

            Argument = argument ??
                       new Argument
                       {
                           Arity = ArgumentArity.Zero
                       };

            foreach (var symbol in symbols)
            {
                AddSymbol(symbol);
            }
        }
  
        private void AddSymbol(Symbol symbol)
        {
            symbol.Parent = this;
            Children.Add(symbol);
        }

        public bool TreatUnmatchedTokensAsErrors { get; set; }

        internal ICommandHandler Handler { get; }

        public void WriteHelp(IConsole console)
        {
            IHelpBuilder helpBuilder = _helpBuilder ?? new HelpBuilder(console);
            helpBuilder.Write(this);
        }
    }
}
