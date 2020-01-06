// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Invocation
{
    internal class ParseDirectiveResult : IInvocationResult
    {
        public void Apply(InvocationContext context)
        {
            var parseResult = context.ParseResult;
            context.Console.Out.WriteLine(parseResult.Diagram());
            context.ResultCode = parseResult.Errors.Count == 0
                                     ? 0
                                     : 1;
        }
    }
}
