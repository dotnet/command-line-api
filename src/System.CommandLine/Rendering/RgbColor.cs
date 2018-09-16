namespace System.CommandLine.Rendering
{
    public class RgbColor
    {
        public RgbColor(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        public byte Red { get; }
        public byte Green { get; }
        public byte Blue { get; }
    }
}
