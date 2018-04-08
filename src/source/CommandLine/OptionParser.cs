// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

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

        protected override ParseResult CreateParseResult(
            IReadOnlyCollection<string> rawArgs,
            ParsedOptionSet rootParsedOptions,
            bool isProgressive,
            ParserConfiguration parserConfiguration,
            string[] unparsedTokens,
            List<string> unmatchedTokens,
            List<OptionError> errors)
        {
            return new OptionParseResult(
                rawArgs,
                rootParsedOptions,
                isProgressive,
                parserConfiguration,
                unparsedTokens,
                unmatchedTokens,
                errors);
        }
    }
}
