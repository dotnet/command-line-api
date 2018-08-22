using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public abstract class ConsoleView<T> : IConsoleView<T>
    {
        private Region _effectiveRegion;
        private int _verticalOffset;

        protected ConsoleView(
            ConsoleRenderer renderer,
            Region region = null)
        {
            ConsoleRenderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            Region = region ?? renderer.Console.GetRegion();
        }

        protected ConsoleRenderer ConsoleRenderer { get; }

        public Region Region { get; }

        public virtual void Render(T value)
        {
            _effectiveRegion = new Region(
                Region.Left,
                Region.Top,
                Region.Width,
                Region.Height,
                false);

            _verticalOffset = 0;

            OnRender(value);
        }

        protected abstract void OnRender(T value);

        public void RenderTable<TItem>(
            IEnumerable<TItem> items,
            Action<ConsoleTable<TItem>> table)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var tableView = new ConsoleTable<TItem>(ConsoleRenderer);

            table(tableView);

            var left = 0;

            foreach (var column in tableView.Columns)
            {
                column.Left = left;
                column.CalculateSpans(items.ToList());
                left += column.Width;
            }

            var columnCount = tableView.Columns.Count;

            for (var rowIndex = 0; rowIndex <= items.Count(); rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
                {
                    var column = tableView.Columns[columnIndex];

                    column.FlushRow(
                        rowIndex,
                        _verticalOffset,
                        columnIndex == columnCount - 1,
                        ConsoleRenderer);
                }
            }
        }

        public void WriteLine()
        {
            if (_effectiveRegion.Height <= 1)
            {
                return;
            }

            _verticalOffset++;

            _effectiveRegion = new Region(
                _effectiveRegion.Left,
                _effectiveRegion.Top + 1,
                _effectiveRegion.Width,
                _effectiveRegion.Height - 1,
                false);
        }

        public void Write(object value)
        {
            ConsoleRenderer.RenderToRegion(value, _effectiveRegion);
        }

        public void WriteLine(object value)
        {
            Write(value);
            WriteLine();
        }

        protected Span Span(FormattableString formattable) =>
            ConsoleRenderer.Formatter.ParseToSpan(formattable);

        protected Span Span(object value) =>
            ConsoleRenderer.Formatter.Format(value);
    }
}
