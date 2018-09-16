namespace System.CommandLine.Rendering.Models
{
    public class Region
    {
        public Region(Coordinate topLeft, Size size)
        {
            if (topLeft == null)
            {
                throw new ArgumentNullException(nameof(topLeft));
            }

            if (size == null)
            {
                throw new ArgumentNullException(nameof(size));
            }

            var rightX = topLeft.X + size.Width;
            var bottomY = topLeft.Y + size.Height;

            TopLeft = topLeft;
            TopRight = new Coordinate(rightX, topLeft.Y);

            BottomLeft = new Coordinate(topLeft.X, bottomY);
            BottomRight = new Coordinate(rightX, bottomY);
        }

        public Coordinate TopLeft { get; }

        public Coordinate TopRight { get; }

        public Coordinate BottomLeft { get; }

        public Coordinate BottomRight { get; }
    }
}
