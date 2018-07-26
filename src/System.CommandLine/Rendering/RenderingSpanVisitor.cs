using System.Text;

namespace System.CommandLine.Rendering
{
    internal class AnsiRenderingSpanVisitor : RenderingSpanVisitor
    {
        public AnsiRenderingSpanVisitor(
            ConsoleWriter consoleWriter,
            Region region) : base(consoleWriter, region)
        {
        }
    }

    internal class RenderingSpanVisitor : SpanVisitor
    {
        private readonly ConsoleWriter _consoleWriter;
        private readonly Region _region;
        private readonly StringBuilder _buffer = new StringBuilder();
        private int _linesWritten;

        public RenderingSpanVisitor(
            ConsoleWriter consoleWriter,
            Region region)
        {
            _consoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _region = region ?? throw new ArgumentNullException(nameof(region));
        }

        public override void VisitContentSpan(ContentSpan contentSpan)
        {
            var text = contentSpan.ToString();

            foreach (var word in text.SplitForWrapping())
            {
                if (_linesWritten >= _region.Height)
                {
                    return;
                }

                var lengthWithCurrentWord = word.Length + _buffer.Length;

                if (_linesWritten > 0 && _buffer.Length == 0)
                {
                    MoveToNewLine();
                }

                if (lengthWithCurrentWord > _region.Width)
                {
                    if (word.Length > _region.Width)
                    {
                        // the word won't fit on a line by itself, so chop
                        _buffer.Append(
                            word.Substring(0,
                                           RemainingWidth()));
                    }
                    else
                    {
                        FlushLine();

                        if (_linesWritten >= _region.Height)
                        {
                            return;
                        }

                        MoveToNewLine();

                        _buffer.Append(word);
                        _buffer.Append(" ");
                    }
                }
                else
                {
                    _buffer.Append(word);
                    _buffer.Append(" ");
                }

                if (RemainingWidth() <= 0)
                {
                    FlushLine();
                }
            }

            if (_buffer.Length > 0)
            {
                FlushLine();
            }
        }

        private int RemainingWidth() => _region.Width - _buffer.Length;

        protected virtual void MoveToNewLine()
        {
            _consoleWriter.Console.Out.WriteLine();
        }

        protected virtual void FlushLine()
        {
            PadLine();

            _consoleWriter.Console.Out.Write(
                _buffer.ToString().Substring(0, _region.Width));
            _buffer.Clear();
            _linesWritten++;
        }

        protected virtual void PadLine()
        {
            if (RemainingWidth() > 0)
            {
                _buffer.Append(' ', RemainingWidth());
            }
        }
    }
}
