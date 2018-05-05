// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class Suggestions
    {
        internal static IEnumerable<string> Containing(
            this IEnumerable<string> candidates,
            string textToMatch) =>
            candidates.Where(c => c.ContainsCaseInsensitive(textToMatch));
    }
}
