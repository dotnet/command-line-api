// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace System.CommandLine.Parsing
{
    internal class DirectiveNode : SyntaxNode
    {
        public DirectiveNode(
            Token token,
            string name,
            string? value) : base(token)
        {
            Debug.Assert(token.Type == TokenType.Directive, $"Incorrect token type: {token}");

            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string? Value { get; }
    }
}
