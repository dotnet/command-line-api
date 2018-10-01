namespace System.CommandLine.Rendering.Views
{
    public interface ITableViewColumn<T>
    {
        ColumnDefinition ColumnDefinition { get; }

        View Header { get; }

        View GetCell(T item, SpanFormatter formatter);
    }
}
