
namespace System.CommandLine.Rendering.Spans
{
    public abstract class Span
    {
        private Span _root;

        public abstract int ContentLength { get; }

        public Span Root => _root ?? (_root = this);

        public ContainerSpan Parent { get; private set; }

        public int Start { get; private set; }

        public int End => Start + ContentLength;

        internal virtual void RecalculatePositions(ContainerSpan parent, int start)
        {
            Parent = parent;
            _root = parent.Root;
            Start = start;
        }
    }
}
