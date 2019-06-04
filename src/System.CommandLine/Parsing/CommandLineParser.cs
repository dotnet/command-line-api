// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    public class CommandLineParser : Parser
    {
        public CommandLineParser(CommandLineConfiguration configuration) : base(configuration)
        {
        }

        public override ParseResult Parse(
            IReadOnlyList<string> arguments,
            string rawInput = null)
        {
            var tokenizeResult = arguments.Tokenize(Configuration);

            var operation = new ParseOperation(
                tokenizeResult,
                Configuration);

            operation.Parse();

            var visitor = new ParseResultVisitor(
                this,
                tokenizeResult,
                operation.UnparsedTokens,
                operation.UnmatchedTokens,
                operation.Errors,
                rawInput);

            visitor.Visit(operation.RootCommandNode);

            return visitor.Result;
        }
    }
}
