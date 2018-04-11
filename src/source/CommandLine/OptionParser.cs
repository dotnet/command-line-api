// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class OptionParser : Parser
    {
        public OptionParser(params Option[] options) : base(options)
        {
            // FIX: (OptionParser) enforce this at the compiler, i.e. remove inheritance of Command : Option

            foreach (var option in options)
            {
                if (option is Command)
                {
                    throw new ArgumentException($"OptionParser does not accept Command instances but was passed {option}");
                }
            }
        }

        public OptionParser(ParserConfiguration configuration) : base(configuration)
        {
        }

        public OptionParseResult Parse(string[] args) => Parse(args, null);

        internal OptionParseResult Parse(
            string[] args,
            string input)
        {
            var raw = base.Parse(args, input);

            return new OptionParseResult(
                raw.RawTokens,
                raw.Parsed,
                raw.Configuration,
                raw.UnparsedTokens,
                raw.UnmatchedTokens,
                raw.Errors,
                raw.RawInput);
        }
    }
}
