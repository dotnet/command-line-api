﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Suggestions
{
    internal class AnonymousSuggestionSource : ISuggestionSource
    {
        private readonly SuggestDelegate _suggest;

        public AnonymousSuggestionSource(SuggestDelegate suggest)
        {
            _suggest = suggest ?? throw new ArgumentNullException(nameof(suggest));
        }

        public IEnumerable<string> GetSuggestions(string? textToMatch = null)
        {
            return _suggest(textToMatch);
        }
    }
}
