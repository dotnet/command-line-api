using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public class ConsoleView<T> : IConsoleView<T>
    {
        public ConsoleView(
            ConsoleRenderer renderer,
            Region region = null)
        {
            ConsoleRenderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            Region = region ?? renderer.Console.GetRegion();
        }

        protected ConsoleRenderer ConsoleRenderer { get; }

        public Region Region { get; }

        // TODO: (ConsoleView) name Write vs Render sensically
        public virtual void Render(T value)
        {
            Write(value);
        }

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

            for (var rowIndex = 0; rowIndex <= items.Count(); rowIndex++)
            {
                foreach (var column in tableView.Columns)
                {
                    column.FlushRow(rowIndex, ConsoleRenderer);
                }

                WriteLine();
            }
        }

        public void WriteLine()
        {
            ConsoleRenderer.Console.Out.WriteLine();
        }

        public void Write(object value)
        {
            ConsoleRenderer.RenderToRegion(value, Region);
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
