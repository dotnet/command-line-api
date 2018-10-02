using System.Diagnostics;

namespace System.CommandLine.Rendering
{
    [DebuggerDisplay("{Width}x{Height}")]
    public class Size
    {
        public const int MaxValue = -1;

        public Size(int width, int height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }
    }
}
