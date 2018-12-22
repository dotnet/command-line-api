// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Rendering.Views
{
    public abstract class ItemsView<TItem> : View
    {
        private IReadOnlyList<TItem> _items;
        //TODO: IEnumerable? INCC? IObservable?
        public virtual IReadOnlyList<TItem> Items
        {
            get => _items;
            set
            {
                if (!EqualityComparer<IReadOnlyList<TItem>>.Default.Equals(_items, value))
                {
                    _items = value;
                    OnUpdated();
                }
            }
        }
    }
}
