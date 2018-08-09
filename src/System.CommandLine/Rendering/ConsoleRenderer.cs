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

            if (Mode == OutputMode.Auto)
            {
                
            }

            switch (Mode)
            {
                case OutputMode.NonAnsi:
                    visitor = new ContentRenderingSpanVisitor(
                        Console.Out,
                        region);
                    break;

                case OutputMode.Ansi:
                    visitor = new AnsiRenderingSpanVisitor(
                        Console.Out,
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

        protected void WriteLine()
        {
            switch (Mode)
            {
                case OutputMode.NonAnsi:
                    Console.Out.WriteLine();
                    break;
                case OutputMode.Ansi:
                    Console.Out.Write(Ansi.Cursor.Move.Down());
                    Console.Out.Write(Ansi.Cursor.Move.NextLine(1));
                    break;
                case OutputMode.File:
                    Console.Out.WriteLine();
                    break;
            }
        }
    }
}
