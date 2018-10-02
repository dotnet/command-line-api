using System.CommandLine.Rendering;

namespace RenderingPlayground
{
    internal static class Colorizer
    {
        public static Span Underline(this string value) =>
            new ContainerSpan(StyleSpan.UnderlinedOn,
                              new ContentSpan(value),
                              StyleSpan.UnderlinedOff);
    }
}
