// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace System.CommandLine.Rendering.Views
{
    //TODO: consider IEnumerable<T> addition
    public abstract class LayoutView<T> : View, IEnumerable<T>
        where T : View
    {
        private readonly List<T> _children = new();
        public IReadOnlyList<T> Children => _children.AsReadOnly();

        public virtual void Add(T child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            _children.Add(child);
            
            child.Updated -= OnChildUpdated;
            child.Updated += OnChildUpdated;
        }

        public virtual void Clear()
        {
            while (_children.Count != 0)
            {
                Remove(Children[0]);
            }
        }

        public virtual bool Remove(T child)
        {
            child.Updated -= OnChildUpdated;
            return _children.Remove(child);
        }

        protected virtual void OnChildUpdated(object sender, EventArgs e)
        {
            OnUpdated();
        }

        public IEnumerator<T> GetEnumerator() => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_children).GetEnumerator();
    }
}
