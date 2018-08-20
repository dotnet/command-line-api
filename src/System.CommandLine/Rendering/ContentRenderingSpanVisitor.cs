using System.IO;
using System.Linq;
using System.Text;

namespace System.CommandLine.Rendering
{
    internal abstract class ContentRenderingSpanVisitor : SpanVisitor
    {
        private readonly StringBuilder _buffer = new StringBuilder();

        private int _positionOnLine;
        private bool _lastSpanEndedWithWhitespace;

        protected ContentRenderingSpanVisitor(
            TextWriter writer,
            Region region)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            Region = region ?? throw new ArgumentNullException(nameof(region));
        }

        protected TextWriter Writer { get; }

        protected int LinesWritten { get; private set; }

        protected Region Region { get; }

        public override void VisitContentSpan(ContentSpan span)
        {
            var text = span.ToString();

            var truncate = !_lastSpanEndedWithWhitespace
                           && _positionOnLine == 0
                           && LinesWritten > 0
                           && !text.StartsWithWhitespace();

            foreach (var word in text.SplitForWrapping())
            {
                if (truncate)
                {
                    truncate = false;
                    continue;
                }

                if (word.IsNewLine())
                {
                    if (TryStartNewLine())
                    {
                        FlushLine();
                    }
                }
                else if (!TryAppendWord(word))
                {
                    break;
                }
            }

            if (_buffer.Length > 0)
            {
                Flush();
            }

            _lastSpanEndedWithWhitespace = text.EndsWithWhitespace();
        }

        protected override void Stop(Span span)
        {
            if (_positionOnLine > 0 ||
                span.ContentLength == 0)
            {
                FlushLine();
            }

            if (Region.IsOverwrittenOnRender)
            {
                while (TryStartNewLine())
                {
                    FlushLine();
                }
            }
        }

        protected bool FilledRegionHeight => LinesWritten >= Region.Height;

        private int RemainingWidthOnLine => Region.Width - _positionOnLine;

        private void FlushLine()
        {
            PadRemainderOfLineWithWhitespace();

            Flush();

            LinesWritten++;

            _positionOnLine = 0;
        }

        protected abstract void StartNewLine();

        private void PadRemainderOfLineWithWhitespace()
        {
            var remainingWidthOnLine = RemainingWidthOnLine;

            if (remainingWidthOnLine > 0)
            {
                _buffer.Append(new string(' ', remainingWidthOnLine));
                _positionOnLine += remainingWidthOnLine;
            }
        }

        private void Flush()
        {
            Writer.Write(_buffer.ToString());

            _buffer.Clear();
        }

        private bool TryAppendWord(string value)
        {
            if (_positionOnLine == 0 &&
                string.IsNullOrWhiteSpace(value))
            {
                // omit whitespace if it's at the beginning of the line
                return true;
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
                }
            }

            if (!TryStartNewLine())
            {
                return false;
            }

            _buffer.Append(value);
            _positionOnLine += value.Length;

            if (RemainingWidthOnLine <= 0)
            {
                FlushLine();
            }

            return !FilledRegionHeight;

            bool WillFitIfEndIsTrimmed()
            {
                return value.TrimEnd().Length <= RemainingWidthOnLine;
            }
        }

        protected virtual bool TryStartNewLine()
        {
            if (FilledRegionHeight)
            {
                return false;
            }

            if (LinesWritten > 0 &&
                _positionOnLine == 0)
            {
                StartNewLine();
            }

            return true;
        }
    }
}
