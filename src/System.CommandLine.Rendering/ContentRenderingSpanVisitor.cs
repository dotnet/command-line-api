// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.IO;
using System.Text;

namespace System.CommandLine.Rendering
{
    internal abstract class ContentRenderingSpanVisitor : TextSpanVisitor
    {
        private readonly StringBuilder _buffer = new();

        private int _positionOnLine;
        private bool _lastSpanEndedWithWhitespace;
        private int _cursorLeft = -1;
        private int _cursorTop = -1;

        protected ContentRenderingSpanVisitor(
            IStandardStreamWriter writer,
            Region region)
        {
            Writer = writer ?? throw new ArgumentNullException(nameof(writer));
            Region = region ?? throw new ArgumentNullException(nameof(region));
        }

        protected IStandardStreamWriter Writer { get; }

        protected int LinesWritten { get; private set; }

        protected Region Region { get; }

        protected override void Start(TextSpan span)
        {
            TrySetCursorPosition(Region.Left, Region.Top);
        }

        public override void VisitContentSpan(ContentSpan span)
        {
            var text = span.ToString();

            // if text from the previous line was not truncated because the word was separated by an ANSI code, it should be truncated
            var skipWordRemainderFromPreviousLine = !_lastSpanEndedWithWhitespace
                           && _positionOnLine == 0
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

            if (_buffer.Length > 0)
            {
                Flush();
            }

            _lastSpanEndedWithWhitespace = text.EndsWithWhitespace();
        }

        protected override void Stop(TextSpan span)
        {
            if (_positionOnLine > 0 ||
                span.ContentLength == 0)
            {
                FlushLine();
            }

            ClearRemainingHeight();
        }

        protected bool FilledRegionHeight => LinesWritten >= Region.Height;

        private int RemainingWidthInRegion => Region.Width - _positionOnLine;

        private void FlushLine()
        {
            TryClearRemainingWidth();

            Flush();

            LinesWritten++;

            _positionOnLine = 0;
        }

        protected virtual void TryClearRemainingWidth()
        {
            if (!Region.IsOverwrittenOnRender)
            {
               return;
            }

            ClearRemainingWidth();
        }

        protected void ClearRemainingWidth()
        {
            var remainingWidthOnLine = RemainingWidthInRegion;

            if (remainingWidthOnLine > 0)
            {
                _buffer.Append(new string(' ', remainingWidthOnLine));
                _positionOnLine += remainingWidthOnLine;
            }
        }

        protected virtual void ClearRemainingHeight()
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

        private void Flush()
        {
            Writer.Write(_buffer.ToString());

            _buffer.Clear();
        }

        private bool TryAppendWord(string value)
        {
            if (_positionOnLine == 0 &&
                string.IsNullOrWhiteSpace(value) &&
                LinesWritten > 0)
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

            _buffer.Append(value);
            _positionOnLine += value.Length;

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

        protected virtual bool TryStartNewLine()
        {
            if (FilledRegionHeight)
            {
                return false;
            }

            TrySetCursorPosition(Region.Left, Region.Top + LinesWritten);

            return true;
        }

        private void TrySetCursorPosition(
            int left, 
            int? top = null)
        {
            if (left == _cursorLeft &&
                top == _cursorTop)
            {
                return;
            }

            _cursorLeft = left;
            if (top != null)
            {
                _cursorTop = top.Value;
            }

            SetCursorPosition(_cursorLeft, _cursorTop);
        }

        protected abstract void SetCursorPosition(
            int? left = null, 
            int? top = null);
    }
}
