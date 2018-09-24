
namespace System.CommandLine.Rendering
{
    public abstract class Span
    {
        public static Span Empty => new ContentSpan("");

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
