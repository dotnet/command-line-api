using System.CommandLine.Rendering.Models;
using System.CommandLine.Rendering.Spans;

namespace System.CommandLine.Rendering.Views
{
    public class ContentView : View
    {
        public ContentView(string content)
        {
            Span = new ContentSpan(content);
        }

        private Span Span { get; }

        public override void Render(Region region, IRenderer renderer)
        {
            renderer.RenderToRegion(Span, region);
        }

        public override Size GetContentSize()
        {
            return new Size(Span.ContentLength, 1);
        }

        public override Size GetAdjustedSize(IRenderer renderer, Size maxSize)
        {
            return renderer.MeasureSpan(Span, maxSize);
        }
    }
}
