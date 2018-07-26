namespace System.CommandLine.Rendering
{
    public class ConsoleWriter
    {
        public ConsoleWriter(
            IConsole console,
            OutputMode mode = OutputMode.NonAnsi)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
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
            var formatted = Formatter.Format(value);

            RenderToRegion(formatted, region);
        }

        public void RenderToRegion(
            Span span,
            Region region)
        {
            var visitor = new RenderingSpanVisitor(this, region);

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
