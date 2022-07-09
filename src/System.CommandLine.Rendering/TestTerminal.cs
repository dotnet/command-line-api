// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.IO;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace System.CommandLine.Rendering
{
    public class TestTerminal : ITerminal, IRenderable
    {
        private int _cursorLeft;
        private int _cursorTop;
        private readonly List<ConsoleEvent> _events = new();
        private readonly StringBuilder _outBuffer = new();
        private readonly StringBuilder _ansiCodeBuffer = new();
        private ConsoleColor _backgroundColor = ConsoleColor.Black;
        private ConsoleColor _foregroundColor = ConsoleColor.White;
        private readonly RecordingWriter _out = new();
        private readonly RecordingWriter _error = new();

        public TestTerminal()
        {
            _out.CharWritten += OnCharWrittenToOut;
        }

        public IStandardStreamWriter Out => _out;
        public IStandardStreamWriter Error => _error;

        public bool IsOutputRedirected { get; set; }
        public bool IsErrorRedirected { get; set; }
        public bool IsInputRedirected { get; set; }

        public OutputMode OutputMode { get; set; } = OutputMode.Auto;

        private void OnCharWrittenToOut(char c)
        {
            if (IsAnsiTerminal)
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
            else
            {
                _outBuffer.Append(c);
            }
        }

        public int Height { get; set; } = 100;

        public int Width { get; set; } = 100;

        public virtual void ResetColor()
        {
            RecordEvent(new ColorReset());
        }

        public Region GetRegion() => new(0, 0, Width, Height);

        public void Clear()
        {
            RecordEvent(new Cleared());
        }

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

        public virtual ConsoleColor BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;

                RecordEvent(new BackgroundColorChanged(value));
            }
        }

        public virtual ConsoleColor ForegroundColor
        {
            get => _foregroundColor;
            set
            {
                _foregroundColor = value;

                RecordEvent(new ForegroundColorChanged(value));
            }
        }

        public bool IsAnsiTerminal { get; set; } = true;

        public IEnumerable<TextRendered> RenderOperations()
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
                        if (position != cursorPositionChanged.Position)
                        {
                            if (buffer.Length > 0)
                            {
                                yield return new TextRendered(buffer.ToString(), position);
                                buffer.Clear();
                            }

                            position = cursorPositionChanged.Position;
                        }

                        break;
                }
            }

            if (buffer.Length > 0)
            {
                yield return new TextRendered(buffer.ToString(), position);
            }
        }

        public void HideCursor()
        {
            RecordEvent(new CursorHidden());
        }

        public void ShowCursor()
        {
            RecordEvent(new CursorShown());
        }

        public abstract class ConsoleEvent
        {
        }

        public class AnsiControlCodeWritten : ConsoleEvent
        {
            public AnsiControlCodeWritten(AnsiControlCode ansiControlCode)
            {
                Code = ansiControlCode ?? throw new ArgumentNullException(nameof(ansiControlCode));
            }

            public AnsiControlCode Code { get; }
        }

        public class BackgroundColorChanged : ConsoleEvent
        {
            public BackgroundColorChanged(ConsoleColor backgroundColor)
            {
                BackgroundColor = backgroundColor;
            }

            public ConsoleColor BackgroundColor { get; }
        }

        public class Cleared : ConsoleEvent
        {
        }

        public class ColorReset : ConsoleEvent
        {
        }

        [DebuggerDisplay(nameof(CursorPositionChanged) + ": {" + nameof(Position) + ", nq}")]
        public class CursorPositionChanged : ConsoleEvent
        {
            public CursorPositionChanged(Point position)
            {
                Position = position;
            }

            public Point Position { get; }
        }

        [DebuggerDisplay(nameof(ContentWritten) + ": {" + nameof(Content) + "}")]
        public class ContentWritten : ConsoleEvent
        {
            public ContentWritten(string text)
            {
                Content = text ?? throw new ArgumentNullException(nameof(text));
            }

            public string Content { get; }
        }

        [DebuggerDisplay(nameof(ForegroundColorChanged) + ": {" + nameof(ForegroundColor) + ", nq}")]
        public class ForegroundColorChanged : ConsoleEvent
        {
            public ForegroundColorChanged(ConsoleColor foregroundColor)
            {
                ForegroundColor = foregroundColor;
            }

            public ConsoleColor ForegroundColor { get; }
        }

        public class CursorHidden : ConsoleEvent
        {

        }

        public class CursorShown : ConsoleEvent
        {

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
