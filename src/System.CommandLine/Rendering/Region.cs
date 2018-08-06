namespace System.CommandLine.Rendering
{
    public class Region
    {
        protected Region()
        {
        }

        public Region(
            int width,
            int height,
            int left,
            int top,
            bool isOverwrittenOnRender = true)
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

            IsOverwrittenOnRender = isOverwrittenOnRender;
        }

        public virtual int Height { get; }

        public virtual int Width { get; }

        public virtual int Top { get; }

        public virtual int Left { get; }

        public bool IsOverwrittenOnRender { get; }

        public override string ToString() => $"{Height}h × {Width}w @ top {Top}, left {Left}";
    }
}
