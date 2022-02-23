// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal class DirectiveNode : SyntaxNode
    {
        public DirectiveNode(
            Token token,
            CommandNode parent,
            string name,
            string? value) : base(token, parent)
        {
            if (token.Type != TokenType.Directive)
            {
                throw new ArgumentException($"Incorrect token type: {token}");
            }

            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string? Value { get; }
    }
}
