// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class SymbolResultSet : AliasedSet<SymbolResult>
    {
        protected override bool ContainsItemWithAlias(SymbolResult item, string alias) =>
            item.Symbol.HasAlias(alias);

        protected override bool ContainsItemWithRawAlias(SymbolResult item, string alias) =>
            item.Symbol.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> GetAliases(SymbolResult item) =>
            item.Symbol.RawAliases;
    }
}
