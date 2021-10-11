// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;

namespace System.CommandLine.Parsing
{
    /// <summary>
    /// Provides extension methods for parsers.
    /// </summary>
    public static class ParserExtensions
    {
        /// <summary>
        /// Parses a command line string value and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        public static int Invoke(
            this Parser parser,
            string commandLine,
            IConsole? console = null) =>
            parser.Invoke(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console);

        /// <summary>
        /// Parses a command line string array and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        public static int Invoke(
            this Parser parser,
            string[] args,
            IConsole? console = null) =>
            parser.Parse(args).Invoke(console);

        /// <summary>
        /// Parses a command line string value and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        public static Task<int> InvokeAsync(
            this Parser parser,
            string commandLine,
            IConsole? console = null) =>
            parser.InvokeAsync(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console);

        /// <summary>
        /// Parses a command line string array and invokes the handler for the indicated command.
        /// </summary>
        /// <returns>The exit code for the invocation.</returns>
        public static async Task<int> InvokeAsync(
            this Parser parser,
            string[] args,
            IConsole? console = null) =>
            await parser.Parse(args).InvokeAsync(console);

        /// <summary>
        /// Parses a command line string.
        /// </summary>
        /// <remarks>The command line string input will be split into tokens as if it had been passed on the command line.</remarks>
        public static ParseResult Parse(
            this Parser parser,
            string commandLine)
        {
            var splitter = CommandLineStringSplitter.Instance;

            var readOnlyCollection = splitter.Split(commandLine).ToArray();

            return parser.Parse(readOnlyCollection, commandLine);
        }
    }
}