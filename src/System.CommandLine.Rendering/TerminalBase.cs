// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering
{
    public abstract class TerminalBase :
        ITerminal,
        IDisposable
    {
        protected TerminalBase(IConsole console)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public abstract void Clear();

        public abstract int CursorLeft { get; set; }

        public abstract int CursorTop { get; set; }

        public IConsole Console { get; }

        public abstract void SetCursorPosition(int left, int top);

        public OutputMode OutputMode { get; set; } = OutputMode.Auto;

        public abstract ConsoleColor BackgroundColor { get; set; }

        public abstract ConsoleColor ForegroundColor { get; set; }

        public abstract void ResetColor();

        public IStandardStreamWriter Out => Console.Out;

        public IStandardStreamWriter Error => Console.Error;

        public bool IsOutputRedirected => Console.IsOutputRedirected;

        public bool IsErrorRedirected => Console.IsErrorRedirected;

        public bool IsInputRedirected => Console.IsInputRedirected;

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
