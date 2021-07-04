// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    /// <summary>
    /// Defines a symbol which is an identifier on the command line, such as a <see cref="ICommand">command</see> or <see cref="IOption">option</see>.
    /// </summary>
    public interface IIdentifierSymbol : ISymbol
    {
        /// <summary>
        /// Gets the set of strings that can be used on the command line to specify the symbol.
        /// </summary>
        IReadOnlyCollection<string> Aliases { get; }

        /// <summary>
        /// Determines whether the alias has already been defined.
        /// </summary>
        /// <param name="alias">The alias to search for.</param>
        /// <returns><see langkeyword="true">true</see> if the alias has already been defined; otherwise <see langkeyword="true">false</see>.</returns>
        bool HasAlias(string alias);
    }
}