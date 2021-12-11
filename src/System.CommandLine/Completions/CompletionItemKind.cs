// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Completions
{
    internal static class CompletionItemKind
    {
        // reference: https://microsoft.github.io/language-server-protocol/specifications/specification-3-17/#textDocument_completion
        public const string Keyword = nameof(Keyword);
        public const string Value = nameof(Value);
    }
}