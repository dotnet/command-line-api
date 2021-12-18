// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Parsing
{
    internal class OptionNode : NonterminalSyntaxNode
    {
        public OptionNode(
            Token token,
            Option option,
            CommandNode parent) : base(token, parent)
        {
            Option = option;
        }

        public Option Option { get; }
    }
}