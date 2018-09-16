using System.CommandLine.Rendering.Models;

namespace System.CommandLine.Rendering.Spans
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
