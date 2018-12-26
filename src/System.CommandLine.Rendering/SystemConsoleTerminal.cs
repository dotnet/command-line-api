// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public class SystemConsoleTerminal : TerminalBase
    {
        public SystemConsoleTerminal(IConsole console) : base(console)
        {
        }

        public override void Clear() => System.Console.Clear();

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

        public override void SetCursorPosition(int left, int top) => System.Console.SetCursorPosition(left, top);
    }
}
