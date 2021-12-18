// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Collections;
using System.Linq;

namespace System.CommandLine.Tests
{
    internal static class SymbolSetExtensions
    {
        internal static Symbol GetByAlias(this SymbolSet symbolSet, string alias)
            => symbolSet.SingleOrDefault(symbol => symbol.Name.Equals(alias) || symbol is IdentifierSymbol id && id.HasAlias(alias));

        internal static bool ContainsAlias(this SymbolSet symbolSet, string alias)
            => symbolSet.OfType<IdentifierSymbol>().Any(symbol => symbol.HasAlias(alias));
    }
}