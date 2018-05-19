// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine
{
    internal static class SymbolSetExtensions
    {
        internal static CommandDefinition CommandDefinition(this SymbolSet symbols) =>
            symbols.FlattenBreadthFirst()
                   .Select(a => a.SymbolDefinition)
                   .OfType<CommandDefinition>()
                   .LastOrDefault();
    }
}
