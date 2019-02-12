// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine
{
    public static class ParserExtensions
    {
        public static ParseResult Parse(
            this Parser parser,
            string input) =>
            parser.Parse(input.Tokenize().ToArray(), input);
    }
}
