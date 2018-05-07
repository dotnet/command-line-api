// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace System.CommandLine
{
    internal class NullSuggestionSource : ISuggestionSource
    {
        private NullSuggestionSource()
        {
        }

        public IEnumerable<string> Suggest(ParseResult parseResult, int? position = null) => Array.Empty<string>();

        public static ISuggestionSource Instance { get; } = new NullSuggestionSource();
    }
}
