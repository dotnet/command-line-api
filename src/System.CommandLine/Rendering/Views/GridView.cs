using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering.Views
{
    public class GridView : LayoutView<View>
    {
        private List<ColumnDefinition> Columns { get; } = new List<ColumnDefinition>();
        private List<RowDefinition> Rows { get; } = new List<RowDefinition>();

        private View[,] ChildLocations { get; set; }

        public GridView()
        {
            //1 column 1 row
            Columns.Add(ColumnDefinition.Star(1));
            Rows.Add(RowDefinition.Star(1));

            ChildLocations = new View[1, 1];
        }

        public void AddChild(View child, int column, int row)
        {
            //TODO: Ensure row/column is in a valid range
            base.AddChild(child);
            ChildLocations[column, row] = child;
        }

        public override void AddChild(View child) => throw new InvalidOperationException("Must call AddChild(View child, int column, int row) instead");

        public void SetColumns(params ColumnDefinition[] columns)
        {
            if (Children.Any())
            {
                throw new InvalidOperationException("Cannot change columns once children are added");
            }
            if (columns?.Any() != true)
            {
                throw new ArgumentException("Must specify at least one column", nameof(columns));
            }
            Columns.Clear();
            Columns.AddRange(columns);
            ChildLocations = new View[Columns.Count, Rows.Count];
        }

        public void SetRows(params RowDefinition[] rows)
        {
            if (Children.Any())
            {
                throw new InvalidOperationException("Cannot change rows once children are added");
            }
            if (rows?.Any() != true)
            {
                throw new ArgumentException("Must specify at least one row", nameof(rows));
            }
            Rows.Clear();
            Rows.AddRange(rows);
            ChildLocations = new View[Columns.Count, Rows.Count];
        }

        public override Size Measure(IRenderer renderer, Size maxSize)
        {
            int width = 0;
            int height = 0;

            Size[,] sizes = GetGridSizes(renderer, maxSize);

            for (int column = 0; column < Columns.Count; column++)
            {
                width += sizes[column, 0].Width;
            }
            for (int row = 0; row < Rows.Count; row++)
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

        public override void Render(IRenderer renderer, Region region)
        {
            Size[,] sizes = GetGridSizes(renderer, new Size(region.Width, region.Height));

            int top = region.Top;
            for (int row = 0; row < Rows.Count; row++)
            {
                int left = region.Left;
                int maxRowHeight = 0;
                for (int column = 0; column < Columns.Count; column++)
                {
                    if (ChildLocations[column, row] is View child)
                    {
                        child.Render(renderer, new Region(left, top, sizes[column, row]));
                    }
                    left += sizes[column, row].Width;
                    maxRowHeight = Math.Max(maxRowHeight, sizes[column, row].Height);
                }
                top += maxRowHeight;
            }
        }

        private Size[,] GetGridSizes(IRenderer renderer, Size maxSize)
        {
            double totalColumnStarSize = Columns.Where(x => x.SizeMode == SizeMode.Star).Sum(x => x.Value);
            double totalRowStarSize = Rows.Where(x => x.SizeMode == SizeMode.Star).Sum(x => x.Value);

            int?[] measuredColumns = new int?[Columns.Count];
            int?[] measuredRows = new int?[Rows.Count];

            int availableWidth = maxSize.Width;
            int? totalWidthForStarSizing = null;

            foreach (var (column, columnIndex) in Columns.OrderBy(x => GetProcessOrder(x.SizeMode)).Select((x, i) => (x, i)))
            {
                int availableHeight = maxSize.Height;

                for (int rowIndex = 0; rowIndex < Rows.Count; rowIndex++)
                {
                    if (measuredRows[rowIndex] == null)
                    {
                        switch (Rows[rowIndex].SizeMode)
                        {
                            case SizeMode.Fixed:
                                measuredRows[rowIndex] = Math.Min((int)Rows[rowIndex].Value, availableHeight);
                                break;
                            case SizeMode.Star:
                                measuredRows[rowIndex] = (int)Math.Round(Rows[rowIndex].Value / totalRowStarSize * maxSize.Height);
                                break;
                        }
                    }
                    Size childSize = null;
                    switch (column.SizeMode)
                    {
                        case SizeMode.Fixed:
                        {
                            if (measuredColumns[columnIndex] == null)
                            {
                                measuredColumns[columnIndex] = Math.Min((int)column.Value, availableWidth);
                            }
                            break;
                        }
                        case SizeMode.SizeToContent:
                        {
                            if (ChildLocations[columnIndex, rowIndex] is View child)
                            {
                                childSize = child.Measure(renderer, new Size(availableWidth, availableHeight));
                            }
                            measuredColumns[columnIndex] = Math.Min(Math.Max(measuredColumns[columnIndex] ?? 0, childSize?.Width ?? 0), availableWidth);
                        }
                        break;
                        case SizeMode.Star:
                        {
                            if (totalWidthForStarSizing == null)
                            {
                                totalWidthForStarSizing = availableWidth;
                            }
                            int starWidth = (int)Math.Round(column.Value / totalColumnStarSize * totalWidthForStarSizing.Value);
                            if (measuredColumns[columnIndex] < starWidth)
                            {
                                starWidth = measuredColumns[columnIndex].Value;
                            }
                            measuredColumns[columnIndex] = starWidth;
                            break;
                        }
                        default:
                            throw new InvalidOperationException($"Unknown size mode {column.SizeMode}");
                    }

                    if (Rows[rowIndex].SizeMode == SizeMode.SizeToContent)
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

            var rv = new Size[Columns.Count, Rows.Count];
            for (int rowIndex = 0; rowIndex < Rows.Count; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < Columns.Count; columnIndex++)
                {
                    rv[columnIndex, rowIndex] = new Size(measuredColumns[columnIndex].Value, measuredRows[rowIndex].Value);
                }
            }
            return rv;
        }

        private static int GetProcessOrder(SizeMode sizeMode)
        {
            switch (sizeMode)
            {
                case SizeMode.Fixed:
                    return 0;
                case SizeMode.SizeToContent:
                    return 1;
                case SizeMode.Star:
                    return 2;
                default:
                    throw new InvalidOperationException($"Unknown size mode {sizeMode}");
            }
        }
    }
}
