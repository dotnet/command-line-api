// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Invocation
{
    public class SuggestDirectiveResult : IInvocationResult
    {
        public void Apply(InvocationContext context)
        {
            var tokensAfterDirective = context.ParseResult.Tokens.Skip(1).ToArray();

            var reparseResult = context.Parser.Parse(tokensAfterDirective);

            var suggestions = reparseResult.Suggestions();

            context.Console.Out.WriteLine(
                string.Join(
                    Environment.NewLine,
                    suggestions));
        }
    }
}
