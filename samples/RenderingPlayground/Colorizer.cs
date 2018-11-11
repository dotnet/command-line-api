using System.CommandLine.Rendering;

namespace RenderingPlayground
{
    internal static class Colorizer
    {
        public static Span Underline(this string value) =>
            new ContainerSpan(StyleSpan.UnderlinedOn(),
                              new ContentSpan(value),
                              StyleSpan.UnderlinedOff());  
                              
                              
        public static Span Rgb(this string value, byte r, byte g, byte b) =>
            new ContainerSpan(ForegroundColorSpan.Rgb(r, g, b),
                              new ContentSpan(value),
                              ForegroundColorSpan.Reset());

        public static Span LightGreen(this string value) =>
            new ContainerSpan(ForegroundColorSpan.LightGreen(),
                              new ContentSpan(value),
                              ForegroundColorSpan.Reset());

        public static Span White(this string value) =>
            new ContainerSpan(ForegroundColorSpan.White(),
                              new ContentSpan(value),
                              ForegroundColorSpan.Reset());
    }
}
