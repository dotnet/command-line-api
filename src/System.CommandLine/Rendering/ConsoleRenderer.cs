using System.CommandLine.Invocation;

namespace System.CommandLine.Rendering
{
    public class ConsoleRenderer : IRenderer
    {
        public ConsoleRenderer(
            IConsole console = null,
            OutputMode mode = OutputMode.Auto)
        {
            Console = console ?? SystemConsole.Instance;

            Mode = mode == OutputMode.Auto
                       ? Console.DetectOutputMode()
                       : mode;
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
            SpanVisitor visitor;

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
                        new Region(region.Left,
                                   region.Top,
                                   region.Width,
                                   region.Height,
                                   false));
                    break;

                default:
                    throw new NotSupportedException();
            }

            visitor.Visit(span);
        }

        public Size MeasureSpan(Span span, Size maxSize)
        {
            var measuringVisitor = new SpanMeasuringVisitor(new Region(0, 0, maxSize.Width, maxSize.Height));
            measuringVisitor.Visit(span);
            return new Size(measuringVisitor.Width, measuringVisitor.Height);
        }
    }
}
