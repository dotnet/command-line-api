using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public class ConsoleView<T> : IConsoleView<T>
    {
        public ConsoleView(
            ConsoleWriter writer,
            Region region = null)
        {
            ConsoleWriter = writer ?? throw new ArgumentNullException(nameof(writer));
            Region = region ?? writer.Console.GetRegion();
        }

        protected ConsoleWriter ConsoleWriter { get; }

        public Region Region { get; }

        // FIX: (ConsoleView) name Write vs Render sensically
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

            var tableView = new ConsoleTable<TItem>(ConsoleWriter);

            table(tableView);

            var left = 0;

            foreach (var column in tableView.Columns)
            {
                column.Left = left;
                column.CalculateSpans(items.ToList());
            }

            for (var rowIndex = 0; rowIndex <= items.Count(); rowIndex++)
            {
                foreach (var column in tableView.Columns)
                {
                    column.FlushRow(rowIndex, ConsoleWriter);
                }

                WriteLine();
            }
        }

        public void WriteLine()
        {
            ConsoleWriter.WriteLine();
        }

        public void Write(object value)
        {
            ConsoleWriter.RenderToRegion(value, Region);
        }

        public void WriteLine(object value)
        {
            Write(value);
            WriteLine();
        }
    }
}
