// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Collections;

namespace System.CommandLine.Parsing
{
    public class SymbolResultSet : AliasedSet<SymbolResult>
    {
        internal SymbolResult? ResultFor(ISymbol symbol)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (Equals(item.Symbol, symbol))
                {
                    return item;
                }
            }

            return default;
        }

        protected override IReadOnlyCollection<string> GetAliases(SymbolResult result) =>
            result.Symbol switch
            {
                IIdentifierSymbol named => named.Aliases,
                _ => new[] { result.Symbol.Name }
            };
    }
}