namespace System.CommandLine.Rendering.Views
{
    public class ContentView<T> : ContentView
    {
        public ContentView(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public override Size Measure(ConsoleRenderer renderer, Size maxSize)
        {
            EnsureSpanCreated(renderer);
            return base.Measure(renderer, maxSize);
        }

        public override void Render(ConsoleRenderer renderer, Region region)
        {
            EnsureSpanCreated(renderer);
            base.Render(renderer, region);
        }

        private void EnsureSpanCreated(ConsoleRenderer renderer)
        {
            if (Span == null)
            {
                Span = renderer.Formatter.Format(Value);
            }
        }
    }
}
