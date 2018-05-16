// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class SymbolDefinitionSet : SymbolSet<SymbolDefinition>
    {
        protected override bool ContainsSymbolWithAlias(SymbolDefinition symbol, string alias) =>
            symbol.HasAlias(alias);

        protected override bool ContainsSymbolWithRawAlias(SymbolDefinition symbol, string alias) =>
            symbol.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(SymbolDefinition symbol) =>
            symbol.RawAliases;
    }
}
