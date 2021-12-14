// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Collections;
using System.Linq;

namespace System.CommandLine.Tests
{
    internal static class SymbolSetExtensions
    {
        internal static ISymbol GetByAlias(this SymbolSet symbolSet, string alias)
            => symbolSet.SingleOrDefault(symbol => symbol.Matches(alias));

        internal static bool ContainsAlias(this SymbolSet symbolSet, string alias)
            => symbolSet.Any(symbol => symbol.Matches(alias));
    }
}