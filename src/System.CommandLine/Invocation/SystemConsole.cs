// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering;
using System.IO;

namespace System.CommandLine.Invocation
{
    internal class SystemConsole : IConsole
    {
        private SystemConsole()
        {
        }

        public static IConsole Instance { get; } = new SystemConsole();

        public TextWriter Error => System.Console.Error;

        public TextWriter Out => System.Console.Out;

        public ConsoleColor ForegroundColor
        {
            get => System.Console.ForegroundColor;
            set => System.Console.ForegroundColor = value;
        }

        public void ResetColor() => System.Console.ResetColor();

        public Region GetRegion() => EntireConsoleRegion.Instance;

        public int CursorLeft
        {
            get => System.Console.CursorLeft;
            set => System.Console.CursorLeft = value;
        }

        public int CursorTop
        {
            get => System.Console.CursorTop;
            set => System.Console.CursorTop = value;
        }

        public void SetCursorPosition(int left, int top) => System.Console.SetCursorPosition(left, top);

        public bool IsOutputRedirected => System.Console.IsOutputRedirected; 

        public bool IsErrorRedirected => System.Console.IsErrorRedirected; 

        public bool IsInputRedirected => System.Console.IsInputRedirected; 
    }
}
