using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering.Views
{
    public class GridView : LayoutView<View>
    {
        private List<ColumnDefinition> Columns { get; } = new List<ColumnDefinition>();
        private List<RowDefinition> Rows { get; } = new List<RowDefinition>();

        //private Dictionary<View, (int column, int row)> ChildLocations { get; } = new Dictionary<View, (int column, int row)>();

        private View[,] ChildLocations { get; set; }


        public GridView()
        {
            //1 column 1 row
            Columns.Add(new ColumnDefinition(1));
            Rows.Add(new RowDefinition(1));

            ChildLocations = new View[1, 1];
        }

        public void AddChild(View child, int column, int row)
        {
            //TODO: Ensure row/column is in a valid range
            AddChild(child);
            ChildLocations[column, row] = child;
        }

        public void SetColumns(params ColumnDefinition[] columns)
        {
            if (Children.Any())
            {
                throw new InvalidOperationException("Cannot change columns once children are added");
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
            Rows.Clear();
            Rows.AddRange(rows);
            ChildLocations = new View[Columns.Count, Rows.Count];
        }

        public override Size GetContentSize()
        {
            int width = 0;
            int height = 0;

            for (int row = 0; row < Rows.Count; row++)
            {
                int rowWidth = 0;
                int rowHeight = 0;
                for (int column = 0; column < Columns.Count; column++)
                {
                    if (ChildLocations[column, row] is View child)
                    {
                        Size childSize = child.GetContentSize();
                        rowWidth += childSize.Width;
                        rowHeight = Math.Max(rowHeight, childSize.Height);
                    }
                }

                height += rowHeight;
                width = Math.Max(width, rowWidth);
            }

            return new Size(width, height);
        }

        public override Size GetAdjustedSize(IRenderer renderer, Size maxSize)
        {
            int width = 0;
            int height = 0;

            double totalColumnStarSize = Columns.Sum(x => x.StarSize);
            double totalRowStarSize = Rows.Sum(x => x.StarSize);

            for (int row = 0; row < Rows.Count; row++)
            {
                //TODO: Deal with other size modes
                int maxRowHeight = (int)Math.Round(Rows[row].StarSize / totalRowStarSize * maxSize.Height);

                int rowWidth = 0;
                int rowHeight = maxRowHeight;

                for (int column = 0; column < Columns.Count; column++)
                {
                    if (ChildLocations[column, row] is View child)
                    {
                        //TODO: Deal with other size modes
                        int maxColumnWidth = (int)Math.Round(Columns[column].StarSize / totalColumnStarSize * maxSize.Width);
                        Size childSize = child.GetAdjustedSize(renderer, new Size(maxColumnWidth, rowHeight));

                        rowWidth += childSize.Width;
                        rowHeight = Math.Max(rowHeight, childSize.Height);
                    }
                }

                height += rowHeight;
                width = Math.Max(width, rowWidth);
            }

            return new Size(width, height);
        }

        public override void Render(Region region, IRenderer renderer)
        {
            double totalColumnStarSize = Columns.Sum(x => x.StarSize);
            double totalRowStarSize = Rows.Sum(x => x.StarSize);

            int top = region.Top;

            for (int row = 0; row < Rows.Count; row++)
            {
                if (top > region.Bottom) return;

                int left = region.Left;

                int rowHeight = (int)Math.Round(Rows[row].StarSize / totalRowStarSize * region.Height);

                for (int column = 0; column < Columns.Count; column++)
                {
                    if (left > region.Right) return;

                    //TODO: Deal with other size modes
                    int columnWidth = (int)Math.Round(Columns[column].StarSize / totalColumnStarSize * region.Width);

                    if (ChildLocations[column, row] is View child)
                    {
                        var childRegion = new Region(left, top, columnWidth, rowHeight);
                        child.Render(childRegion, renderer);
                    }
                    left += columnWidth;
                }
                top += rowHeight;
            }
        }


        public class RowDefinition
        {
            public RowDefinition(double starSize)
            {
                StarSize = starSize;
            }

            public double StarSize { get; }
        }

        public class ColumnDefinition
        {
            public ColumnDefinition(double starSize)
            {
                StarSize = starSize;
            }

            public double StarSize { get; }
        }
    }
}
