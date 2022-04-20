// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Parsing;

/// <summary>
/// Replaces a token with one or more other tokens prior to parsing.
/// </summary>
public delegate bool TryReplaceToken(
    string tokenToReplace,
    out IReadOnlyList<string>? replacementTokens, 
    out string? errorMessage);