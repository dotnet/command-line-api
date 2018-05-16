// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class SymbolSet : SymbolSet<Symbol>
    {
        protected override bool ContainsSymbolWithAlias(Symbol symbol, string alias) =>
            symbol.HasAlias(alias);

        protected override bool ContainsSymbolWithRawAlias(Symbol symbol, string alias) =>
            symbol.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(Symbol symbol) =>
            symbol.RawAliases;
    }
}
