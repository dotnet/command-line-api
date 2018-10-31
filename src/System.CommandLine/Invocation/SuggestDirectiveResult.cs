// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Invocation
{
    public class SuggestDirectiveResult : IInvocationResult
    {
        public void Apply(InvocationContext context)
        {
            var suggestions = context.ParseResult.Suggestions();

            context.Console.Out.WriteLine(
                string.Join(
                    Environment.NewLine,
                    suggestions));
        }
    }
}
