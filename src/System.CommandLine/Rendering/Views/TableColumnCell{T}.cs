using System.CommandLine.Rendering.Spans;

namespace System.CommandLine.Rendering.Views
{
    public class TableCellContentView<T> : ContentView
    {
        private T Data { get; }
        public override Span Span { get; set; }

        public TableCellContentView(T data)
        {
            Data = data;
        }
    }
}
