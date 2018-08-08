// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.Drawing;
using System.IO;

namespace System.CommandLine.Tests
{
    public class TestConsole : IConsole
    {
        private int _cursorLeft;
        private int _cursorTop;

        public TestConsole()
        {
            Error = new StringWriter();
            Out = new StringWriter();
        }

        public TextWriter Error { get; }

        public TextWriter Out { get; }

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
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "left",
                        value,
                        "The value must be greater than or equal to zero and less than the console's buffer size in that dimension.");
                }

                _cursorLeft = value;
            }
        }

        public int CursorTop
        {
            get => _cursorTop;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "top",
                        value,
                        "The value must be greater than or equal to zero and less than the console's buffer size in that dimension.");
                }

                _cursorTop = value;
            }
        }

        public List<Point> CursorMovements { get; } = new List<Point>();

        public void SetCursorPosition(int left, int top) => CursorMovements.Add(new Point(left, top));

        public bool IsOutputRedirected { get; }

        public bool IsErrorRedirected { get; }

        public bool IsInputRedirected { get; }
    }
}
