namespace System.CommandLine.Rendering.Views
{
    public abstract class View
    {
        public event EventHandler Updated;

        public abstract void Render(ConsoleRenderer renderer, Region region);

        public abstract Size Measure(ConsoleRenderer renderer, Size maxSize);

        protected void OnUpdated() => Updated?.Invoke(this, EventArgs.Empty);
    }
}
