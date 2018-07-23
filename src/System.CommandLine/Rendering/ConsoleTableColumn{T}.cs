using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Rendering
{
    internal class ConsoleTableColumn<T>
    {
        private Dictionary<int, StringWriter> _writers;

        public ConsoleTableColumn(
            FormattableString header,
            Func<T, object> renderCell,
            IConsoleWriter consoleWriter)
        {
            RenderCell = renderCell ?? throw new ArgumentNullException(nameof(renderCell));
            ConsoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
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

            _writers[0] = new StringWriter(ConsoleWriter);

            _writers[0].Write(Header);

            for (var i = 0; i < items.Count; i++)
            {
                _writers[i + 1] = new StringWriter(ConsoleWriter);

                var value = RenderCell(items[i]);

                _writers[i + 1].Write(ConsoleWriter.Format(value));
            }

            var longest = _writers.Values.Max(v => v.GetStringBuilder().Length);
            var gutterEnd = longest + ColumnGutter;

            foreach (var writer in _writers.Values)
            {
                writer.Write(new string(' ', gutterEnd - writer.GetStringBuilder().Length));
            }
        }

        public int ColumnGutter { get; } = 3;

        public IConsoleWriter ConsoleWriter { get; }
    }
}
