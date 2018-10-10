using System.CommandLine.Invocation;

namespace System.CommandLine.Rendering
{
    public class ConsoleRenderer
    {
        private readonly bool _resetAfterRender;

        public ConsoleRenderer(
            IConsole console = null,
            OutputMode mode = OutputMode.Auto,
            bool resetAfterRender = false)
        {
            Console = console ?? SystemConsole.Instance;
            _resetAfterRender = resetAfterRender;
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

            if (span == null)
            {
                span = Span.Empty();
            }
            else if (_resetAfterRender)
            {
                span = new ContainerSpan(
                    span,
                    ForegroundColorSpan.Reset(),
                    BackgroundColorSpan.Reset());
            }

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
