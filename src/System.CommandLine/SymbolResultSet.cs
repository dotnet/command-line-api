// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public class SymbolResultSet : AliasedSet<SymbolResult>
    {
        internal SymbolResult ResultFor(ISymbol symbol) =>
            Items.SingleOrDefault(i => i.Symbol == symbol);

        protected override IReadOnlyList<string> GetAliases(SymbolResult item) =>
            item.Symbol.Aliases;

        protected override IReadOnlyList<string> GetRawAliases(SymbolResult item) =>
            item.Symbol.RawAliases;
    }
}
