using System.Linq;
using System.Text;

namespace System.CommandLine.Rendering
{
    internal class RenderingSpanVisitor : SpanVisitor
    {
        private readonly StringBuilder _buffer = new StringBuilder();

        private int _positionOnLine;

        public RenderingSpanVisitor(
            ConsoleWriter consoleWriter,
            Region region)
        {
            ConsoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            Region = region ?? throw new ArgumentNullException(nameof(region));
        }

        protected ConsoleWriter ConsoleWriter { get; }

        protected int LinesWritten { get; private set; }

        protected Region Region { get; }

        public override void VisitContentSpan(ContentSpan contentSpan)
        {
            var text = contentSpan.ToString();

            foreach (var word in text.SplitIntoWordsForWrapping())
            {
                if (WroteMoreLinesThanRegionHeight)
                {
                    return;
                }

                AppendWord(word);
            }

            if (_buffer.Length > 0)
            {
                Flush();
            }
        }

        protected override void Stop(Span span)
        {
            FlushLine();
        }

        private bool WroteMoreLinesThanRegionHeight => LinesWritten >= Region.Height;

        protected int RemainingWidthOnLine => Region.Width - _positionOnLine;

        protected virtual void FlushLine()
        {
            PadRemainderOfLineWithWhitespace();

            Flush();

            LinesWritten++;
        }

        protected virtual void StartNewLine()
        {
            ConsoleWriter.Console.Out.WriteLine();
        }

        private void PadRemainderOfLineWithWhitespace()
        {
            var remainingWidthOnLine = RemainingWidthOnLine;

            if (_positionOnLine > 0 &&
                remainingWidthOnLine > 0)
            {
                _buffer.Append(new string(' ', remainingWidthOnLine));
                _positionOnLine += remainingWidthOnLine;
            }
        }

        protected virtual void Flush()
        {
            ConsoleWriter.Console.Out.Write(_buffer.ToString());

            _buffer.Clear();
        }

        private void AppendWord(string value)
        {
            if (_positionOnLine == 0 &&
                string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (LinesWritten > 0 &&
                _buffer.Length == 0)
            {
                _positionOnLine = 0;
                StartNewLine();
            }

            var mustTruncate = value.Length > Region.Width;

            if (mustTruncate)
            {
                value = value.Substring(0, Math.Min(value.Length, RemainingWidthOnLine));
            }

            if (value.Length > RemainingWidthOnLine)
            {
                if (value.TrimEnd().Length > RemainingWidthOnLine)
                {
                    FlushLine();
                    StartNewLine();
                    _positionOnLine = 0;
                }
                else
                {
                    value = value.TrimEnd();
                }
            }

            _buffer.Append(value);
            _positionOnLine += value.Length;

            if (RemainingWidthOnLine <= 0)
            {
                FlushLine();
                _positionOnLine = 0;
            }
        }
    }
}
