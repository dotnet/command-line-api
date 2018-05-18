// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.CommandLine
{
    [DebuggerStepThrough]
    public abstract class SymbolSet<T> : IReadOnlyCollection<T>
        where T : class
    {
        private readonly HashSet<T> _symbols = new HashSet<T>();

        protected SymbolSet()
        {
        }

        protected SymbolSet(IReadOnlyCollection<T> symbols)
        {
            if (symbols == null)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            foreach (var option in symbols)
            {
                Add(option);
            }
        }

        public T this[string alias] =>
            _symbols.SingleOrDefault(o => ContainsSymbolWithRawAlias(o, alias)) ??
            _symbols.SingleOrDefault(o => ContainsSymbolWithAlias(o, alias));

        public int Count => _symbols.Count;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _symbols.GetEnumerator();
        }

        internal void AddRange(IEnumerable<T> options)
        {
            foreach (var option in options)
            {
                Add(option);
            }
        }

        protected abstract bool ContainsSymbolWithAlias(T symbol, string alias);

        protected abstract bool ContainsSymbolWithRawAlias(T symbol, string alias);

        internal void Add(T option)
        {
            var preexistingAlias = RawAliasesFor(option)
                .FirstOrDefault(alias =>
                                    _symbols.Any(o =>
                                                    ContainsSymbolWithRawAlias(o, alias)));

            if (preexistingAlias != null)
            {
                throw new ArgumentException($"Alias '{preexistingAlias}' is already in use.");
            }

            _symbols.Add(option);
        }

        protected abstract IReadOnlyCollection<string> RawAliasesFor(T symbol);

        public bool Contains(string alias) =>
            _symbols.Any(option => ContainsSymbolWithAlias(option, alias));
    }
}
