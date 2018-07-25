using System.Linq;

namespace System.CommandLine.Rendering
{
    public abstract class Span
    {
        public abstract int ContentLength { get; }

        public static Span Empty { get; } = new ContentSpan("");
    }
}
