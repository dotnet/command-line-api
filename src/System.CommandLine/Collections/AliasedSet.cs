// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Collections
{
    /// <summary>
    /// An ordered set containing instances that are unique based on one or more string aliases per instance.
    /// </summary>
    /// <typeparam name="T">The type of the instances contained by the set.</typeparam>
    public abstract class AliasedSet<T> : IReadOnlyList<T>
        where T : class
    {
        private protected readonly Dictionary<string, T> ItemsByAlias = new();

        private protected List<T> Items { get; } = new();

        private protected HashSet<T> DirtyItems { get; } = new();
        
        /// <inheritdoc/>
        public int Count => Items.Count;

        /// <summary>
        /// Determines whether the <see cref="AliasedSet{T}"/> contains a value with the specified alias.
        /// </summary>
        /// <param name="alias">The alias to locate.</param>
        /// <returns><see langword="true" /> if the set contains a value with the specified alias; otherwise, <see langword="false"/>.</returns>
        public bool ContainsAlias(string alias)
        {
            EnsureAliasIndexIsCurrent();

            return ItemsByAlias.ContainsKey(alias);
        }

        /// <summary>
        /// Gets the member of the set having the specified alias, if any.
        /// </summary>
        /// <param name="alias">Any alias for the sought item.</param>
        /// <returns>The member of the set having the specified alias, if any; otherwise, null.</returns>
        public T? GetByAlias(string alias)
        {
            EnsureAliasIndexIsCurrent();

            ItemsByAlias.TryGetValue(alias, out var value);

            return value;
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        internal virtual void Add(T item)
        {
            Items.Add(item);

            foreach (var alias in GetAliases(item))
            {
                ItemsByAlias.TryAdd(alias, item);
            }
        }

        /// <summary>
        /// Gets the list of aliases of the specified item.
        /// </summary>
        /// <param name="item">The item for which a list of aliases is to be provided.</param>
        /// <returns>The list of aliases for the specified item.</returns>
        protected abstract IReadOnlyCollection<string> GetAliases(T item);

        /// <inheritdoc/>
        public T this[int index] => Items[index];

        private protected void EnsureAliasIndexIsCurrent()
        {
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