// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine.Invocation
{
    public class InvocationContext
    {
        public InvocationContext(ParseResult parseResult)
        {
            ParseResult = parseResult;
        }

        public ParseResult ParseResult { get; }

        public IInvocationResult InvocationResult { get; set; }

        public TextWriter Output { get; set; } = Console.Out;
    }
}
