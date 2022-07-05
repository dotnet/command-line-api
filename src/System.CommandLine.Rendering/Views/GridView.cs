// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering.Views
{
    public class GridView : LayoutView<View>
    {
        private readonly List<ColumnDefinition> _columns = new();
        private readonly List<RowDefinition> _rows = new();
        private readonly int _columnPaddingRight = 2;

        private View[,] ChildLocations { get; set; }

        public GridView()
        {
            //1 column 1 row
            _columns.Add(ColumnDefinition.Star(1));
            _rows.Add(RowDefinition.Star(1));

            ChildLocations = new View[1, 1];
        }

        //TODO: Consider indexer access to get/set children
        //TODO: Should this be row, column or column, row?
        public void SetChild(View child, int column, int row)
        {
            //TODO: Ensure row/column is in a valid range
            base.Add(child);
            ChildLocations[column, row] = child;
        }

        public override void Add(View child) => throw new InvalidOperationException("Must call SetChild(View child, int column, int row) instead");

        public void SetColumns(params ColumnDefinition[] columns)
        {
            if (Children.Count > 0)
            {
                throw new InvalidOperationException("Cannot change columns once children are added");
            }
            if ((columns is null) || (columns.Length == 0))
            {
                throw new ArgumentException("Must specify at least one column", nameof(columns));
            }
            _columns.Clear();
            _columns.AddRange(columns);
            ChildLocations = new View[_columns.Count, _rows.Count];
        }

        public void SetRows(params RowDefinition[] rows)
        {
            if (Children.Count > 0)
            {
                throw new InvalidOperationException("Cannot change rows once children are added");
            }
            if ((rows is null) || (rows.Length == 0))
            {
                throw new ArgumentException("Must specify at least one row", nameof(rows));
            }
            _rows.Clear();
            _rows.AddRange(rows);
            ChildLocations = new View[_columns.Count, _rows.Count];
        }

        public override Size Measure(ConsoleRenderer renderer, Size maxSize)
        {
            int width = 0;
            int height = 0;

            Size[,] sizes = GetGridSizes(renderer, maxSize);

            for (int column = 0; column < _columns.Count; column++)
            {
                width += sizes[column, 0].Width;
            }
            for (int row = 0; row < _rows.Count; row++)
            {
                height += sizes[0, row].Height;
            }
            if (width > maxSize.Width)
            {
                width = maxSize.Width;
            }
            if (height > maxSize.Height)
            {
                height = maxSize.Height;
            }

            return new Size(width, height);
        }

        public override void Render(ConsoleRenderer renderer, Region region)
        {
            Size[,] sizes = GetGridSizes(renderer, new Size(region.Width, region.Height));

            int top = region.Top;
            for (int row = 0; row < _rows.Count; row++)
            {
                int left = region.Left;
                int maxRowHeight = 0;
                for (int column = 0; column < _columns.Count; column++)
                {
                    var paddingWidth = GetColumnPaddingWidth(column);
                    if (ChildLocations[column, row] is not { } child ||
                        sizes[column, row].Width - paddingWidth <= 0 ||
                        sizes[column, row].Height <= 0)
                    {
                        //child no view or no space left to render
                        continue;
                    }

                    var contentSize = new Size(sizes[column, row].Width - paddingWidth, sizes[column, row].Height);
                    child.Render(renderer, new Region(left, top, contentSize));

                	if (paddingWidth > 0)
                    { 
                    	// Explicit render the space between the columns to improve rendering in plain text 
                        var paddingSize = new Size(paddingWidth, sizes[column, row].Height);
                        renderer.RenderToRegion(TextSpan.Empty(), new Region(left + contentSize.Width, top, paddingSize));
                    }

                    left += sizes[column, row].Width;
                    maxRowHeight = Math.Max(maxRowHeight, sizes[column, row].Height);
                }
                top += maxRowHeight;
            }
        }

        private Size[,] GetGridSizes(ConsoleRenderer renderer, Size maxSize)
        {
            int countStarSizedColumnsWithPadding = _columns.AsEnumerable().Reverse().Skip(1).Count(x => x.SizeMode == SizeMode.Star);
            double totalColumnStarSize = _columns.Where(x => x.SizeMode == SizeMode.Star).Sum(x => x.Value);
            double totalRowStarSize = _rows.Where(x => x.SizeMode == SizeMode.Star).Sum(x => x.Value);

            int?[] measuredColumns = new int?[_columns.Count];
            int?[] measuredRows = new int?[_rows.Count];

            int availableWidth = maxSize.Width;
            int? totalWidthForStarSizing = null;

            foreach (var (column, columnIndex) in _columns.Select((definition, columnIndex) => (definition, columnIndex)).OrderBy(t => GetProcessOrder(t.definition.SizeMode)))
            {
                int availableHeight = maxSize.Height;
                int? totalHeightForStarSizing = null;

                foreach (var (row, rowIndex) in _rows.Select((definition, rowIndex) => (definition, rowIndex)).OrderBy(t => GetProcessOrder(t.definition.SizeMode)))
                {
                    if (measuredRows[rowIndex] == null)
                    {
                        switch (row.SizeMode)
                        {
                            case SizeMode.Fixed:
                            {
                                measuredRows[rowIndex] = Math.Min((int)row.Value, availableHeight);
                                break;
                            }
                            case SizeMode.Star:
                            {
                                totalHeightForStarSizing ??= availableHeight;
                                int starHeight = (int)Math.Round(row.Value / totalRowStarSize * totalHeightForStarSizing.Value);
                                if (measuredRows[rowIndex] < starHeight)
                                {
                                    starHeight = measuredRows[rowIndex].Value;
                                }
                                measuredRows[rowIndex] = starHeight;
                                break;
                            }
                            case SizeMode.SizeToContent:
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown row size mode {row.SizeMode}");
                        }
                    }
                    var paddingWidth = GetColumnPaddingWidth(columnIndex);
                    Size childSize = null;
                    switch (column.SizeMode)
                    {
                        case SizeMode.Fixed:
                        {
                            if (measuredColumns[columnIndex] == null)
                            {
                                measuredColumns[columnIndex] = Math.Min((int)column.Value + paddingWidth, availableWidth);
                            }
                            break;
                        }
                        case SizeMode.SizeToContent:
                        {
                            if (ChildLocations[columnIndex, rowIndex] is View child)
                            {
                                childSize = child.Measure(renderer, new Size(availableWidth, availableHeight));
                            }
                            var width = childSize?.Width + paddingWidth ?? 0;
                            measuredColumns[columnIndex] = Math.Min(Math.Max(measuredColumns[columnIndex] ?? 0, width), availableWidth);
                        }
                            break;
                        case SizeMode.Star:
                        {
                            totalWidthForStarSizing ??= availableWidth - countStarSizedColumnsWithPadding * _columnPaddingRight;
                            int starWidth = (int)Math.Round(column.Value / totalColumnStarSize * totalWidthForStarSizing.Value);
                            if (measuredColumns[columnIndex] < starWidth)
                            {
                                starWidth = measuredColumns[columnIndex].Value;
                            }
                            measuredColumns[columnIndex] = starWidth + paddingWidth;
                            break;
                        }
                        default:
                            throw new InvalidOperationException($"Unknown column size mode {column.SizeMode}");
                    }

                    if (row.SizeMode == SizeMode.SizeToContent)
                    {
                        if (childSize == null && ChildLocations[columnIndex, rowIndex] is View child)
                        {
                            childSize = child.Measure(renderer, new Size(availableWidth, availableHeight));
                        }
                        measuredRows[rowIndex] = Math.Min(Math.Max(childSize?.Height ?? 0, childSize?.Height ?? 0), availableHeight);
                    }

                    availableHeight -= measuredRows[rowIndex].Value;
                }
                availableWidth -= measuredColumns[columnIndex].Value;

            }

            var rv = new Size[_columns.Count, _rows.Count];
            for (int rowIndex = 0; rowIndex < _rows.Count; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < _columns.Count; columnIndex++)
                {
                    rv[columnIndex, rowIndex] = new Size(measuredColumns[columnIndex].Value, measuredRows[rowIndex].Value);
                }
            }
            return rv;
        }


        private int GetColumnPaddingWidth(int columnIndex) => columnIndex < _columns.Count - 1 ? _columnPaddingRight : 0;


        private static int GetProcessOrder(SizeMode sizeMode)
        {
            return sizeMode switch
            {
                SizeMode.Fixed => 0,
                SizeMode.SizeToContent => 1,
                SizeMode.Star => 2,
                _ => throw new InvalidOperationException($"Unknown size mode {sizeMode}")
            };
        }
    }
}
