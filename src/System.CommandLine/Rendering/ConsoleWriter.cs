namespace System.CommandLine.Rendering
{
    public class ConsoleWriter
    {
        public ConsoleWriter(
            IConsole console = null,
            OutputMode mode = OutputMode.NonAnsi)
        {
            Console = console ?? Invocation.Console.Instance;
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
            RenderingSpanVisitor visitor;

            switch (Mode)
            {
                case OutputMode.NonAnsi:
                    visitor = new RenderingSpanVisitor(this, region);
                    break;
                case OutputMode.Ansi:
                    visitor = new AnsiRenderingSpanVisitor(this, region);
                    break;
                default:
                    throw new NotSupportedException();
            }

            visitor.Visit(span);
        }

        public virtual void WriteRawToRegion(
            string raw,
            Region region)
        {
            Console.Out.Write(raw);
        }
    }
}
