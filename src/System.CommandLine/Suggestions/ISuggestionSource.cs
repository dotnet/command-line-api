// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Suggestions
{
    /// <summary>
    /// Provides suggestions for tab completion and example values for help.
    /// </summary>
    public interface ISuggestionSource
    {
        // FIX: (ISuggestionSource) rename this and associated types and namespaces
        /// <summary>
        /// Gets the suggested values for the given parse result and input text.
        /// </summary>
        IEnumerable<CompletionItem> GetSuggestions(CompletionContext context);
    }
}