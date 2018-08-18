namespace System.CommandLine.Rendering
{
    public class ConsoleRenderer
    {
        public ConsoleRenderer(
            IConsole console = null,
            OutputMode mode = OutputMode.NonAnsi)
        {
            Console = console ?? Invocation.SystemConsole.Instance;
            Mode = mode;
        }

        public IConsole Console { get; }

        public SpanFormatter Formatter { get; } = new SpanFormatter();

        public OutputMode Mode { get; }

        public void RenderToRegion(
            object value,
            Region region)
        {
            var formatted = Formatter.Format(value);

            RenderToRegion(formatted, region);
        }

        public void RenderToRegion(
            FormattableString value,
            Region region)
        {
            var formatted = Formatter.ParseToSpan(value);

            RenderToRegion(formatted, region);
        }

        public void RenderToRegion(
            Span span,
            Region region)
        {
            ContentRenderingSpanVisitor visitor;

            switch (Mode)
            {
                case OutputMode.NonAnsi:
                    visitor = new NonAnsiRenderingSpanVisitor(
                        Console,
                        region);
                    break;

                case OutputMode.Ansi:
                    visitor = new AnsiRenderingSpanVisitor(
                        Console,
                        region);
                    break;

                case OutputMode.File:
                    visitor = new FileRenderingSpanVisitor(
                        Console.Out,
                        region);
                    break;

                default:
                    throw new NotSupportedException();
            }

            visitor.Visit(span);
        }
    }
}
