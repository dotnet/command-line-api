using static System.CommandLine.Rendering.Ansi;

namespace System.CommandLine.Rendering
{
    internal class AnsiRenderingSpanVisitor : RenderingSpanVisitor
    {
        public AnsiRenderingSpanVisitor(
            ConsoleWriter consoleWriter,
            Region region) : base(consoleWriter, region)
        {
        }

        protected override void Start(Span span)
        {
            ConsoleWriter.Console.Out.Write(
                Cursor.Move.ToLocation(
                    line: Region.Top + 1,
                    column: Region.Left + 1));
        }

        public override void VisitAnsiControlCode(AnsiControlCode controlCode)
        {
            ConsoleWriter.Console.Out.Write(controlCode);
        }

        protected override void StartNewLine()
        {
            ConsoleWriter.Console.Out.Write(
                Cursor.Move.ToLocation(
                    line: Region.Top + 1 + LinesWritten,
                    column: Region.Left + 1));
        }
    }
}
