// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static Microsoft.DotNet.Cli.CommandLine.Accept;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class Command : Symbol
    {
        private static readonly Lazy<string> executableName =
            new Lazy<string>(() => Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));

        public Command(
            IReadOnlyCollection<Symbol> symbols) :
            this(executableName.Value, "", symbols, NoArguments())
        {
        }

        public Command(
            string name,
            string description,
            IReadOnlyCollection<Command> subcommands) :
            this(name, description, symbols: subcommands)
        {
            var commandNames = subcommands.SelectMany(o => o.Aliases).ToArray();

            ArgumentsRule =
                ExactlyOneCommandRequired()
                    .WithSuggestionsFrom(commandNames)
                    .And(ArgumentsRule);
        }

        public Command(
            string name,
            string description,
            IReadOnlyCollection<Symbol> symbols = null,
            ArgumentsRule arguments = null,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description, arguments, symbols)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;

            if (symbols != null && symbols.Any())
            {
                foreach (var option in symbols)
                {
                    option.Parent = this;
                    DefinedSymbols.Add(option);
                }

                ArgumentsRule = ArgumentsRule.And(ZeroOrMoreOf(symbols.ToArray()));
            }
        }

        public bool TreatUnmatchedTokensAsErrors { get; } = true;

        public override string ToString() => Name;
    }
}
