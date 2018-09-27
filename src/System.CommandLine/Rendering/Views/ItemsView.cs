using System.Collections.Generic;
using System.Linq;

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

    public class TableView<TItem> : ItemsView<TItem>
    {
        public override IReadOnlyList<TItem> Items
        {
            get => base.Items;
            set
            {
                base.Items = value;
                //TODO: Don't un-initialize if the values are equal
                _gridInitialized = false;
            }
        }

        private bool _gridInitialized;

        private GridView Layout { get; } = new GridView();

        //TODO: Expose as read-only
        public IList<TableViewColumn<TItem>> Columns { get; } = new List<TableViewColumn<TItem>>();

        public TableView()
        {
            Layout.Updated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object sender, EventArgs e) => OnUpdated();

        public void AddColumn(TableViewColumn<TItem> column)
        {
            Columns.Add(column);
            _gridInitialized = false;

            OnUpdated();
        }

        public override void Render(IRenderer renderer, Region region)
        {
            EnsureInitialized();
            Layout.Render(renderer, region);
        }

        public override Size Measure(IRenderer renderer, Size maxSize)
        {
            EnsureInitialized();
            return Layout.Measure(renderer, maxSize);
        }

        private void EnsureInitialized()
        {
            if (_gridInitialized) return;

            Layout.SetColumns(Columns.Select(x => x.ColumnDefinition).ToArray());
            Layout.SetRows(Enumerable.Repeat(RowDefinition.SizeToContent(), Items.Count + 1).ToArray());

            for (int columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
            {
                if (Columns[columnIndex].Header is View header)
                {
                    Layout.SetChild(header, columnIndex, 0);
                }
            }

            for (int itemIndex = 0; itemIndex < Items.Count; itemIndex++)
            {
                TItem item = Items[itemIndex];
                for (int columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                {
                    if (Columns[columnIndex].GetCell(item) is View cell)
                    {
                        Layout.SetChild(cell, columnIndex, itemIndex + 1);
                    }
                }
            }

            _gridInitialized = true;
        }
    }

    public class TableViewColumn<TItem>
    {
        private readonly Func<TItem, object> _cellValue;

        public View Header { get; }

        public ColumnDefinition ColumnDefinition { get; }

        public TableViewColumn(Func<TItem, object> cellValue, string header)
            : this(cellValue, header, ColumnDefinition.SizeToContent())
        { }

        public TableViewColumn(Func<TItem, object> cellValue, string header, ColumnDefinition columnDefinition)
            : this(cellValue, new ContentView(header), columnDefinition)
        { }

        public TableViewColumn(Func<TItem, object> cellValue, View header, ColumnDefinition columnDefinition)
        {
            Header = header;
            ColumnDefinition = columnDefinition ?? throw new ArgumentNullException(nameof(columnDefinition));
            _cellValue = cellValue ?? throw new ArgumentNullException(nameof(cellValue));
        }

        public virtual View GetCell(TItem item)
        {
            object value = _cellValue(item);

            switch (value)
            {
                case null: return null;
                case View view: return view;
                default: return ContentView.Create(value);
            }
        }
    }
}
