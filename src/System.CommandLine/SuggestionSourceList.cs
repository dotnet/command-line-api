// // Copyright (c) .NET Foundation and contributors. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Suggestions;

namespace System.CommandLine
{
    /// <summary>
    /// A list of suggestion sources to be used when providing suggestions for completion.
    /// </summary>
    public class SuggestionSourceList : IReadOnlyList<ISuggestionSource>
    {
        private readonly List<ISuggestionSource> _sources = new();

        /// <summary>
        /// Adds a suggestion source to the list.
        /// </summary>
        /// <param name="source">The source to add.</param>
        public void Add(ISuggestionSource source)
        {
            _sources.Add(source);
        }

        /// <inheritdoc />
        public IEnumerator<ISuggestionSource> GetEnumerator() => _sources.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Clears the suggestion sources.
        /// </summary>
        public void Clear()
        {
            _sources.Clear();
        }

        /// <inheritdoc />
        public int Count => _sources.Count;

        /// <inheritdoc />
        public ISuggestionSource this[int index] => _sources[index];
    }
}