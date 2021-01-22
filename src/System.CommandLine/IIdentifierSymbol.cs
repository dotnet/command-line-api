// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    /// <summary>
    /// Defines a symbol with aliases.
    /// </summary>
    public interface IIdentifierSymbol : ISymbol
    {
        /// <summary>
        /// Gets the aliases for the symbol.
        /// </summary>
        IReadOnlyCollection<string> Aliases { get; }

        /// <summary>
        /// Determines whether the alias has already been defined.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns><c>true</c> if the alias has already been defined; otherwise <c>false</c>.</returns>
        bool HasAlias(string alias);
    }
}