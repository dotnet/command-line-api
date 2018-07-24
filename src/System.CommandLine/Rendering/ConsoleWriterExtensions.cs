using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public static class ConsoleWriterExtensions
    {
        public static void RenderTable<TItem>(
            this ConsoleWriter formatProvider,
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

            var tableView = new ConsoleTable<TItem>(formatProvider);

            table(tableView);

            foreach (var column in tableView.Columns)
            {
                column.PrerenderAndCalculateWidth(items.ToList());
            }

            for (var rowIndex = 0; rowIndex <= items.Count(); rowIndex++)
            {
                foreach (var column in tableView.Columns)
                {
                    column.FlushRow(rowIndex, formatProvider);
                }

                formatProvider.WriteLine();
            }
        }
    }
}
