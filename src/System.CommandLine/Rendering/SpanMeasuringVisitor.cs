namespace System.CommandLine.Rendering
{
    internal sealed class SpanMeasuringVisitor : SpanVisitor
    {
        private bool _LastSpanEndedWithWhitespace;
        private int _PositionOnLine;

        public SpanMeasuringVisitor ( Region region)
        {
            Region = region ?? throw new ArgumentNullException(nameof(region));
        }

        private int PositionOnLine
        {
            get => _PositionOnLine;
            set {
                if (value > Width)
                {
                    Width = value;
                }
                _PositionOnLine = value;
            }
        }

        public int Width { get; set; }
        public int Height => LinesWritten;

        private int LinesWritten { get; set; }

        private Region Region { get; }

        public override void VisitContentSpan(ContentSpan span)
        {
            var text = span.ToString();

            // if text from the previous line was not truncated because the word was separated by an ANSI code, it should be truncated
            var skipWordRemainderFromPreviousLine = !_LastSpanEndedWithWhitespace
                           && PositionOnLine == 0
                           && LinesWritten > 0
                           && !text.StartsWithWhitespace();

            foreach (var word in text.SplitForWrapping())
            {
                if (skipWordRemainderFromPreviousLine)
                {
                    skipWordRemainderFromPreviousLine = false;
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

            _LastSpanEndedWithWhitespace = text.EndsWithWhitespace();
        }

        protected override void Stop(Span span)
        {
            if (PositionOnLine > 0 ||
                span.ContentLength == 0)
            {
                FlushLine();
            }

            //ClearRemainingHeight();
        }

        private bool FilledRegionHeight => LinesWritten >= Region.Height;

        private int RemainingWidthInRegion => Region.Width - PositionOnLine;

        private void FlushLine()
        {
            //ClearRemainingWidth();

            LinesWritten++;

            PositionOnLine = 0;
        }

        private void ClearRemainingWidth()
        {
            if (!Region.IsOverwrittenOnRender)
            {
                return;
            }

            var remainingWidthOnLine = RemainingWidthInRegion;

            if (remainingWidthOnLine > 0)
            {
                PositionOnLine += remainingWidthOnLine;
            }
        }

        private void ClearRemainingHeight()
        {
            if (!Region.IsOverwrittenOnRender)
            {
                return;
            }

            while (TryStartNewLine())
            {
                FlushLine();
            }
        }

        private bool TryAppendWord(string value)
        {
            if (PositionOnLine == 0 &&
                string.IsNullOrWhiteSpace(value))
            {
                // omit whitespace if it's at the beginning of the line
                return true;
            }

            var mustTruncate = value.Length > Region.Width;

            if (mustTruncate)
            {
                value = value.Substring(0, Math.Min(value.Length, RemainingWidthInRegion));
            }

            if (value.Length > RemainingWidthInRegion)
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

            PositionOnLine += value.Length;

            if (RemainingWidthInRegion <= 0)
            {
                FlushLine();
            }

            return !FilledRegionHeight;

            bool WillFitIfEndIsTrimmed()
            {
                return value.TrimEnd().Length <= RemainingWidthInRegion;
            }
        }

        private bool TryStartNewLine()
        {
            return !FilledRegionHeight;
        }
    }
}
