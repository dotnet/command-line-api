// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal sealed class ParentNode
    {
        internal ParentNode(Symbol symbol) => Symbol = symbol;

        internal Symbol Symbol { get; }

        internal ParentNode? Next { get; set; }
    }
}