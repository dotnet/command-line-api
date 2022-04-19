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
        }

        /// <param name="command">The root command for the parser.</param>
        public Parser(Command command) : this(new CommandLineConfiguration(command))
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Parser" /> class using the default <see cref="RootCommand" />.
        /// </summary>
        public Parser() : this(new RootCommand())
        {
        }

        /// <summary>
        /// Gets the configuration on which the parser's grammar and behaviors are based.
        /// </summary>
        public CommandLineConfiguration Configuration { get; }

        /// <summary>
        /// Parses a list of arguments.
        /// </summary>
        /// <param name="arguments">The string array typically passed to a program's <c>Main</c> method.</param>
        /// <param name="rawInput">The complete command line input prior to splitting and tokenization. This input is not typically available when the parser is called from <c>Program.Main</c>. It is primarily used when calculating completions via the <c>dotnet-suggest</c> tool.</param>
        /// <returns>A <see cref="ParseResult"/> providing details about the parse operation.</returns>
        public ParseResult Parse(
            IReadOnlyList<string> arguments,
            string? rawInput = null)
        {
            var tokenizeResult = arguments.Tokenize(
                Configuration,
                inferRootCommand: rawInput is not null);

            var operation = new ParseOperation(
                tokenizeResult,
                Configuration);

            operation.Parse();

            var visitor = new ParseResultVisitor(
                this,
                tokenizeResult,
                operation.UnparsedTokens,
                operation.UnmatchedTokens,
                rawInput);

            visitor.Visit(operation.RootCommandNode!);

            return visitor.GetResult();
        }
    }
}
