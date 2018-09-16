using System.Collections.Generic;
using System.CommandLine.Rendering.Spans;

namespace System.CommandLine.Rendering.Views
{
    public class TableView<T> : LayoutView<TableColumnView<T>>
    {
        public override void Render(Region region, IRenderer renderer)
        {

        }

        public void RenderColumn(
            Span header,
            Func<T, Span> cell) =>
            Columns.Add(new ConsoleTableColumn<T>(header, cell));

        public void RenderColumn(
            object header,
            Func<T, object> cell) =>
            Columns.Add(new ConsoleTableColumn<T>(
                            ConsoleRenderer.Formatter.Format(header),
                            value => ConsoleRenderer.Formatter.Format(cell(value))));

        internal IList<ConsoleTableColumn<T>> Columns { get; } = new List<ConsoleTableColumn<T>>();
    }
}
