// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class ConsoleRenderer
    {
        private readonly bool _resetAfterRender;
        private readonly IConsole _console;
        private OutputMode _mode;
        private readonly ITerminal _terminal;

        public ConsoleRenderer(
            IConsole console,
            OutputMode mode = OutputMode.Auto,
            bool resetAfterRender = false)
        {
            _console = console ??
                       throw new ArgumentNullException(nameof(console));
            _mode = mode;

            _terminal = console as ITerminal;
            _resetAfterRender = resetAfterRender;
        }

        public TextSpanFormatter Formatter { get; } = new TextSpanFormatter();

        public void RenderToRegion(
            object value,
            Region region)
        {
            var formatted = Formatter.Format(value);

            RenderToRegion(formatted, region);
        }

        public void Append(FormattableString value) => Append(Formatter.ParseToSpan(value));

        public void Append(TextSpan span)
        {
            Render(span);
        }

        public void RenderToRegion(
            FormattableString value,
            Region region)
        {
            var formatted = Formatter.ParseToSpan(value);

            RenderToRegion(formatted, region);
        }

        public void RenderToRegion(
            TextSpan span,
            Region region)
        {
            if (region == null)
            {
                throw new ArgumentNullException(nameof(region));
            }

            Render(span, region);
        }

        private void Render(TextSpan span, Region region = null)
        {
            if (span == null)
            {
                span = TextSpan.Empty();
            }
            else if (_resetAfterRender)
            {
                span = new ContainerSpan(
                    span,
                    ForegroundColorSpan.Reset(),
                    BackgroundColorSpan.Reset());
            }

            if (_mode == OutputMode.Auto)
            {
                _mode = _terminal?.DetectOutputMode() ??
                        OutputMode.PlainText;
            }


            TextSpanVisitor visitor;
            switch (_mode)
            {
                case OutputMode.NonAnsi:
                    visitor = new NonAnsiRenderingSpanVisitor(
                        _terminal,
                        region);
                    break;

                case OutputMode.Ansi:
                    visitor = new AnsiRenderingSpanVisitor(
                        _console,
                        region);
                    break;

                case OutputMode.PlainText:
                    visitor = new FileRenderingSpanVisitor(
                        _console.Out,
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

        internal static Size MeasureSpan(TextSpan span, Size maxSize)
        {
            var measuringVisitor = new SpanMeasuringVisitor(new Region(0, 0, maxSize.Width, maxSize.Height));
            measuringVisitor.Visit(span);
            return new Size(measuringVisitor.Width, measuringVisitor.Height);
        }

        public Region GetRegion()
        {
            return (_terminal as IRenderable)?.GetRegion();
        }
    }
}
