// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    public class Parser
    {
        public Parser(CommandLineConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Parser(params Symbol[] symbols) : this(new CommandLineConfiguration(symbols))
        {
        }

        public Parser() : this(new RootCommand())
        {
        }

        public CommandLineConfiguration Configuration { get; }

        public ParseResult Parse(
            IReadOnlyList<string> arguments,
            string? rawInput = null)
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

            visitor.Visit(operation.RootCommandNode!);

            return visitor.Result;
        }
    }
}
