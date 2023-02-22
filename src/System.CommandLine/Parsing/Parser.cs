// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Parses command line input.
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// Parses a list of arguments.
        /// </summary>
        /// <param name="command">The command to use to parse the command line input.</param>
        /// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
        /// <param name="configuration">The configuration on which the parser's grammar and behaviors are based.</param>
        /// <returns>A <see cref="ParseResult"/> providing details about the parse operation.</returns>
        public static ParseResult Parse(Command command, IReadOnlyList<string> args, CommandLineConfiguration? configuration = null)
            => Parse(command, args, null, configuration);

        /// <summary>
        /// Parses a command line string.
        /// </summary>
        /// <param name="command">The command to use to parse the command line input.</param>
        /// <param name="commandLine">The complete command line input prior to splitting and tokenization. This input is not typically available when the parser is called from <c>Program.Main</c>. It is primarily used when calculating completions via the <c>dotnet-suggest</c> tool.</param>
        /// <param name="configuration">The configuration on which the parser's grammar and behaviors are based.</param>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        /// <returns>A <see cref="ParseResult"/> providing details about the parse operation.</returns>
        public static ParseResult Parse(Command command, string commandLine, CommandLineConfiguration? configuration = null)
        {
            var splitter = CommandLineStringSplitter.Instance;

            var readOnlyCollection = splitter.Split(commandLine).ToArray();

            return Parse(command, readOnlyCollection, commandLine, configuration);
        }

        private static ParseResult Parse(
            Command command,
            IReadOnlyList<string> arguments,
            string? rawInput,
            CommandLineConfiguration? configuration)
        {
            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            configuration ??= CommandLineConfiguration.CreateBuilder(command).UseDefaults().Build();

            arguments.Tokenize(
                configuration,
                inferRootCommand: rawInput is not null,
                out List<Token> tokens,
                out List<string>? tokenizationErrors);

            var operation = new ParseOperation(
                tokens,
                configuration,
                tokenizationErrors,
                rawInput);

            return operation.Parse();
        }
    }
}
