// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;

namespace System.CommandLine
{
    public class CommandDefinition : SymbolDefinition
    {
        public CommandDefinition(
            string name,
            string description,
            ArgumentDefinition argumentDefinition,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description, argumentDefinition)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
        }

        public CommandDefinition(
            string name,
            string description,
            IReadOnlyCollection<SymbolDefinition> symbolDefinitions = null,
            ArgumentDefinition argumentDefinition = null,
            bool treatUnmatchedTokensAsErrors = true) :
            base(new[] { name }, description)
        {
            TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;

            symbolDefinitions = symbolDefinitions ?? Array.Empty<SymbolDefinition>();

            var validSymbolAliases = symbolDefinitions
                                     .SelectMany(o => o.RawAliases)
                                     .ToArray();

            ArgumentDefinitionBuilder builder;
            if (argumentDefinition == null)
            {
                builder = new ArgumentDefinitionBuilder();
            }
            else
            {
                builder = ArgumentDefinitionBuilder.From(argumentDefinition);
            }

            builder.ValidTokens.UnionWith(validSymbolAliases);

            if (argumentDefinition == null)
            {
                ArgumentDefinition = builder.ZeroOrMore();
            }
            else
            {
                ArgumentDefinition = argumentDefinition;
            }

            foreach (SymbolDefinition symbol in symbolDefinitions)
            {
                symbol.Parent = this;
                SymbolDefinitions.Add(symbol);
            }
        }

        public bool TreatUnmatchedTokensAsErrors { get; }
    }
}
