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
            IReadOnlyList<string>? arguments,
            string? rawInput = null)
        {
            arguments ??= Array.Empty<string>();

            arguments.Tokenize(
                Configuration,
                inferRootCommand: rawInput is not null,
                out List<Token> tokens,
                out List<string>? tokenizationErrors);

            var operation = new ParseOperation(
                tokens,
                Configuration,
                tokenizationErrors,
                rawInput);

            return operation.Parse(this);
        }
    }
}
