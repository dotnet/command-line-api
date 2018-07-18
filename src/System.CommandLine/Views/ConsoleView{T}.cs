using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Views
{
    public abstract class ConsoleView<T> : IConsoleView<T>
    {
        protected ConsoleView(IConsoleWriter writer)
        {
            ConsoleWriter = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public abstract void Render(T value);

        public string Column { get; } = "  ";

        public IConsoleWriter ConsoleWriter { get; }

        public void RenderTable<TItem>(
            IReadOnlyCollection<TItem> items,
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

            foreach (var column in tableView.Columns)
            {
                column.Render(items.ToList());
            }

            for (var rowIndex = 0; rowIndex <= items.Count; rowIndex++)
            {
                foreach (var column in tableView.Columns)
                {
                    column.FlushRow(rowIndex, ConsoleWriter);
                }

                ConsoleWriter.WriteLine();
            }
        }
    }
}
