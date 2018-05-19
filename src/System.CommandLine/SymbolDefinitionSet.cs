// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class SymbolDefinitionSet : AliasedSet<SymbolDefinition>
    {
        protected override bool ContainsItemWithAlias(SymbolDefinition item, string alias) =>
            item.HasAlias(alias);

        protected override bool ContainsItemWithRawAlias(SymbolDefinition item, string alias) =>
            item.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> GetAliases(SymbolDefinition item) =>
            item.RawAliases;
    }
}
