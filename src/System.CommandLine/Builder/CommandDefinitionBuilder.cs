// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Builder
{
    public class CommandDefinitionBuilder : SymbolDefinitionBuilder
    {
        public CommandDefinitionBuilder(
            string name,
            CommandDefinitionBuilder parent = null) : base(parent)
        {
            Name = name;
        }

        protected internal List<OptionDefinitionBuilder> OptionDefinitionBuilders { get; } = new List<OptionDefinitionBuilder>();

        protected internal List<CommandDefinitionBuilder> CommandDefinitionBuilders { get; } = new List<CommandDefinitionBuilder>();

        public bool? TreatUnmatchedTokensAsErrors { get; set; }

        public string Name { get; }

        public virtual CommandDefinition BuildCommandDefinition()
        {
            return new CommandDefinition(
                Name,
                Description,
                argumentDefinition: BuildArguments(),
                symbolDefinitions: BuildChildSymbolDefinitions(),
                treatUnmatchedTokensAsErrors: TreatUnmatchedTokensAsErrors ??
                                              Parent?.TreatUnmatchedTokensAsErrors ??
                                              true);
        }

        protected IReadOnlyCollection<SymbolDefinition> BuildChildSymbolDefinitions()
        {
            var subcommands = CommandDefinitionBuilders
                .Select(b => {
                    b.TreatUnmatchedTokensAsErrors = TreatUnmatchedTokensAsErrors;
                    return b.BuildCommandDefinition();
                });

            var options = OptionDefinitionBuilders
                .Select(b => b.BuildOptionDefinition());

            return subcommands.Concat<SymbolDefinition>(options).ToArray();
        }
    }
}
