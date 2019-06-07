// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    internal class ArgumentConversionResultSet : AliasedSet<ArgumentConversionResult>
    {
        protected override bool ContainsItemWithAlias(ArgumentConversionResult item, string alias)
        {
            return item.Argument.Name.Equals(alias);
        }

        protected override bool ContainsItemWithRawAlias(ArgumentConversionResult item, string alias)
        {
            return item.Argument.Name.Equals(alias);
        }

        protected override IReadOnlyCollection<string> GetAliases(ArgumentConversionResult item)
        {
            return new[] { item.Argument.Name };
        }
    }
}
