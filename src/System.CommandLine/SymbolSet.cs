// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class SymbolSet : SymbolSet<Symbol>
    {
        protected override bool ContainsItemWithAlias(Symbol symbol, string alias) =>
            symbol.HasAlias(alias);

        protected override bool ContainsItemWithRawAlias(Symbol symbol, string alias) =>
            symbol.HasRawAlias(alias);

        protected override IReadOnlyCollection<string> RawAliasesFor(Symbol symbol) =>
            symbol.RawAliases;
    }
}