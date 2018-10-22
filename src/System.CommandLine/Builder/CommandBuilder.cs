// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;

namespace System.CommandLine.Builder
{
    public class CommandBuilder : SymbolBuilder
    {
        private readonly Lazy<List<Command>> _builtCommands = new Lazy<List<Command>>();
        private readonly Lazy<List<Option>> _builtOptions = new Lazy<List<Option>>();

        public CommandBuilder(
            string name,
            CommandBuilder parent = null) : base(parent)
        {
            Name = name;
        }

        public OptionBuilderSet Options { get; } = new OptionBuilderSet();

        public CommandBuilderSet Commands { get; } = new CommandBuilderSet();

        public bool? TreatUnmatchedTokensAsErrors { get; set; }

        internal ICommandHandler Handler { get; set; }

        public string Name { get; }

        public IHelpBuilder HelpBuilder { get; set; }

        internal void AddCommand(Command command) => _builtCommands.Value.Add(command);

        internal void AddOption(Option option) => _builtOptions.Value.Add(option);

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
                handler: Handler,
                helpBuilder: HelpBuilder);
        }

        protected IReadOnlyCollection<Symbol> BuildChildSymbols()
        {
            var subcommands = Commands
                .Select(b =>
                {
                    b.TreatUnmatchedTokensAsErrors = TreatUnmatchedTokensAsErrors;
                    return b.BuildCommand();
                });

            if (_builtCommands.IsValueCreated)
            {
                subcommands = subcommands.Concat(_builtCommands.Value);
            }

            var options = Options
                .Select(b => b.BuildOption());

            if (_builtOptions.IsValueCreated)
            {
                options = options.Concat(_builtOptions.Value);
            }

            return subcommands.Concat<Symbol>(options).ToArray();
        }
    }
}
