namespace System.CommandLine.Rendering
{
    internal class NonAnsiRenderingSpanVisitor : ContentRenderingSpanVisitor
    {
        public IConsole Console { get; }

        public NonAnsiRenderingSpanVisitor(
            IConsole console,
            Region region) : base(console?.Out, region)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
        }

        protected override void Start(Span span)
        {
            Console.SetCursorPosition(Region.Left, Region.Top);
        }

        protected override void StartNewLine()
        {
            if (Region.Left == 0)
            {
                // FIX: (StartNewLine) remove branching, always use cursor positioning
                Writer.WriteLine();
            }
            else
            {
                Console.SetCursorPosition(Region.Left, Region.Top + LinesWritten);
            }
        }
    }
}
