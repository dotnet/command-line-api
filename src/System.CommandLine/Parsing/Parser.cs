// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Parses command line input.
    /// </summary>
    public class Parser
    {
        /// <param name="configuration">The configuration on which the parser's grammar and behaviors are based.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Parser(CommandLineConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (configuration.RootCommand is Command { ImplicitParser: null} cmd)
            {
                cmd.ImplicitParser = this;
            }
        }

        public Parser(Symbol symbol) : this(new CommandLineConfiguration(symbol))
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the Parser class with using the default <seealso cref="RootCommand"/>.
        /// </summary>
        public Parser() : this(new RootCommand())
        {
        }

        /// <summary>
        /// The configuration on which the parser's grammar and behaviors are based.
        /// </summary>
        public CommandLineConfiguration Configuration { get; }

        /// <summary>
        /// Parses a list of arguments.
        /// </summary>
        /// <param name="arguments">The string array typically passed to a program's <c>Main</c> method.</param>
        /// <param name="rawInput">Holds the value of a complete command line input prior to splitting and tokenization, when provided. This will typically not be available when the parser is called from <c>Program.Main</c>. It is primarily used when calculating suggestions via the <c>dotnet-suggest</c> tool.</param>
        /// <returns>A <see cref="ParseResult"/> providing details about the parse operation.</returns>
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
