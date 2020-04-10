// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    public class HelpResult : IInvocationResult
    {
        public void Apply(InvocationContext context)
        {
            context.BindingContext
                   .HelpBuilder
                   .Write(context.ParseResult.CommandResult.Command);

            // indicate it's not an error but it's not a normal result, either;
            // it's short-circuited
            context.ResultCode = 2;
        }
    }
}
