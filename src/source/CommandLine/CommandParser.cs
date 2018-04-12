// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class CommandParser : Parser
    {
        public CommandParser(params Command[] commands) : base(commands)
        {
        }

        public CommandParser(ParserConfiguration configuration) : base(configuration)
        {
        }

        public CommandParseResult Parse(string[] args) => Parse(args, null);

        internal CommandParseResult Parse(
            string[] args, 
            string input)
        {
            var raw = base.Parse(args, input);

            return new CommandParseResult(
                raw.RawTokens,
                raw.ParsedSymbol,
                raw.Configuration,
                raw.UnparsedTokens,
                raw.UnmatchedTokens,
                raw.Errors, 
                raw.RawInput);
        }
    }
}
