// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class SymbolSet : AliasedSet<Symbol>
    {
        protected override bool ContainsItemWithAlias(Symbol item, string alias) =>
            item.SymbolDefinition.HasAlias(alias);

        protected override bool ContainsItemWithRawAlias(Symbol item, string alias) =>
            item.SymbolDefinition.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> GetAliases(Symbol item) =>
            item.SymbolDefinition.RawAliases;
    }

}
