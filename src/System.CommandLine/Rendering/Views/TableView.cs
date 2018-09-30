using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering.Views
{
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
            EnsureInitialized(renderer);
            Layout.Render(renderer, region);
        }

        public override Size Measure(IRenderer renderer, Size maxSize)
        {
            EnsureInitialized(renderer);
            return Layout.Measure(renderer, maxSize);
        }

        private void EnsureInitialized(IRenderer renderer)
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
                    if (Columns[columnIndex].GetCell(item, renderer.Formatter) is View cell)
                    {
                        Layout.SetChild(cell, columnIndex, itemIndex + 1);
                    }
                }
            }

            _gridInitialized = true;
        }
    }
}
