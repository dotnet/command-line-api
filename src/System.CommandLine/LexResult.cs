// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    internal class LexResult
    {
        public LexResult(
            IReadOnlyCollection<Token> tokens,
            IReadOnlyCollection<ParseError> errors)
        {
            Tokens = tokens;
            Errors = errors;
        }

        public IReadOnlyCollection<Token> Tokens { get; }
        public IReadOnlyCollection<ParseError> Errors { get; }
    }
}
