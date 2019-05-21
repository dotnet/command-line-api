// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal class ArgumentNode : SyntaxNode
    {
        public ArgumentNode(Token token, CommandNode parent) : base(token, parent)
        {
        }

        public ArgumentNode(Token token, OptionNode parent) : base(token, parent)
        {
        }
    }
}
