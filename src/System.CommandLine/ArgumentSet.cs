// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    internal class ArgumentSet : AliasedSet<Argument>
    {
        internal void Clear() => Items.Clear();

        protected override bool ContainsItemWithAlias(Argument item, string alias)
        {
            return item.Name.Equals(alias);
        }

        protected override bool ContainsItemWithRawAlias(Argument item, string alias)
        {
            return item.Name.Equals(alias);
        }

        protected override IReadOnlyCollection<string> GetAliases(Argument item)
        {
            return new[] { item.Name };
        }
    }
}
