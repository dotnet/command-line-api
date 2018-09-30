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
