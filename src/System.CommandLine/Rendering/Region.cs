namespace System.CommandLine.Rendering
{
    public class Region
    {
        public Region(int height, int width, int top, int left)
        {
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (top < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(top));
            }

            if (left < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(left));
            }

            Height = height;
            Width = width;
            Top = top;
            Left = left;
        }

        public int Height { get; }

        public int Width { get; }

        public int Top { get; }

        public int Left { get; }

        public override string ToString() => $"{Height}h × {Width}w @ top {Top}, left {Left}";
    }
}
