// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing;

internal class TokenizeResult
{
    internal TokenizeResult(
        List<Token> tokens,
        List<string> errors)
    {
        Tokens = tokens;
        Errors = errors;
    }

    public List<Token> Tokens { get; }

    public List<string> Errors { get; }
}