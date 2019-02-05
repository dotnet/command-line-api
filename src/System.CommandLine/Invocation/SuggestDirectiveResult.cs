// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Invocation
{
    internal class SuggestDirectiveResult : IInvocationResult
    {
        private readonly int? position;

        public SuggestDirectiveResult(int? position)
        {
            this.position = position;
        }

        public void Apply(InvocationContext context)
        {
            var suggestions = context.ParseResult.Suggestions(position);

            context.Console.Out.WriteLine(
                string.Join(
                    Environment.NewLine,
                    suggestions));
        }
    }
}
