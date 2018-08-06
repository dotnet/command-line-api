using System;
using System.CommandLine.Rendering;

namespace RenderingPlayground
{
    internal static class Colorizer
    {
        public static Span Underline(this string value) =>
            new ContainerSpan(Ansi.Text.UnderlinedOn,
                              new ContentSpan(value),
                              Ansi.Text.UnderlinedOff);
    }
}
