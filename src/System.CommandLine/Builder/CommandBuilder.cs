// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine.Builder
{
    public class CommandBuilder : SymbolBuilder
    {
        public CommandBuilder(
            string name,
            CommandBuilder parent = null) : base(parent)
        {
            Name = name;
        }

        public OptionBuilderSet Options { get; } = new OptionBuilderSet();

        public CommandBuilderSet Commands { get; } = new CommandBuilderSet();

        public bool? TreatUnmatchedTokensAsErrors { get; set; }

        internal IBinderInvoker ExecutionHandler { get; set; }

        public string Name { get; }

        public IHelpBuilder HelpBuilder { get; set; }

        public Command BuildCommand()
        {
            return new Command(
                Name,
                Description,
                argument: BuildArguments(),
                symbols: BuildChildSymbols(),
                treatUnmatchedTokensAsErrors: TreatUnmatchedTokensAsErrors ??
                                              Parent?.TreatUnmatchedTokensAsErrors ??
                                              true,
                executionHandler: ExecutionHandler,
                helpBuilder: HelpBuilder);
        }

        protected IReadOnlyCollection<Symbol> BuildChildSymbols()
        {
            var subcommands = Commands
                .Select(b => {
                    b.TreatUnmatchedTokensAsErrors = TreatUnmatchedTokensAsErrors;
                    return b.BuildCommand();
                });

            var options = Options
                .Select(b => b.BuildOption());

            return subcommands.Concat<Symbol>(options).ToArray();
        }
    }
}
