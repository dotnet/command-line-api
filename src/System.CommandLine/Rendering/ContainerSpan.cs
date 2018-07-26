using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public class ContainerSpan : Span, IReadOnlyCollection<Span>
    {
        private readonly List<Span> _children;

        public ContainerSpan(params Span[] children)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            _children = new List<Span>(children);
        }

        public override int ContentLength => _children.Sum(span => span.ContentLength);

        public IEnumerator<Span> GetEnumerator() => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _children.Count;

        public override string ToString() => string.Join("", _children);
    }
}
