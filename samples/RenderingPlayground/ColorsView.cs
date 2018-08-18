using System.CommandLine.Rendering;

namespace RenderingPlayground
{
    internal class ColorsView : ConsoleView<string>
    {
        public ColorsView(ConsoleRenderer renderer, Region region = null) : base(renderer, region)
        {
        }

        public override void Render(string text)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            var i = 0;

            for (var x = 0; x < Region.Width; x++)
            for (var y = 0; y < Region.Height; y++)
            {
                if (i >= text.Length - 1)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }

                var subregion = new Region(
                    Region.Left + x,
                    Region.Top + y,
                    1,
                    1);

                ConsoleRenderer.RenderToRegion(
                    $"{ForegroundColorSpan.Rgb(r += 2, g += 3, b += 5)}{text[i]}{ForegroundColorSpan.Reset}",
                    subregion);
            }
        }
    }
}
