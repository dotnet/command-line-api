using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Views
{
    internal class ConsoleTableColumn<T>
    {
        private Dictionary<int, StringWriter> _writers;

        public ConsoleTableColumn(
            FormattableString header,
            Func<T, object> renderCell,
            IFormatProvider formatProvider = null)
        {
            RenderCell = renderCell ?? throw new ArgumentNullException(nameof(renderCell));
            FormatProvider = formatProvider;
            Header = header;
        }

        public Func<T, object> RenderCell { get; }

        public FormattableString Header { get; }

        public void FlushRow(
            int index,
            IConsoleWriter consoleWriter)
        {
            if (_writers == null)
            {
                return;
            }

            var columnWriter = _writers[index];

            consoleWriter.Write(columnWriter.ToString());
        }

        public void Render(IReadOnlyList<T> items)
        {
            _writers = new Dictionary<int, StringWriter>();

            _writers[0] = new StringWriter(FormatProvider);

            _writers[0].Write(Header);

            for (var i = 0; i < items.Count; i++)
            {
                _writers[i + 1] = new StringWriter();

                var value = RenderCell(items[i]);

                _writers[i + 1].Write(value);
            }

            var longest = _writers.Values.Max(v => v.GetStringBuilder().Length);
            var gutterEnd = longest + ColumnGutter;

            foreach (var writer in _writers.Values)
            {
                writer.Write(new string(' ', gutterEnd - writer.GetStringBuilder().Length));
            }
        }

        public int ColumnGutter { get; } = 3;

        public IFormatProvider FormatProvider { get; }
    }
}
