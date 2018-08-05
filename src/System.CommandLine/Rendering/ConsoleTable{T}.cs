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
            FormattableString header,
            Func<T, object> cell) =>
            RenderColumn(
                ConsoleRenderer.Formatter.ParseToSpan(header),
                cell);

        public void RenderColumn(
            Span header,
            Func<T, object> cell) =>
            Columns.Add(new ConsoleTableColumn<T>(
                            header,
                            value => ConsoleRenderer.Formatter.Format(cell(value)),
                            ConsoleRenderer));

        public void RenderColumn(
            object header,
            Func<T, object> cell) =>
            RenderColumn(
                ConsoleRenderer.Formatter.Format(header),
                cell);

        internal IList<ConsoleTableColumn<T>> Columns { get; } = new List<ConsoleTableColumn<T>>();
    }
}
