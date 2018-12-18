// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Invocation;

namespace System.CommandLine.Rendering
{
    public class ConsoleRenderer
    {
        private readonly bool _resetAfterRender;
        private readonly IConsole _console;
        private readonly ITerminal _terminal;

        public ConsoleRenderer(
            IConsole console = null,
            OutputMode mode = OutputMode.Auto,
            bool resetAfterRender = false)
        {
            _console = console ?? SystemConsole.Create();
            _terminal = console as ITerminal;
            _resetAfterRender = resetAfterRender;

            Mode = mode == OutputMode.Auto
                       ? _terminal.DetectOutputMode()
                       : mode;
        }

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
                        _terminal,
                        region);
                    break;

                case OutputMode.Ansi:
                    visitor = new AnsiRenderingSpanVisitor(
                        _console,
                        region);
                    break;

                case OutputMode.File:
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

        public Size MeasureSpan(Span span, Size maxSize)
        {
            var measuringVisitor = new SpanMeasuringVisitor(new Region(0, 0, maxSize.Width, maxSize.Height));
            measuringVisitor.Visit(span);
            return new Size(measuringVisitor.Width, measuringVisitor.Height);
        }

        public Region GetRegion()
        {
            return _terminal?.GetRegion() ;
        }
    }
}
