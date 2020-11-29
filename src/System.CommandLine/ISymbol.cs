// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Collections;
using System.CommandLine.Suggestions;

namespace System.CommandLine
{
    /// <summary>
    /// Defines a named symbol that resides in a hierarchy with parent and child symbols.
    /// </summary>
    public interface ISymbol : ISuggestionSource
    {
        /// <summary>
        /// Gets the symbol name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the symbol description.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Gets a value that indicates whether the symbol is hidden.
        /// </summary>
        bool IsHidden { get; }

        /// <summary>
        /// Gets the child symbols.
        /// </summary>
        ISymbolSet Children { get; }

        /// <summary>
        /// Gets the parent symbols.
        /// </summary>
        ISymbolSet Parents { get; }
    }
}