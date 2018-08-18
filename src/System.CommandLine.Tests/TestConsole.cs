// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace System.CommandLine.Tests
{
    public class TestConsole : IConsole
    {
        private int _cursorLeft;
        private int _cursorTop;
        private readonly RecordingWriter _error;
        private readonly RecordingWriter _out;
        private readonly List<ConsoleEvent> _events = new List<ConsoleEvent>();
        private readonly StringBuilder _outCharsWritten = new StringBuilder();

        public TestConsole()
        {
            _out = new RecordingWriter();

            _out.CharWritten += value => _outCharsWritten.Append(value);

            _error = new RecordingWriter();
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

                if (_outCharsWritten.Length > 0)
                {
                    yield return new ContentWritten(_outCharsWritten.ToString());
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

            _events.Add(@event);
        }

        private void TryFlushTextWrittenEvent()
        {
            if (_outCharsWritten.Length > 0)
            {
                _events.Add(new ContentWritten(_outCharsWritten.ToString()));
                _outCharsWritten.Clear();
            }
        }

        public bool IsOutputRedirected { get; }

        public bool IsErrorRedirected { get; }

        public bool IsInputRedirected { get; }

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
                Text = text;
            }

            public string Text { get; }
        }

        private class RecordingWriter : TextWriter
        {
            private readonly StringBuilder _stringBuilder = new StringBuilder();

            public event Action<Char> CharWritten;

            public override void Write(char value)
            {
                _stringBuilder.Append(value);
                CharWritten?.Invoke(value);
            }

            public override Encoding Encoding { get; } = Encoding.Unicode;

            public override string ToString() => _stringBuilder.ToString();
        }
    }
}
