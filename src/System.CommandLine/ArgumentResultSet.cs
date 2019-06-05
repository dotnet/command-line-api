// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    internal class ArgumentResultSet : AliasedSet<ArgumentResult>
    {
        protected override bool ContainsItemWithAlias(ArgumentResult item, string alias)
        {
            return item.Argument.Name.Equals(alias);
        }

        protected override bool ContainsItemWithRawAlias(ArgumentResult item, string alias)
        {
            return item.Argument.Name.Equals(alias);
        }

        protected override IReadOnlyCollection<string> GetAliases(ArgumentResult item)
        {
            return new[] { item.Argument.Name };
        }
    }
}
