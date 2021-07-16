// // Copyright (c) .NET Foundation and contributors. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Suggestions;

namespace System.CommandLine
{
    public class SuggestionSourceList : IReadOnlyList<ISuggestionSource>
    {
        private readonly List<ISuggestionSource> _sources = new();

        public void Add(ISuggestionSource source)
        {
            _sources.Add(source);
        }

        public IEnumerator<ISuggestionSource> GetEnumerator() => _sources.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Clear()
        {
            _sources.Clear();
        }

        public int Count => _sources.Count;

        public ISuggestionSource this[int index] => _sources[index];
    }
}