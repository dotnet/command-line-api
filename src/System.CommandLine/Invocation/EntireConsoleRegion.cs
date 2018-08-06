using System;
using System.CommandLine.Rendering;
using static System.Console;

namespace System.CommandLine.Invocation
{
    internal class EntireConsoleRegion : Region
    {
        public static EntireConsoleRegion Instance { get; } = new EntireConsoleRegion();

        private EntireConsoleRegion() : base(WindowWidth, WindowHeight, 0, 0, false)
        {
        }

        public override int Height => WindowHeight;

        public override int Width => WindowWidth;

        public override int Top => WindowTop;

        public override int Left => WindowLeft;
    }
}
