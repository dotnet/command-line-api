using System.Collections.Generic;

namespace System.CommandLine.Views
{
    public class ConsoleTable<T>
    {
        public IFormatProvider FormatProvider { get; }

        internal ConsoleTable(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider ?? throw new ArgumentNullException(nameof(formatProvider));
        }

        public void RenderColumn(
            string header,
            Func<T, object> cell)
        {
            Columns.Add(new ConsoleTableColumn<T>($"{header}", cell, FormatProvider));
        }

        internal void RenderColumn(
            FormattableString header,
            Func<T, object> cell)
        {
            Columns.Add(new ConsoleTableColumn<T>(header, cell, FormatProvider));
        }

        internal IList<ConsoleTableColumn<T>> Columns { get; } = new List<ConsoleTableColumn<T>>();
    }
}
