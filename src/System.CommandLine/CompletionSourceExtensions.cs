// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Completions;
using System.Linq;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for working with completion sources.
    /// </summary>
    public static class CompletionSourceExtensions
    {
        /// <summary>
        /// Adds a completion source using a delegate.
        /// </summary>
        /// <param name="completionSources">The list of completion sources to add to.</param>
        /// <param name="complete">The delegate to be called when calculating completions.</param>
        public static void Add(
            this CompletionSourceList completionSources,
            Func<CompletionContext, IEnumerable<string>> complete)
        {
            if (completionSources is null)
            {
                throw new ArgumentNullException(nameof(completionSources));
            }

            if (complete is null)
            {
                throw new ArgumentNullException(nameof(complete));
            }

            completionSources.Add(new AnonymousCompletionSource(complete));
        }
        
        /// <summary>
        /// Adds a completion source using a delegate.
        /// </summary>
        /// <param name="completionSources">The list of completion sources to add to.</param>
        /// <param name="complete">The delegate to be called when calculating completions.</param>
        public static void Add(
            this CompletionSourceList completionSources,
            CompletionDelegate complete)
        {
            if (completionSources is null)
            {
                throw new ArgumentNullException(nameof(completionSources));
            }

            if (complete is null)
            {
                throw new ArgumentNullException(nameof(complete));
            }

            completionSources.Add(new AnonymousCompletionSource(complete));
        }

        /// <summary>
        /// Adds a completion source using a delegate.
        /// </summary>
        /// <param name="completionSources">The list of completion sources to add to.</param>
        /// <param name="completions">A list of strings to be suggested for command line completions.</param>
        public static void Add(
            this CompletionSourceList completionSources,
            params string[] completions)
        {
            if (completionSources is null)
            {
                throw new ArgumentNullException(nameof(completionSources));
            }

            if (completions is null)
            {
                throw new ArgumentNullException(nameof(completions));
            }

            completionSources.Add(new AnonymousCompletionSource(_ => completions.Select(s => new CompletionItem(s))));
        }
    }
}
