// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class VirtualTerminal : TerminalBase
    {
        private readonly VirtualTerminalMode _virtualTerminalMode;

        public VirtualTerminal(
            IConsole console,
            VirtualTerminalMode virtualTerminalMode) : base(console)
        {
            _virtualTerminalMode = virtualTerminalMode;
        }

        public override ConsoleColor BackgroundColor { get; set; }

        public override ConsoleColor ForegroundColor { get; set; }

        public override void ResetColor()
        {
            Console.Out.Write(
                Ansi.Color.Background.Default.EscapeSequence);
            Console.Out.Write(
                Ansi.Color.Foreground.Default.EscapeSequence);
        }

        public override void Clear()
        {
            Console.Out.Write(
                Ansi.Clear.EntireScreen.EscapeSequence);
        }

        public override int CursorLeft
        {
            get => System.Console.CursorLeft;
            set => Console.Out.Write(
                Ansi.Cursor.Move.ToLocation(left: value + 1).EscapeSequence);
        }

        public override int CursorTop
        {
            get => System.Console.CursorTop;
            set => Console.Out.Write(
                Ansi.Cursor.Move.ToLocation(top: value + 1).EscapeSequence);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _virtualTerminalMode?.Dispose();
            }

            base.Dispose(disposing);
        }

        public override void SetCursorPosition(int left, int top) => Console.Out.Write(
            Ansi.Cursor.Move
                .ToLocation(left: left + 1, top: top + 1)
                .EscapeSequence);

        public override void HideCursor()
        {
            Console.Out.Write(Ansi.Cursor.Hide.EscapeSequence);
        }

        public override void ShowCursor()
        {
            Console.Out.Write(Ansi.Cursor.Show.EscapeSequence);
        }
    }
}
