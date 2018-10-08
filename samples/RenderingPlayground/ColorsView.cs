using System.CommandLine.Rendering;
using System.CommandLine.Rendering.Views;

namespace RenderingPlayground
{
    internal class ColorsView : View
    {
        public ColorsView(string text)
        {
            Text = text;
        }

        private string Text { get; }

        public override Size Measure(ConsoleRenderer renderer, Size maxSize) => maxSize;

        public override void Render(ConsoleRenderer renderer, Region region)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            var i = 0;

            for (var x = 0; x < region.Width; x++)
            for (var y = 0; y < region.Height; y++)
            {
                if (i >= Text.Length - 1)
                {
                    i = 0;
                }
                else
                {
                    i++;
                }

                var subregion = new Region(
                    region.Left + x,
                    region.Top + y,
                    1,
                    1);

                unchecked
                {
                    renderer.RenderToRegion(
                        $"{ForegroundColorSpan.Rgb(r += 2, g += 3, b += 5)}{Text[i]}{ForegroundColorSpan.Reset()}",
                        subregion);
                }
            }
        }
    }
}
