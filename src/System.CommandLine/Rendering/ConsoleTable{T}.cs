using System.Collections.Generic;

namespace System.CommandLine.Rendering
{
    public class ConsoleTable<T>
    {
        public ConsoleRenderer ConsoleRenderer { get; }

        public ConsoleTable(ConsoleRenderer consoleRenderer)
        {
            ConsoleRenderer = consoleRenderer ?? throw new ArgumentNullException(nameof(consoleRenderer));
        }

        public void RenderColumn(
            Span header,
            Func<T, Span> cell) =>
            Columns.Add(new ConsoleTableColumn<T>(
                            header,
                            cell));

        public void RenderColumn(
            object header,
            Func<T, object> cell) =>
            Columns.Add(new ConsoleTableColumn<T>(
                            ConsoleRenderer.Formatter.Format(header),
                            value => ConsoleRenderer.Formatter.Format(cell(value))));

        internal IList<ConsoleTableColumn<T>> Columns { get; } = new List<ConsoleTableColumn<T>>();
    }
}
