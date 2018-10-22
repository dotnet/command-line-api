// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;

namespace System.CommandLine
{
    public class OptionBuilderSet : AliasedSet<OptionBuilder>
    {
        protected override bool ContainsItemWithAlias(OptionBuilder item, string alias) =>
            item.Aliases.Any(c => c == alias);

        protected override bool ContainsItemWithRawAlias(OptionBuilder item, string alias) =>
            item.Aliases.Any(c => c.RemovePrefix() == alias);

        protected override IReadOnlyCollection<string> GetAliases(OptionBuilder item) =>
            item.Aliases;
    }
}
