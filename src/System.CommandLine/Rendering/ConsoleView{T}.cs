namespace System.CommandLine.Rendering
{
    public abstract class ConsoleView<T> : IConsoleView<T>
    {
        private Region _effectiveRegion;
        private int _verticalOffset;

        protected ConsoleView(
            ConsoleRenderer renderer,
            Region region = null)
        {
            ConsoleRenderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            Region = region ?? renderer.Console.GetRegion();

            SetEffectiveRegion();
        }

        protected ConsoleRenderer ConsoleRenderer { get; }

        public Region Region { get; }

        public virtual void Render(T value)
        {
            SetEffectiveRegion();

            OnRender(value);
        }

        protected abstract void OnRender(T value);

        public void WriteLine()
        {
            if (_effectiveRegion.Height <= 1)
            {
                return;
            }

            _verticalOffset++;

            _effectiveRegion = new Region(
                _effectiveRegion.Left,
                _effectiveRegion.Top + 1,
                _effectiveRegion.Width,
                _effectiveRegion.Height - 1,
                Region.IsOverwrittenOnRender);
        }

        public void Write(object value)
        {
            ConsoleRenderer.RenderToRegion(value, _effectiveRegion);
        }

        public void WriteLine(object value)
        {
            Write(value);
            WriteLine();
        }

        protected Span Span(FormattableString formattable) =>
            ConsoleRenderer.Formatter.ParseToSpan(formattable);

        protected Span Span(object value) =>
            ConsoleRenderer.Formatter.Format(value);

        private void SetEffectiveRegion()
        {
            _effectiveRegion = new Region(
                Region.Left,
                Region.Top,
                Region.Width,
                Region.Height,
                Region.IsOverwrittenOnRender);
        }
    }
}
