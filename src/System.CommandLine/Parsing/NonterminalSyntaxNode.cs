// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal abstract class NonterminalSyntaxNode : SyntaxNode
    {
        private List<SyntaxNode>? _children;

        protected NonterminalSyntaxNode(Token token) : base(token)
        {
        }

        public IReadOnlyList<SyntaxNode> Children => _children is not null ? _children : Array.Empty<SyntaxNode>();

        internal void AddChildNode(SyntaxNode node) => (_children ??= new()).Add(node);
    }
}
