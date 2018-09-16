namespace System.CommandLine.Rendering.Spans
{
    public class EmptySpan : Span
    {
        public override int ContentLength { get; } = 0;

        public override string ToString() => "";
    }
}
