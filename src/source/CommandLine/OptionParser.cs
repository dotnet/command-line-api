// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class OptionParser : Parser
    {
        public OptionParser(params Option[] options) : base(options)
        {
        }

        public OptionParser(ParserConfiguration configuration) : base(configuration)
        {
        }

        public OptionParseResult Parse(IReadOnlyCollection<string> args) => Parse(args, null);

        internal OptionParseResult Parse(
            IReadOnlyCollection<string> args,
            string input)
        {
            var raw = base.Parse(args, input);

            return new OptionParseResult(
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
