// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal abstract class NonterminalSyntaxNode : SyntaxNode
    {
        private readonly List<SyntaxNode> _children = new List<SyntaxNode>();

        protected NonterminalSyntaxNode(Token token, SyntaxNode? parent) : base(token, parent)
        {
        }

        public IReadOnlyList<SyntaxNode> Children => _children;

        internal void AddChildNode(SyntaxNode node) => _children.Add(node);
    }
}
