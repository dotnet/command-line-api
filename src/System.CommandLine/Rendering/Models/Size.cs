namespace System.CommandLine.Rendering.Models
{
    public class Size
    {
        public const int MaxValue = -1;

        public Size(int width = MaxValue, int height = MaxValue)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
