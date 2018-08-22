using System.IO;

namespace System.CommandLine.Rendering
{
    internal class FileRenderingSpanVisitor : ContentRenderingSpanVisitor
    {
        public FileRenderingSpanVisitor(
            TextWriter writer,
            Region region) : base(writer, region)
        {
        }

        protected override void SetCursorPosition(int left, int top)
        {
            Writer.WriteLine();

            Writer.Write(new string(' ', left));
        }
    }
}
