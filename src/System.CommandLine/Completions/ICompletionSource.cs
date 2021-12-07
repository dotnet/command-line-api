// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Completions
{
    /// <summary>
    /// Provides completions and example values for help.
    /// </summary>
    public interface ICompletionSource
    {
        /// <summary>
        /// Gets the suggested values for command line completion.
        /// </summary>
        IEnumerable<CompletionItem> GetCompletions(CompletionContext context);
    }
}