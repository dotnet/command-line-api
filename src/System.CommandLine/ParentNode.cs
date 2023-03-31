// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal sealed class ParentNode
    {
        internal ParentNode(CliSymbol symbol) => Symbol = symbol;

        internal CliSymbol Symbol { get; }

        internal ParentNode? Next { get; set; }
    }
}