// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.CommandLine
{
    [DebuggerStepThrough]
    public abstract class AliasedSet<T> : IReadOnlyCollection<T>
        where T : class
    {
        protected HashSet<T> Items { get; } = new HashSet<T>();

        public T this[string alias] => GetByAlias(alias);

        public T GetByAlias(string alias) =>
            Items.SingleOrDefault(o => ContainsItemWithRawAlias(o, alias)) ??
            Items.SingleOrDefault(o => ContainsItemWithAlias(o, alias));

        public int Count => Items.Count;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        protected abstract bool ContainsItemWithAlias(T item, string alias);

        protected abstract bool ContainsItemWithRawAlias(T item, string alias);

        internal void Add(T item)
        {
            var preexistingAlias = GetAliases(item)
                .FirstOrDefault(alias =>
                                    Items.Any(o =>
                                                  ContainsItemWithRawAlias(o, alias)));

            if (preexistingAlias != null)
            {
                throw new ArgumentException($"Alias '{preexistingAlias}' is already in use.");
            }

            Items.Add(item);
        }

        internal void Remove(T item)
        {
            Items.Remove(item);
        }

        protected abstract IReadOnlyCollection<string> GetAliases(T item);

        public bool Contains(string alias) =>
            Items.Any(option => ContainsItemWithAlias(option, alias));
    }
}
