// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine
{
    public class Command : Symbol, ICommand, IEnumerable<Symbol>
    {
        public Command(
            string name,
            string description = "",
            IReadOnlyCollection<Symbol> symbols = null,
            Argument argument = null,
            bool treatUnmatchedTokensAsErrors = true,
            ICommandHandler handler = null,
            bool isHidden = false) :
            base(new[] { name }, description, isHidden: isHidden)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
            Handler = handler;
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

        public void AddCommand(Command command) => AddSymbol(command);

        public void AddOption(Option option) => AddSymbol(option);

        public void Add(Symbol symbol) => AddSymbol(symbol);

        public bool TreatUnmatchedTokensAsErrors { get; set; }

        public ICommandHandler Handler { get; set; }

        public IEnumerator<Symbol> GetEnumerator() => Children.OfType<Symbol>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
