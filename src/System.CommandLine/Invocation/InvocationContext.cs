// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public class InvocationContext
    {
        public InvocationContext(
            ParseResult parseResult,
            IConsole console)
        {
            ParseResult = parseResult;
            Console = console;
        }

        public ParseResult ParseResult { get; }

        public IConsole Console { get; }

        public IInvocationResult InvocationResult { get; set; }
    }
}
