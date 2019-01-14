namespace System.CommandLine.Rendering
{
    public static class Terminal
    {
        public static void Render(
            this ITerminal terminal,
            Span span,
            Region region = null)
        {
            if (terminal is IRenderable t)
            {
                var renderer = new ConsoleRenderer(terminal, t.OutputMode);

                renderer.RenderToRegion(span, region ?? t.GetRegion());
            }
        }

        public static void Render(
            this ITerminal terminal,
            FormattableString value,
            Region region = null)
        {
            if (terminal is IRenderable t)
            {
                var renderer = new ConsoleRenderer(terminal, t.OutputMode);

                var span = renderer.Formatter.Format(value);

                renderer.RenderToRegion(span, region ?? t.GetRegion());
            }
        }
    }
}
