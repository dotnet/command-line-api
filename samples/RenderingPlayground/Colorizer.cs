using System;
using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Spans;

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
