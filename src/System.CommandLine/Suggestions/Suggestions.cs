// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace System.CommandLine.Suggestions
{
    internal static class SuggestionExtensions
    {
        public static IEnumerable<string> Containing(
            this IEnumerable<string> candidates,
            string textToMatch)
        {
            foreach (var candidate in candidates)
            {
                if (candidate is { } && 
                    candidate.ContainsCaseInsensitive(textToMatch))
                {
                    yield return candidate;
                }
            }
        }
    }
}