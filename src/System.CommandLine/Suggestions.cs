// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    internal static class Suggestions
    {
        public static IEnumerable<string> Containing(
            this IEnumerable<string> candidates,
            string textToMatch) =>
            candidates.Where(c => c.ContainsCaseInsensitive(textToMatch));
    }
}
