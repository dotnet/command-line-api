// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Suggestions
{
    /// <summary>
    /// Provides suggestions for command line completion.
    /// </summary>
    /// <returns>A list of suggestions.</returns>
    public delegate IEnumerable<CompletionItem> SuggestDelegate(CompletionContext context);
}