// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;

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
            IHelpBuilder helpBuilder = null,
            bool isHidden = false) :
            base(new[] { name }, description, isHidden: isHidden)
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

        public void AddCommand(Command command) => AddSymbol(command);

        public void AddOption(Option option) => AddSymbol(option);

        public bool TreatUnmatchedTokensAsErrors { get; set; }

        public ICommandHandler Handler { get; set; }

        public void WriteHelp(IConsole console)
        {
            IHelpBuilder helpBuilder = _helpBuilder ?? new HelpBuilder(console);
            helpBuilder.Write(this);
        }
    }
}
