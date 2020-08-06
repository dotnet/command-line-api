// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal abstract class SyntaxNode
    {
        protected SyntaxNode(
            Token token,
            SyntaxNode? parent)
        {
            Token = token;
            Parent = parent;
        }

        public SyntaxNode? Parent { get; }

        public Token Token { get; }

        public override string ToString() => Token.Value;
    }
}
