namespace System.CommandLine.Rendering.Views
{
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

        public TableViewColumn(Func<TItem, object> cellValue, View header)
            : this(cellValue, header, ColumnDefinition.SizeToContent())
        { }

        public TableViewColumn(Func<TItem, object> cellValue, View header, ColumnDefinition columnDefinition)
        {
            Header = header;
            ColumnDefinition = columnDefinition ?? throw new ArgumentNullException(nameof(columnDefinition));
            _cellValue = cellValue ?? throw new ArgumentNullException(nameof(cellValue));
        }

        public virtual View GetCell(TItem item, SpanFormatter formatter)
        {
            object value = _cellValue(item);

            switch (value)
            {
                case null: return null;
                case View view: return view;
                default: return ContentView.Create(value, formatter);
            }
        }
    }
}
