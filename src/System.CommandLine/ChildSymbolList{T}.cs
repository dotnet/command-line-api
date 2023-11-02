// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.CommandLine
{
    /// <summary>
    /// a wrapper of List<typeparamref name="T"/> that sets parent for every added element
    /// </summary>
    internal sealed class ChildSymbolList<T> : IList<T> where T : CliSymbol
    {
        private readonly List<T> _children;
        private readonly CliCommand _parent;

        public ChildSymbolList(CliCommand parent)
        {
            _parent = parent;
            _children = new();
        }
        
        public T this[int index]
        {
            get => _children[index];
            set
            {
                _children[index] = value;
                value.AddParent(_parent);
            }
        }

        public int Count => _children.Count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            item.AddParent(_parent);
            _children.Add(item);
        }

        public void Clear() => _children.Clear();

        public bool Contains(T item) => _children.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _children.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _children.GetEnumerator();

        public int IndexOf(T item) => _children.IndexOf(item);

        public void Insert(int index, T item)
        {
            item.AddParent(_parent);
            _children.Insert(index, item);
        }

        public bool Remove(T item) => _children.Remove(item);

        public void RemoveAt(int index) => _children.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => _children.GetEnumerator();
    }
}
