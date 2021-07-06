// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Collections
{
    /// <summary>
    /// A set of symbols that can be looked up by alias.
    /// </summary>
    public interface ISymbolSet : IReadOnlyList<ISymbol>
    {
        /// <summary>
        /// Gets a symbol from the collection using any of its aliases, including its name.
        /// </summary>
        /// <param name="alias">The alias to look up a symbol by.</param>
        /// <returns>A symbol if one is present having a matching name or alias; otherwise, <see langword="null"/>.</returns>
        ISymbol? GetByAlias(string alias);
    }
}
