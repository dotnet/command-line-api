// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace System.CommandLine.Help
{
    internal class HelpResult : IInvocationResult
    {
        /// <inheritdoc />
        public void Apply(InvocationContext context)
        {
            context.BindingContext
                   .HelpBuilder
                   .Write(context.ParseResult.CommandResult.Command, StandardStreamWriter.Create(context.Console.Out));
        }
    }
}
