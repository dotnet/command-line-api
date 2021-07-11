// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing
{
    internal class TokenizeResult
    {
        internal TokenizeResult(
            IReadOnlyList<Token> tokens,
            IReadOnlyList<TokenizeError> errors)
        {
            Tokens = tokens;
            Errors = errors;
        }

        public IReadOnlyList<Token> Tokens { get; }

        public IReadOnlyList<TokenizeError> Errors { get; }
    }
}
