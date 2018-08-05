using System.IO;
using System.Text;

namespace System.CommandLine.Rendering
{
    internal class ContentRenderingSpanVisitor : SpanVisitor
    {
        private readonly StringBuilder _buffer = new StringBuilder();

        private int _positionOnLine;

        public ContentRenderingSpanVisitor(
            TextWriter writer,
            Region region)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            Region = region ?? throw new ArgumentNullException(nameof(region));
        }

        protected TextWriter Writer { get; }

        protected int LinesWritten { get; private set; }

        protected Region Region { get; }

        public override void VisitContentSpan(ContentSpan contentSpan)
        {
            var text = contentSpan.ToString();

            foreach (var word in text.SplitIntoWordsForWrapping())
            {
                if (!TryAppendWord(word))
                {
                    break;
                }
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

            _positionOnLine = 0;
        }

        protected virtual void StartNewLine()
        {
            Writer.WriteLine();
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
            Writer.Write(_buffer.ToString());

            _buffer.Clear();
        }

        private bool TryAppendWord(string value)
        {
            if (_positionOnLine == 0 &&
                string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            if (LinesWritten > 0 &&
                _buffer.Length == 0)
            {
                StartNewLine();
            }

            var mustTruncate = value.Length > Region.Width;

            if (mustTruncate)
            {
                value = value.Substring(0, Math.Min(value.Length, RemainingWidthOnLine));
            }

            if (value.Length > RemainingWidthOnLine)
            {
                if (WillFitIfEndIsTrimmed())
                {
                    value = value.TrimEnd();
                }
                else
                {
                    FlushLine();

                    if (WroteMoreLinesThanRegionHeight)
                    {
                        return false;
                    }

                    StartNewLine();
                }
            }

            _buffer.Append(value);
            _positionOnLine += value.Length;

            if (RemainingWidthOnLine <= 0)
            {
                FlushLine();
            }

            return !WroteMoreLinesThanRegionHeight;

            bool WillFitIfEndIsTrimmed()
            {
                return value.TrimEnd().Length <= RemainingWidthOnLine;
            }
        }
    }
}
