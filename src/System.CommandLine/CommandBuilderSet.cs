// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;

namespace System.CommandLine
{
    public class CommandBuilderSet : AliasedSet<CommandBuilder>
    {
        protected override bool ContainsItemWithAlias(CommandBuilder item, string alias) =>
            item.Name == alias;

        protected override bool ContainsItemWithRawAlias(CommandBuilder item, string alias) =>
            item.Name == alias;

        protected override IReadOnlyCollection<string> GetAliases(CommandBuilder item) =>
            new[] { item.Name };
    }
}
