namespace System.CommandLine.Rendering
{
    public interface IRenderer
    {
        SpanFormatter Formatter { get; }

        void RenderToRegion(object value, Region region);

        void RenderToRegion(FormattableString value, Region region);

        void RenderToRegion(Span span, Region region);

        Size MeasureSpan(Span span, Size maxSize);
    }
}
