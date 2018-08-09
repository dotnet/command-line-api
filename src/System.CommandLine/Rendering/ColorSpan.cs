namespace System.CommandLine.Rendering
{
    public abstract class ColorSpan : FormatSpan
    {
        protected ColorSpan(string name) : base(name)
        {
        }

        protected ColorSpan(RgbColor rgbColor) :
            base(GetName(rgbColor) ?? throw new ArgumentNullException(nameof(rgbColor)))
        {
            RgbColor = rgbColor;
        }

        public RgbColor RgbColor { get; }

        protected internal static string GetName(RgbColor rgbColor)
        {
            return rgbColor?.ToString();
        }
    }
}
