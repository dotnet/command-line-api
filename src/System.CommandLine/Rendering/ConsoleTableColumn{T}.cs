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
            ConsoleWriter consoleWriter)
        {
            RenderCell = renderCell ?? throw new ArgumentNullException(nameof(renderCell));
            ConsoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            Header = header;
        }

        public Func<T, object> RenderCell { get; }

        public FormattableString Header { get; }

        public void FlushRow(
            int index,
            ConsoleWriter consoleWriter)
        {
            if (_writers == null)
            {
                return;
            }

            var columnWriter = _writers[index];

            consoleWriter.WriteRawToRegion(
                columnWriter.ToString(),
                new Region(Height,
                             Width,
                             0,
                             0));
        }

        private int Width { get; set; } = 80;

        private int Height { get; set; } = 1;

        public void PrerenderAndCalculateWidth(IReadOnlyList<T> items)
        {
            _writers = new Dictionary<int, StringWriter>();

            _writers[0] = new StringWriter(ConsoleWriter.Formatter);

            _writers[0].Write(Header);

            for (var i = 0; i < items.Count; i++)
            {
                _writers[i + 1] = new StringWriter(ConsoleWriter.Formatter);

                var value = RenderCell(items[i]);

                _writers[i + 1].Write(ConsoleWriter.Formatter.Format(value));
            }

            var widest = _writers.Values.Max(v => v.GetStringBuilder().Length);

            var gutterEnd = widest + ColumnGutter;

            foreach (var writer in _writers.Values)
            {
                var whitespace = new string(' ', gutterEnd - writer.GetStringBuilder().Length);
                writer.Write(whitespace);
            }
        }

        public int ColumnGutter { get; } = 3;

        public ConsoleWriter ConsoleWriter { get; }
    }
}
