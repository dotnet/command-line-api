// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    internal class AnonymousSuggestionSource : ISuggestionSource
    {
        private readonly Suggest suggest;

        public AnonymousSuggestionSource(Suggest suggest)
        {
            this.suggest = suggest;
        }

        public IEnumerable<string> Suggest(string textToMatch = null)
        {
            return suggest(textToMatch);
        }
    }
}
