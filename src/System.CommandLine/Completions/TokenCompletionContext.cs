// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;

namespace System.CommandLine.Completions;

/// <summary>
/// Provides details for getting completions when the complete text of the original command line is not available.
/// </summary>
public class TokenCompletionContext : CompletionContext
{
    internal TokenCompletionContext(ParseResult parseResult) : base(parseResult, GetWordToComplete(parseResult))
    {
    }
}