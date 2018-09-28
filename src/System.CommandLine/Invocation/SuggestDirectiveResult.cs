// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Invocation
{
    public class SuggestDirectiveResult : IInvocationResult
    {
        public void Apply(InvocationContext context)
        {
            var tokensAfterDirective = context.ParseResult.Tokens.Skip(1).ToArray();
            (int? positionIndex, var tokensAfterTakenOutPositionOption) = ProcessPositionOption(tokensAfterDirective);

            var reparseResult = context.Parser.Parse(tokensAfterTakenOutPositionOption.ToArray());

            var suggestions = reparseResult.Suggestions(positionIndex);

            context.Console.Out.WriteLine(
                string.Join(
                    Environment.NewLine,
                    suggestions));
        }

        private static (int? positionIndex, IEnumerable<string> tokensAfterTakenOutPositionOption) ProcessPositionOption(string[] tokensAfterDirective)
        {
            int? positionIndex = null;
            IEnumerable<string> tokensAfterTakenOutPositionOption;

            if (tokensAfterDirective.Length >= 2 && (tokensAfterDirective[0] == "--position" || tokensAfterDirective[0] == "-p"))
            {
                var indexstring = tokensAfterDirective[1];
                if (int.TryParse(indexstring, out var index))
                {
                    positionIndex = index;
                    tokensAfterTakenOutPositionOption = tokensAfterDirective.Skip(2);
                }
                else
                {
                    tokensAfterTakenOutPositionOption = tokensAfterDirective.Skip(1);
                }
            }
            else
            {
                tokensAfterTakenOutPositionOption = tokensAfterDirective;
            }

            return (positionIndex, tokensAfterTakenOutPositionOption);
        }
    }
}
