// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Completions
{
    /// <summary>
    /// Provides command line completion.
    /// </summary>
    /// <returns>A list of completions.</returns>
    public delegate IEnumerable<CompletionItem> CompletionDelegate(CompletionContext context);
}