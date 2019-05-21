// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

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
            var normalizedArgs = NormalizeRootCommand(arguments);
            var tokenizeResult = normalizedArgs.Tokenize(Configuration);

            var operation = new ParseOperation(
                tokenizeResult,
                Configuration);

            operation.Parse();

            var visitor = new ParseResultVisitor(
                this,
                tokenizeResult,
                rawInput);

            visitor.Visit(operation.RootCommandNode);

            return visitor.Result;
        }
    }
}
