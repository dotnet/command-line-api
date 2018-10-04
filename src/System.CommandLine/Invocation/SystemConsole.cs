// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering;
using System.IO;

namespace System.CommandLine.Invocation
{
    internal class SystemConsole : IConsole
    {
        private VirtualTerminalMode _virtualTerminalMode;

        private SystemConsole()
        {
        }

        public static IConsole Instance { get; } = new SystemConsole();

        public void SetOut(TextWriter writer) => Console.SetOut(writer);

        public TextWriter Error => Console.Error;

        public TextWriter Out => Console.Out;

        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public void ResetColor() => Console.ResetColor();

        public Region GetRegion() => 
            IsOutputRedirected 
                ? new Region(0, 0, int.MaxValue, int.MaxValue, false)
                : EntireConsoleRegion.Instance;

        public int CursorLeft
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public int CursorTop
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

        public bool IsOutputRedirected => Console.IsOutputRedirected;

        public bool IsErrorRedirected => Console.IsErrorRedirected;

        public bool IsInputRedirected => Console.IsInputRedirected;

        public bool IsVirtualTerminal
        {
            get
            {
                if (_virtualTerminalMode != null)
                {
                    return _virtualTerminalMode.IsEnabled;
            }

            var terminalName = Environment.GetEnvironmentVariable("TERM");

                return !string.IsNullOrEmpty(terminalName)
                       && terminalName.StartsWith("xterm", StringComparison.OrdinalIgnoreCase);
            }
        }

        public void TryEnableVirtualTerminal()
        {
            if (IsOutputRedirected)
            {
                return;
            }

            _virtualTerminalMode = VirtualTerminalMode.TryEnable();
        }

        public void Dispose()
        {
            _virtualTerminalMode?.Dispose();
        }
    }
}
