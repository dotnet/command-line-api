// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Builder
{
    public class OptionDefinitionBuilder : SymbolDefinitionBuilder
    {
        public OptionDefinitionBuilder(
            IReadOnlyCollection<string> aliases,
            CommandDefinitionBuilder parent) : base(parent)
        {
            Aliases.AddRange(aliases);
        }

        public List<string> Aliases { get; } = new List<string>();

        public OptionDefinition BuildOptionDefinition()
        {
            return new OptionDefinition(Aliases, Description, BuildArguments());
        }
    }
}
