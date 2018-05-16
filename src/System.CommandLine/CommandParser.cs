// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class CommandParser : SymbolParser
    {
        public CommandParser(params CommandDefinition[] commandDefinitions) : base(commandDefinitions)
        {
        }

        public CommandParser(ParserConfiguration configuration) : base(configuration)
        {
        }

        public CommandParseResult Parse(IReadOnlyCollection<string> args) => Parse(args, null);

        internal CommandParseResult Parse(
            IReadOnlyCollection<string> args, 
            string input)
        {
            var raw = ParseRaw(args, input);

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
