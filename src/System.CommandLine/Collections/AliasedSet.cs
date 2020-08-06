// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.CommandLine.Collections
{
    public abstract class AliasedSet<T> : IReadOnlyList<T>
        where T : class
    {
        private protected Dictionary<string, T> _itemsByAlias = new Dictionary<string, T>();

        protected IList<T> Items { get; } = new List<T>();

        public T? this[string alias] => GetByAlias(alias);

        public T? GetByAlias(string alias)
        {
            if (_itemsByAlias.TryGetValue(alias, out var value) && 
                value is { })
            {
                return value;
            }

            return null;
        }

        public int Count => Items.Count;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        internal virtual void Add(T item)
        {
            Items.Add(item);

            foreach (var alias in GetRawAliases(item))
            {
                if (!_itemsByAlias.ContainsKey(alias))
                {
                    _itemsByAlias.Add(alias, item);
                }
            }

            foreach (var alias in GetAliases(item))
            {
                if (!_itemsByAlias.ContainsKey(alias))
                {
                    _itemsByAlias.Add(alias, item);
                }
            }
        }

        internal void Remove(T item)
        {
            Items.Remove(item);

            foreach (var alias in GetRawAliases(item))
            {
                if (_itemsByAlias.ContainsKey(alias))
                {
                    _itemsByAlias.Remove(alias);
                }
            }

            foreach (var alias in GetAliases(item))
            {
                if (_itemsByAlias.ContainsKey(alias))
                {
                    _itemsByAlias.Remove(alias);
                }
            }
        }

        protected abstract IReadOnlyCollection<string> GetAliases(T item);

        protected abstract IReadOnlyCollection<string> GetRawAliases(T item);

        public bool Contains(string alias) => _itemsByAlias.ContainsKey(alias);

        public T this[int index] => Items[index];
    }
}
