// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine
{
    public class Command : Symbol, ICommand, ISuggestionSource
    {
        private readonly IHelpBuilder _helpBuilder;

        public Command(
            string name,
            string description,
            Argument argument,
            bool treatUnmatchedTokensAsErrors = true,
            IHelpBuilder helpBuilder = null) :
            base(new[] { name }, description, argument)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
            _helpBuilder = helpBuilder;
        }

        public Command(
            string name,
            string description,
            IReadOnlyCollection<ISymbol> symbols = null,
            Argument argument = null,
            bool treatUnmatchedTokensAsErrors = true,
            ICommandHandler handler = null,
            IHelpBuilder helpBuilder = null) :
            base(new[] { name }, description)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
            Handler = handler;
            _helpBuilder = helpBuilder;
            symbols = symbols ?? Array.Empty<ISymbol>();

            var validSymbolAliases = symbols
                                     .SelectMany(o => o.RawAliases)
                                     .ToArray();

            ArgumentBuilder builder;
            if (argument == null)
            {
                builder = new ArgumentBuilder();
            }
            else
            {
                builder = ArgumentBuilder.From(argument);
            }

            builder.ValidTokens.UnionWith(validSymbolAliases);

            if (argument == null)
            {
                Argument = builder.ZeroOrMore();
            }
            else
            {
                Argument = argument;
            }

            foreach (var symbol in symbols.OfType<Symbol>())
            {
                symbol.Parent = this;
                Children.Add(symbol);
            }
        }

        public bool TreatUnmatchedTokensAsErrors { get; }

        internal ICommandHandler Handler { get; }
        
        public void WriteHelp(IConsole console)
        {
            IHelpBuilder helpBuilder = _helpBuilder ?? new HelpBuilder(console);
            helpBuilder.Write(this);
        }
    }
}
