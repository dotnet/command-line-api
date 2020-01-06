// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Parsing
{
    public static class ParserExtensions
    {
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
