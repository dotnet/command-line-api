namespace System.CommandLine.Rendering
{
    public abstract class ColorSpan : FormatSpan
    {
        protected ColorSpan(string name) : base(name)
        {
        }

        protected ColorSpan(RgbColor rgbColor) :
            base(rgbColor?.ToString() ?? throw new ArgumentNullException(nameof(rgbColor)))
        {
            RgbColor = rgbColor;
        }

        public RgbColor RgbColor { get; }
    }
}
