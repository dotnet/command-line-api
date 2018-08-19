// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace System.CommandLine.Tests
{
    public class TestConsole : IConsole
    {
        private int _cursorLeft;
        private int _cursorTop;
        private readonly RecordingWriter _error;
        private readonly RecordingWriter _out;
        private readonly List<ConsoleEvent> _events = new List<ConsoleEvent>();
        private readonly StringBuilder _outBuffer = new StringBuilder();
        private readonly StringBuilder _ansiCodeBuffer = new StringBuilder();

        public TestConsole()
        {
            _out = new RecordingWriter();

            _out.CharWritten += OnCharWrittenToOut;

            _error = new RecordingWriter();
        }

        private void OnCharWrittenToOut(char c)
        {
            if (_ansiCodeBuffer.Length == 0 &&
                c != Ansi.Esc[0])
            {
                _outBuffer.Append(c);
            }
            else
            {
                _ansiCodeBuffer.Append(c);

                if (char.IsLetter(c))
                {
                    // terminate the in-progress ANSI sequence

                    var escapeSequence = _ansiCodeBuffer.ToString();

                    _ansiCodeBuffer.Clear();

                    RecordEvent(
                        new AnsiControlCodeWritten(
                            new AnsiControlCode(
                                escapeSequence)));
                }
            }
        }

        public class AnsiControlCodeWritten : ConsoleEvent
        {
            public AnsiControlCodeWritten(AnsiControlCode ansiControlCode)
            {
                Code = ansiControlCode ?? throw new ArgumentNullException(nameof(ansiControlCode));
            }

            public AnsiControlCode Code { get; }
        }

        public TextWriter Error => _error;

        public TextWriter Out => _out;

        public virtual ConsoleColor ForegroundColor { get; set; }

        public int Height { get; set; } = 100;

        public int Width { get; set; } = 100;

        public virtual void ResetColor()
        {
        }

        public Region GetRegion() =>
            new Region(0,
                       0,
                       Width,
                       Height);

        public int CursorLeft
        {
            get => _cursorLeft;
            set => SetCursorPosition(value, _cursorTop);
        }

        public int CursorTop
        {
            get => _cursorTop;
            set => SetCursorPosition(_cursorLeft, value);
        }

        public IEnumerable<ConsoleEvent> Events
        {
            get
            {
                foreach (var e in _events)
                {
                    yield return e;
                }

                var unflushedOutput = UnflushedOutput;

                if (unflushedOutput.Length > 0)
                {
                    yield return new ContentWritten(unflushedOutput);
                }
            }
        }

        public void SetCursorPosition(int left, int top)
        {
            if (left < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(left),
                    left,
                    "The value must be greater than or equal to zero and less than the console's buffer size in that dimension.");
            }

            if (top < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(top),
                    top,
                    "The value must be greater than or equal to zero and less than the console's buffer size in that dimension.");
            }

            _cursorLeft = left;
            _cursorTop = top;

            RecordEvent(new CursorPositionChanged(new Point(_cursorLeft, _cursorTop)));
        }

        private void RecordEvent(ConsoleEvent @event)
        {
            if (@event is ContentWritten)
            {
                throw new ArgumentException($"{nameof(ContentWritten)} events should be recorded by calling {nameof(TryFlushTextWrittenEvent)}");
            }

            TryFlushTextWrittenEvent();

            if (@event is AnsiControlCodeWritten controlCodeWritten)
            {
                var escapeSequence = controlCodeWritten.Code.EscapeSequence;

                if (escapeSequence.EndsWith("H"))
                {
                    var positionFinder = new Regex(@"\x1b\[(?<line>[0-9]*);(?<column>[0-9]*)H");
                    var match = positionFinder.Match(escapeSequence);
                    var column = int.Parse(match.Groups["column"].Value);
                    var line = int.Parse(match.Groups["line"].Value);
                    RecordEvent(
                        new CursorPositionChanged(
                            new Point(column - 1, line - 1)));
                    return;
                }
            }

            _events.Add(@event);
        }

        private void TryFlushTextWrittenEvent()
        {
            var unflushedOutput = UnflushedOutput;

            if (unflushedOutput.Length > 0)
            {
                var contentWritten = new ContentWritten(unflushedOutput);
                _events.Add(contentWritten);
                _outBuffer.Clear();
            }
        }

        private string UnflushedOutput => _outBuffer.ToString();

        public bool IsOutputRedirected { get; }

        public bool IsErrorRedirected { get; }

        public bool IsInputRedirected { get; }

        public IEnumerable<TextRendered> OutputLines()
        {
            var buffer = new StringBuilder();

            var position = new Point(CursorLeft, CursorTop);

            foreach (var @event in Events)
            {
                switch (@event)
                {
                    case AnsiControlCodeWritten ansiControlCodeWritten:
                        buffer.Append(ansiControlCodeWritten.Code.EscapeSequence);
                        break;
                    case ContentWritten contentWritten:
                        buffer.Append(contentWritten.Content);
                        break;
                    case CursorPositionChanged cursorPositionChanged:

                        if (buffer.Length > 0)
                        {
                            yield return new TextRendered(buffer.ToString(), position);
                            buffer.Clear();
                        }

                        position = cursorPositionChanged.Point;

                        break;
                }
            }

            if (buffer.Length > 0)
            {
                yield return new TextRendered(buffer.ToString(), position);
            }
        }

        public abstract class ConsoleEvent
        {
        }

        public class CursorPositionChanged : ConsoleEvent
        {
            public CursorPositionChanged(Point point)
            {
                Point = point;
            }

            public Point Point { get; }
        }

        public class ContentWritten : ConsoleEvent
        {
            public ContentWritten(string text)
            {
                Content = text ?? throw new ArgumentNullException(nameof(text));
            }

            public string Content { get; }
        }

        private class RecordingWriter : TextWriter
        {
            private readonly StringBuilder _stringBuilder = new StringBuilder();

            public event Action<char> CharWritten;

            public override void Write(char value)
            {
                _stringBuilder.Append(value);
                CharWritten?.Invoke(value);
            }

            public override Encoding Encoding { get; } = Encoding.Unicode;

            public override string ToString() => _stringBuilder.ToString();
        }
    }

    public class TextRendered
    {
        public TextRendered(string text, Point position)
        {
            Text = text;
            Position = position;
        }

        public string Text { get; }

        public Point Position { get; }
    }
}
