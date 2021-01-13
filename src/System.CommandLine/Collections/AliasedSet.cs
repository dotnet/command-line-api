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
        private protected readonly Dictionary<string, T> ItemsByAlias = new Dictionary<string, T>();

        public int Count => Items.Count;

        private protected List<T> Items { get; } = new List<T>();

        private protected HashSet<T> DirtyItems { get; } = new HashSet<T>();

        public bool Contains(string alias)
        {
            EnsureAliasIndexIsCurrent();

            return ItemsByAlias.ContainsKey(alias);
        }

        public T? GetByAlias(string alias)
        {
            EnsureAliasIndexIsCurrent();

            ItemsByAlias.TryGetValue(alias, out var value);

            return value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        internal virtual void Add(T item)
        {
            Items.Add(item);

            foreach (var alias in GetAliases(item))
            {
                ItemsByAlias.TryAdd(alias, item);
            }
        }

        internal virtual void Remove(T item)
        {
            Items.Remove(item);

            foreach (var alias in GetAliases(item))
            {
                ItemsByAlias.Remove(alias);
            }
        }

        protected abstract IReadOnlyCollection<string> GetAliases(T item);

        public T this[int index] => Items[index];

        private protected void EnsureAliasIndexIsCurrent()
        {
            // FIX: (EnsureAliasIndexIsCurrent) 
            if (DirtyItems.Count == 0)
            {
                return;
            }

            foreach (var dirtyItem in DirtyItems)
            {
                var aliases = GetAliases(dirtyItem).ToArray();

                foreach (var pair in ItemsByAlias.ToArray())
                {
                    if (pair.Value.Equals(dirtyItem))
                    {
                        ItemsByAlias.Remove(pair.Key);
                    }
                }

                if (Items.Contains(dirtyItem))
                {
                    for (var j = 0; j < aliases.Length; j++)
                    {
                        var alias = aliases[j];
                        ItemsByAlias.TryAdd(alias, dirtyItem);
                    }
                }
            }

            DirtyItems.Clear();
        }
    }
}