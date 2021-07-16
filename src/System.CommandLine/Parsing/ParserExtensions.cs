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
        public static int Invoke(
            this Parser parser,
            string commandLine,
            IConsole? console = null) =>
            parser.Invoke(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console);

        public static int Invoke(
            this Parser parser,
            string[] args,
            IConsole? console = null) =>
            parser.Parse(args).Invoke(console);

        public static Task<int> InvokeAsync(
            this Parser parser,
            string commandLine,
            IConsole? console = null) =>
            parser.InvokeAsync(CommandLineStringSplitter.Instance.Split(commandLine).ToArray(), console);

        public static async Task<int> InvokeAsync(
            this Parser parser,
            string[] args,
            IConsole? console = null) =>
            await parser.Parse(args).InvokeAsync(console);

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