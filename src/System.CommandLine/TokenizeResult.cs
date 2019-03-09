// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    internal class TokenizeResult
    {
        public TokenizeResult(
            IReadOnlyCollection<Token> tokens = null,
            IReadOnlyCollection<TokenizeError> errors = null)
        {
            Tokens = tokens ?? Array.Empty<Token>();
            Errors = errors ?? Array.Empty<TokenizeError>();
        }

        public IReadOnlyCollection<Token> Tokens { get; }

        public IReadOnlyCollection<TokenizeError> Errors { get; }
    }
}
