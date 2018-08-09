using System.IO;
using static System.CommandLine.Rendering.Ansi;

namespace System.CommandLine.Rendering
{
    internal class AnsiRenderingSpanVisitor : ContentRenderingSpanVisitor
    {
        public AnsiRenderingSpanVisitor(
            TextWriter writer,
            Region region) : base(writer, region)
        {
        }

        protected override void Start(Span span)
        {
            Writer.Write(
                Cursor.Move.ToLocation(
                    line: Region.Top + 1,
                    column: Region.Left + 1));
        }

        public override void VisitFormatSpan(FormatSpan controlCode)
        {
            Writer.Write(controlCode);
        }

        protected override void StartNewLine()
        {
            Writer.Write(
                Cursor.Move.ToLocation(
                    line: Region.Top + 1 + LinesWritten,
                    column: Region.Left + 1));
        }
    }
}
