// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public class SymbolResultSet : AliasedSet<SymbolResult>
    {
        internal SymbolResult? ResultFor(ISymbol symbol) =>
            Items.FirstOrDefault(i => Equals(i.Symbol, symbol));

        protected override IReadOnlyCollection<string> GetAliases(SymbolResult result) =>
            result.Symbol switch
            {
                INamedSymbol named => named.Aliases,
                _ => new[] { result.Symbol.Name }
            };
    }
}