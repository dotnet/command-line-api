// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Collections
{
    public abstract class AliasedSet<T> : IReadOnlyList<T>
        where T : class
    {
        private protected readonly Dictionary<string, T> _itemsByAlias = new Dictionary<string, T>();

        private protected List<T> Items { get; } = new List<T>();

        private protected HashSet<T> DirtyItems { get; } = new HashSet<T>();

        public T? this[string alias] => GetByAlias(alias);

        public T? GetByAlias(string alias)
        {
            EnsureAliasIndexIsCurrent();

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

            foreach (var alias in GetAllAliases(item))
            {
                _itemsByAlias.TryAdd(alias, item);
            }
        }

        internal virtual void Remove(T item)
        {
            Items.Remove(item);

            foreach (var alias in GetAllAliases(item))
            {
                _itemsByAlias.TryRemove(alias);
            }
        }

        protected abstract IReadOnlyCollection<string> GetAliases(T item);

        protected abstract IReadOnlyCollection<string> GetRawAliases(T item);

        public bool Contains(string alias)
        {
            EnsureAliasIndexIsCurrent();

            return _itemsByAlias.ContainsKey(alias);
        }

        private IReadOnlyCollection<string> GetAllAliases(T item) =>
            GetAliases(item).Concat(GetRawAliases(item)).ToArray();

        public T this[int index] => Items[index];

        private protected void EnsureAliasIndexIsCurrent()
        {
            foreach (var dirtyItem in DirtyItems.ToArray())
            {
                var aliases = GetAllAliases(dirtyItem).ToList();

                foreach (var pair in _itemsByAlias.Where(p => p.Value.Equals(dirtyItem)).ToArray())
                {
                    _itemsByAlias.Remove(pair.Key);
                }

                var wasRemoved = !Items.Contains(dirtyItem);

                if (!wasRemoved)
                {
                    for (var i = 0; i < aliases.Count; i++)
                    {
                        var alias = aliases[i];
                        _itemsByAlias.TryAdd(alias, dirtyItem);
                    }
                }

                DirtyItems.Remove(dirtyItem);
            }
        }
    }
}