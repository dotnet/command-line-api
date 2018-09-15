// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine
{
    public class Command : Symbol
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
            IReadOnlyCollection<Symbol> symbols = null,
            Argument argument = null,
            bool treatUnmatchedTokensAsErrors = true,
            MethodBinder executionHandler = null,
            IHelpBuilder helpBuilder = null) :
            base(new[] { name }, description)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
            ExecutionHandler = executionHandler;
            _helpBuilder = helpBuilder;
            symbols = symbols ?? Array.Empty<Symbol>();

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

            foreach (Symbol symbol in symbols)
            {
                symbol.Parent = this;
                Symbols.Add(symbol);
            }
        }

        public bool TreatUnmatchedTokensAsErrors { get; }

        internal MethodBinder ExecutionHandler { get; }
        
        public void WriteHelp(IConsole console)
        {
            IHelpBuilder helpBuilder = _helpBuilder ?? new HelpBuilder(console);
            helpBuilder.Write(this);
        }
    }
}
