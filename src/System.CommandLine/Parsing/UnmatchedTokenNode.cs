// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal class UnmatchedTokenNode : SyntaxNode
    {
        public UnmatchedTokenNode(
            Token token,
            RootCommandNode parent) : base(token, parent)
        {
        }
    }
}
