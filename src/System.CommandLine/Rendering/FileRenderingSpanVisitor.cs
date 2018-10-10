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
            if (top > 0 && left == 0)
            {
                Writer.WriteLine();
            }
        }

        protected override void TryClearRemainingWidth()
        {
            ClearRemainingWidth();
        }
    }
}
