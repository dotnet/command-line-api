// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;

namespace System.CommandLine.Invocation
{
    internal static class SuggestDirectiveResult
    {
        internal static void Apply(InvocationContext context, int position)
        {
            var commandLineToComplete = context.ParseResult.Tokens.LastOrDefault(t => t.Type != TokenType.Directive)?.Value ?? "";

            var completionParseResult = context.ParseResult.RootCommandResult.Command.Parse(commandLineToComplete, context.ParseResult.Configuration);

            var completions = completionParseResult.GetCompletions(position);

            context.Console.Out.WriteLine(
                string.Join(
                    Environment.NewLine,
                    completions));
        }
    }
}
