// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;

namespace System.CommandLine
{
    public class OptionDefinitionBuilderSet : AliasedSet<OptionDefinitionBuilder>
    {
        protected override bool ContainsItemWithAlias(OptionDefinitionBuilder item, string alias) =>
            item.Aliases.Any(c => c == alias);

        protected override bool ContainsItemWithRawAlias(OptionDefinitionBuilder item, string alias) =>
            item.Aliases.Any(c => c.RemovePrefix() == alias);

        protected override IReadOnlyCollection<string> GetAliases(OptionDefinitionBuilder item) =>
            item.Aliases;
    }
}
