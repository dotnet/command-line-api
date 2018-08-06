using System.CommandLine.Rendering;

namespace System.CommandLine.Invocation
{
    internal class EntireConsoleRegion : Region
    {
        public static EntireConsoleRegion Instance { get; } = new EntireConsoleRegion();

        private EntireConsoleRegion() : base(0, 0, 0, 0, false)
        {
        }

        public override int Height => System.Console.WindowHeight;

        public override int Width => System.Console.WindowWidth;

        public override int Top => System.Console.WindowTop;

        public override int Left => System.Console.WindowLeft;
    }
}
