// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
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
        private readonly HashSet<T> symbols = new HashSet<T>();

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
            symbols.SingleOrDefault(o => ContainsSymbolWithRawAlias(o, alias)) ??
            symbols.SingleOrDefault(o => ContainsSymbolWithAlias(o, alias));

        public int Count => symbols.Count;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return symbols.GetEnumerator();
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
                                    symbols.Any(o =>
                                                    ContainsSymbolWithRawAlias(o, alias)));

            if (preexistingAlias != null)
            {
                throw new ArgumentException($"Alias '{preexistingAlias}' is already in use.");
            }

            symbols.Add(option);
        }

        protected abstract IReadOnlyCollection<string> RawAliasesFor(T symbol);

        public bool Contains(string alias) =>
            symbols.Any(option => ContainsSymbolWithAlias(option, alias));
    }
}
