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

        protected override void SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }
    }
}
