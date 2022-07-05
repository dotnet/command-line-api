// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

        private readonly List<ITableViewColumn<TItem>> _columns = new();
        public IReadOnlyList<ITableViewColumn<TItem>> Columns => _columns;

        public TableView()
        {
            Layout.Updated += OnLayoutUpdated;
        }

        private void OnLayoutUpdated(object sender, EventArgs e) => OnUpdated();

        public void AddColumn<T>(Func<TItem, T> cellValue, string header, ColumnDefinition column = null) 
            => AddColumn(cellValue, new ContentView(header), column);

        public void AddColumn<T>(Func<TItem, T> cellValue, View header, ColumnDefinition column = null) 
            => AddColumn(new TableViewColumn<T>(cellValue, header, column ?? ColumnDefinition.SizeToContent()));

        public void AddColumn(ITableViewColumn<TItem> column)
        {
            _columns.Add(column);
            _gridInitialized = false;

            OnUpdated();
        }

        public override void Render(ConsoleRenderer renderer, Region region)
        {
            EnsureInitialized(renderer);
            Layout.Render(renderer, region);
        }

        public override Size Measure(ConsoleRenderer renderer, Size maxSize)
        {
            EnsureInitialized(renderer);
            return Layout.Measure(renderer, maxSize);
        }

        private void EnsureInitialized(ConsoleRenderer renderer)
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

        private class TableViewColumn<T> : ITableViewColumn<TItem>
        {
            private readonly Func<TItem, T> _cellValue;

            public View Header { get; }

            public ColumnDefinition ColumnDefinition { get; }

            public TableViewColumn(Func<TItem, T> cellValue, View header, ColumnDefinition columnDefinition)
            {
                Header = header;
                ColumnDefinition = columnDefinition ?? throw new ArgumentNullException(nameof(columnDefinition));
                _cellValue = cellValue ?? throw new ArgumentNullException(nameof(cellValue));
            }

            public View GetCell(TItem item, TextSpanFormatter formatter)
            {
                T value = _cellValue(item);

                if (ReferenceEquals(value, null))
                {
                    return null;
                }

                if (value is View view)
                {
                    return view;
                }

                return ContentView.Create(value, formatter);
            }
        }
    }
}
