using System.Collections.Generic;

namespace System.CommandLine.Rendering
{
    public class ConsoleTable<T>
    {
        public ConsoleWriter ConsoleWriter { get; }

        public ConsoleTable(ConsoleWriter consoleWriter)
        {
            ConsoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
        }

        public void RenderColumn(
            FormattableString header,
            Func<T, object> cell) =>
            Columns.Add(new ConsoleTableColumn<T>(
                            ConsoleWriter.Formatter.ParseToSpan(header),
                            value => ConsoleWriter.Formatter.Format(cell(value)),
                            ConsoleWriter));

        public void RenderColumn(
            Span header,
            Func<T, object> cell) =>
            Columns.Add(new ConsoleTableColumn<T>(
                            header,
                            value => ConsoleWriter.Formatter.Format(cell(value)),
                            ConsoleWriter));

        public void RenderColumn(
            object header,
            Func<T, object> cell) =>
            Columns.Add(new ConsoleTableColumn<T>(
                            ConsoleWriter.Formatter.Format(header),
                            value => ConsoleWriter.Formatter.Format(cell(value)),
                            ConsoleWriter));

        internal IList<ConsoleTableColumn<T>> Columns { get; } = new List<ConsoleTableColumn<T>>();
    }
}
