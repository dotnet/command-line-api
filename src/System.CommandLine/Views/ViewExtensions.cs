using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Views
{
    public static class ViewExtensions
    {
        public static void RenderTable<T>(
            this IConsoleView view,
            IReadOnlyCollection<T> items,
            Action<ConsoleTable<T>> table)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            var tableView = new ConsoleTable<T>(view.ConsoleWriter);

            table(tableView);

            foreach (var column in tableView.Columns)
            {
                column.Render(items.ToList());
            }

            for (var rowIndex = 0; rowIndex <= items.Count; rowIndex++)
            {
                foreach (var column in tableView.Columns)
                {
                    column.FlushRow(rowIndex, view.ConsoleWriter);
                }

                view.ConsoleWriter.WriteLine();
            }
        }

        internal static IReadOnlyCollection<string> SplitLines(this string source) =>
            source.Split('\n');
    }
}
