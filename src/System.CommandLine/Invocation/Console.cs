// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Rendering;
using System.IO;

namespace System.CommandLine.Invocation
{
    internal class Console : IConsole
    {
        private Console()
        {
        }

        public static IConsole Instance { get; } = new Console();

        public TextWriter Error => System.Console.Error;

        public TextWriter Out => System.Console.Out;

        public ConsoleColor ForegroundColor
        {
            get => System.Console.ForegroundColor;
            set => System.Console.ForegroundColor = value;
        }

        public int Height
        {
            get => System.Console.WindowHeight;
            set => System.Console.WindowHeight = value;
        }

        public int Width
        {
            get => System.Console.WindowWidth;
            set => System.Console.WindowWidth = value;
        }

        public void ResetColor() => System.Console.ResetColor();

        public Region GetRegion() => new Region(Height, Width, 0, 0);
    }
}
