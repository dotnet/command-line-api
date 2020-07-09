// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.CommandLine.Collections
{
    public abstract class AliasedSet<T> : IReadOnlyList<T>
        where T : class
    {
        protected IList<T> Items { get; } = new List<T>();

        public T? this[string alias] => GetByAlias(alias);

        public T? GetByAlias(string alias)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var caseInsensitive = IsCaseInsensitive(item);

                if (Contains(GetAliases(item), alias, caseInsensitive) || 
                    Contains(GetRawAliases(item), alias, caseInsensitive))
                {
                    return item;
                }
            }

            return null;
        }

        private protected bool Contains(
            IReadOnlyList<string> aliases,
            string alias,
            bool caseInsensitive = false)
        {
            for (var i = 0; i < aliases.Count; i++)
            {
                if (string.Equals(aliases[i], alias,
                    caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public int Count => Items.Count;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        internal virtual void Add(T item)
        {
            Items.Add(item);
        }

        internal void Remove(T item)
        {
            Items.Remove(item);
        }

        protected abstract IReadOnlyList<string> GetAliases(T item);

        protected abstract IReadOnlyList<string> GetRawAliases(T item);

        protected virtual bool IsCaseInsensitive(T item) => false;

        public bool Contains(string alias) => GetByAlias(alias) != null;

        public T this[int index] => Items[index];
    }
}
