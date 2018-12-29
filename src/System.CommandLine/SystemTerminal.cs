// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    public class SystemTerminal :
        SystemConsole,
        ITerminal,
        IDisposable
    {
        private readonly ConsoleColor _initialForegroundColor;
        private readonly ConsoleColor _initialBackgroundColor;

        public SystemTerminal()
        {
            _initialForegroundColor = Console.ForegroundColor;
            _initialBackgroundColor = Console.BackgroundColor;
        }

        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }

        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public void ResetColor() => Console.ResetColor();

        private void RestoreConsoleSettings()
        {
            Console.ForegroundColor = _initialForegroundColor;
            Console.BackgroundColor = _initialBackgroundColor;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RestoreConsoleSettings();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
