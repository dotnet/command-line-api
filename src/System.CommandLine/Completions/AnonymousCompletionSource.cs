// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Completions
{
    internal class AnonymousCompletionSource : ICompletionSource
    {
        private readonly CompletionDelegate _complete;

        public AnonymousCompletionSource(CompletionDelegate complete)
        {
            _complete = complete ?? throw new ArgumentNullException(nameof(complete));
        }

        public AnonymousCompletionSource(Func<CompletionContext, IEnumerable<string>> complete)
        {
            _complete = context => complete(context).Select(value => new CompletionItem(value));
        }

        public IEnumerable<CompletionItem> GetCompletions(CompletionContext context)
        {
            return _complete(context);
        }
    }
}