// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Invocation
{
    public class ParseDirectiveResult : IInvocationResult
    {
        public void Apply(InvocationContext context)
        {
            var tokensAfterDirective = context.ParseResult.Tokens.Skip(1).ToArray();

            var reparseResult = context.ParseResult.Parser.Parse(tokensAfterDirective);

            context.Console.Out.WriteLine(reparseResult.Diagram());
        }
    }
}
