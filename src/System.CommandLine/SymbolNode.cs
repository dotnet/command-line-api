// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal sealed class SymbolNode
    {
        internal SymbolNode(Symbol symbol, Command? parent = null)
        {
            Symbol = symbol;
            Parent = parent;
        }

        internal Symbol Symbol { get; }

        internal Command? Parent { get; }

        internal SymbolNode? Next { get; set; }
    }
}