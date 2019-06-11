// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class SystemConsoleTerminal : TerminalBase
    {
        private readonly ConsoleColor _initialForegroundColor;
        private readonly ConsoleColor _initialBackgroundColor;

        public SystemConsoleTerminal(IConsole console) : base(console)
        {
            _initialForegroundColor = System.Console.ForegroundColor;
            _initialBackgroundColor = System.Console.BackgroundColor;
        }

        public override void Clear() => System.Console.Clear();

        public override ConsoleColor BackgroundColor
        {
            get => System.Console.BackgroundColor;
            set => System.Console.BackgroundColor = value;
        }

        public override ConsoleColor ForegroundColor
        {
            get => System.Console.ForegroundColor;
            set => System.Console.ForegroundColor = value;
        }

        public override int CursorLeft
        {
            get => System.Console.CursorLeft;
            set => System.Console.CursorLeft = value;
        }

        public override int CursorTop
        {
            get => System.Console.CursorTop;
            set => System.Console.CursorTop = value;
        }

        public override void ResetColor() => System.Console.ResetColor();

        private void RestoreConsoleSettings()
        {
            System.Console.ForegroundColor = _initialForegroundColor;
            System.Console.BackgroundColor = _initialBackgroundColor;
        }

        public override void SetCursorPosition(int left, int top) => System.Console.SetCursorPosition(left, top);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RestoreConsoleSettings();
            }
        }

        public override void HideCursor()
        {
            System.Console.CursorVisible = false;
        }

        public override void ShowCursor()
        {
            System.Console.CursorVisible = true;
        }
    }
}
