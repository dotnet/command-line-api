// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Suggestions
{
    internal class AnonymousSuggestionSource : ISuggestionSource
    {
        private readonly SuggestDelegate _suggest;

        public AnonymousSuggestionSource(SuggestDelegate suggest)
        {
            _suggest = suggest ?? throw new ArgumentNullException(nameof(suggest));
        }

        public AnonymousSuggestionSource(Func<CompletionContext, IEnumerable<string>> suggest)
        {
            _suggest = context => suggest(context).Select(value => new CompletionItem(value));
        }

        public IEnumerable<CompletionItem> GetSuggestions(CompletionContext context)
        {
            return _suggest(context);
        }
    }
}