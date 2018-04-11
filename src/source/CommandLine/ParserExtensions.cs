// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class ParserExtensions
    {
        public static CommandParseResult Parse(
            this CommandParser parser,
            string input) =>
            parser.Parse(input.Tokenize().ToArray(), input);

        public static OptionParseResult Parse(
            this OptionParser parser,
            string input) =>
            parser.Parse(input.Tokenize().ToArray(), input);
    }
}
