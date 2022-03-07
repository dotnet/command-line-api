// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Invocation
{
    internal class SuggestDirectiveResult : IInvocationResult
    {
        private readonly int _position;

        public SuggestDirectiveResult(int position)
        {
            _position = position;
        }

        public void Apply(InvocationContext context)
        {
            var commandLineToComplete = context.ParseResult.Tokens.LastOrDefault(t => t.Type != TokenType.Directive)?.Value ?? "";

            var completionParseResult = context.Parser.Parse(commandLineToComplete);

            var completions = completionParseResult.GetCompletions(_position);

            context.Console.Out.WriteLine(
                string.Join(
                    Environment.NewLine,
                    completions));
        }
    }
}
