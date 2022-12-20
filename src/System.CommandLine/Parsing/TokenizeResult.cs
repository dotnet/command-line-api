// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing;

internal class TokenizeResult
{
    internal TokenizeResult(
        List<Token> tokens,
        List<string>? errors)
    {
        Tokens = tokens;
        Errors = errors is null ? Array.Empty<string>() : errors;
    }

    public List<Token> Tokens { get; }

    public IReadOnlyList<string> Errors { get; }
}