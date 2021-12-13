// // Copyright (c) .NET Foundation and contributors. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Completions;

namespace System.CommandLine
{
    /// <summary>
    /// A list of completion sources to be used when providing completions for completion.
    /// </summary>
    public class CompletionSourceList : IReadOnlyList<ICompletionSource>
    {
        private readonly List<ICompletionSource> _sources = new();

        /// <summary>
        /// Adds a completion source to the list.
        /// </summary>
        /// <param name="source">The source to add.</param>
        public void Add(ICompletionSource source)
        {
            _sources.Add(source);
        }

        /// <inheritdoc />
        public IEnumerator<ICompletionSource> GetEnumerator() => _sources.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Clears the completion sources.
        /// </summary>
        public void Clear()
        {
            _sources.Clear();
        }

        /// <inheritdoc />
        public int Count => _sources.Count;

        /// <inheritdoc />
        public ICompletionSource this[int index] => _sources[index];
    }
}