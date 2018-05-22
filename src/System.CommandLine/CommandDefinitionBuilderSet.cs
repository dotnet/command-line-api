// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;

namespace System.CommandLine
{
    public class CommandDefinitionBuilderSet : AliasedSet<CommandDefinitionBuilder>
    {
        protected override bool ContainsItemWithAlias(CommandDefinitionBuilder item, string alias) =>
            item.Name == alias;

        protected override bool ContainsItemWithRawAlias(CommandDefinitionBuilder item, string alias) =>
            item.Name == alias;

        protected override IReadOnlyCollection<string> GetAliases(CommandDefinitionBuilder item) =>
            new[] { item.Name };
    }
}
