using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering
{
    internal class ConsoleTableColumn<T>
    {
        private Dictionary<int, Span> _spans;

        public ConsoleTableColumn(
            Span header,
            Func<T, Span> renderCell)
        {
            RenderCell = renderCell ?? throw new ArgumentNullException(nameof(renderCell));
            Header = header;
        }

        public Func<T, Span> RenderCell { get; }

        public Span Header { get; }

        public void FlushRow(
            int rowIndex,
            int verticalOffset,
            ConsoleRenderer consoleRenderer)
        {
            if (_spans == null)
            {
                return;
            }

            var span = _spans[rowIndex];

            var region = new Region(
                left: Left,
                top: rowIndex + verticalOffset,
                width: Width, 
                height: 1);

            consoleRenderer.RenderToRegion(span, region);
        }

        public int Width { get; private set; }

        public void CalculateSpans(IReadOnlyList<T> items)
        {
            _spans = new Dictionary<int, Span>();

            _spans[0] = Header;

            for (var i = 0; i < items.Count; i++)
            {
                _spans[i + 1] = RenderCell(items[i]);
            }

            Width = _spans.Values.Max(s => s.ContentLength) + Gutter;
        }

        public int Gutter { get; set; } = 2;

        public int Left { get; internal set; }
    }
}
