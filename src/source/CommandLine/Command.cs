// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class Command : Symbol
    {
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

        public Command(
            IReadOnlyCollection<Symbol> symbols) :
            this(executableName.Value, "", symbols)
        {
        }

        public Command(
            string name,
            string description,
            ArgumentsRule arguments,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description, arguments)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
        }

        public Command(
            string name,
            string description,
            IReadOnlyCollection<Symbol> symbols,
            ArgumentsRule arguments = null,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;

            var validSymbolAliases = symbols
                                     .SelectMany(o => o.RawAliases)
                                     .ToArray();

            var suggestableSymbolAliases = symbols
                                           .Where(s => !s.IsHidden())
                                           .SelectMany(o => o.RawAliases)
                                           .ToArray();

            ArgumentRuleBuilder builder;
            if (arguments == null)
            {
                builder = new ArgumentRuleBuilder();
            }
            else
            {
                builder = ArgumentRuleBuilder.From(arguments);
            }

            builder.ValidTokens.UnionWith(validSymbolAliases);
            builder.Suggestions.AddRange(suggestableSymbolAliases);

            if (arguments == null)
            {
                ArgumentsRule = builder.ZeroOrMore();
            }
            else
            {
                // FIX: (Command) 
                switch (arguments.Parser.ArgumentArity)
                {
                    case ArgumentArity.One:
                        ArgumentsRule = arguments;
                        break;
                    case ArgumentArity.Many:
                        ArgumentsRule = arguments;
                        break;
                    case ArgumentArity.Zero:
                        ArgumentsRule = arguments;
                        break;
                }
            }

            foreach (Symbol symbol in symbols)
            {
                symbol.Parent = this;
                DefinedSymbols.Add(symbol);
            }
        }

        public bool TreatUnmatchedTokensAsErrors { get; }

        public override string ToString() => Name;
    }
}
