// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    public class OptionParseResult : ParseResult
    {
        internal OptionParseResult(
            IReadOnlyCollection<string> tokens,
            SymbolSet options,
            ParserConfiguration configuration,
            IReadOnlyCollection<string> unparsedTokens = null,
            IReadOnlyCollection<string> unmatchedTokens = null,
            IReadOnlyCollection<ParseError> errors = null,
            string rawInput = null) : base(tokens, options, configuration, unparsedTokens, unmatchedTokens, errors, rawInput)
        {
        }

        public Option this[string alias] => (Option) Symbols[alias];
    }
}
