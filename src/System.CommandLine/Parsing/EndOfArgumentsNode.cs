// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal class EndOfArgumentsNode : SyntaxNode
    {
        public EndOfArgumentsNode(
            Token token,
            SyntaxNode parent) : base(token, parent)
        {
            if (token.Type != TokenType.EndOfArguments)
            {
                throw new ArgumentException($"Incorrect token type: {token}");
            }
        }
    }
}
