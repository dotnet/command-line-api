// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class ParserExtensions
    {
        public static ParseResult Parse(this Parser parser, string s) =>
            parser.Parse(s.Tokenize().ToArray(),
                         isProgressive: !s.EndsWith(" "));
    }
}