// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Invocation
{
    public class ParseDirectiveResult : IInvocationResult
    {
        public void Apply(InvocationContext context)
        {
            context.Console.Out.WriteLine(context.ParseResult.Diagram());
        }
    }
}
