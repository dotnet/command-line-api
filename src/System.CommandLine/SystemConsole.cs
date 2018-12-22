// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine
{
    internal interface ISystemConsoleColorShim
    {
        ConsoleColor BackgroundColor { get; set; }

        ConsoleColor ForegroundColor { get; set; }

        void ResetColor();
    }

    internal class SystemTerminalShim : SystemConsole, ISystemConsoleColorShim, IDisposable
    {
        private readonly ConsoleColor _initialForegroundColor;
        private readonly ConsoleColor _initialBackgroundColor;

        internal SystemTerminalShim()
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

        private void ResetConsole()
        {
            Console.ForegroundColor = _initialForegroundColor;
            Console.BackgroundColor = _initialBackgroundColor;
        }

        protected virtual void Dispose(bool disposing)
        {
            ResetConsole();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SystemTerminalShim()
        {
            Dispose(false);
        }
    }

    internal class SystemConsole : IConsole
    {
        public SystemConsole()
        {
            Error = StandardStreamWriter.Create(Console.Error);
            Out = StandardStreamWriter.Create(Console.Out);
        }

        public IStandardStreamWriter Error { get; }

        public bool IsErrorRedirected => Console.IsErrorRedirected;

        public IStandardStreamWriter Out { get; }

        public bool IsOutputRedirected => Console.IsOutputRedirected;

        public bool IsInputRedirected => Console.IsInputRedirected;

        public static IConsole Create()
        {
            if (Console.IsOutputRedirected)
            {
                return new SystemConsole();
            }
            else
            {
                return new SystemTerminalShim();
            }
        }
    }
}
