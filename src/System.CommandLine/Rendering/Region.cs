namespace System.CommandLine.Rendering
{
    public class Region
    {
        public Region(
            int left,
            int top,
            int width,
            int height,
            bool isOverwrittenOnRender = true)
        {
            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (width < 0)
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

            IsOverwrittenOnRender = isOverwrittenOnRender;
        }

        public Region(int left, int top, Size size)
            : this(left, top, size.Width, size.Height)
        {
        }

        public virtual int Height { get; }

        public virtual int Width { get; }

        public virtual int Top { get; }

        public virtual int Left { get; }

        public int Bottom => Top + Height - 1;

        public int Right => Left + Width - 1;

        public bool IsOverwrittenOnRender { get; }

        public override string ToString() => $" {Width}w × {Height}h @ {Left}x, {Top}y";
    }
}
