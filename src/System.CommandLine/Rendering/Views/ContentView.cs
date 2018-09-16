using System.CommandLine.Rendering.Spans;

namespace System.CommandLine.Rendering.Views
{
    public abstract class ContentView : View
    {
        public abstract Span Span { get; set; }

        public override void Render(Region region, IRenderer renderer)
        {
            renderer.RenderToRegion(Span, region);
        }
    }
}
