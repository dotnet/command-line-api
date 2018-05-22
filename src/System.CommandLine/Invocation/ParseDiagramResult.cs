// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Invocation
{
    public class ParseDiagramResult : IInvocationResult
    {
        public void Apply(InvocationContext context)
        {
            var tokensAfterCommand = context.ParseResult.Tokens.Skip(1).ToArray();

            var reparseResult = context.ParseResult.Parser.Parse(tokensAfterCommand);

            context.Console.Out.WriteLine(reparseResult.Diagram());
        }
    }
}
