using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public class ContainerSpan : Span, IReadOnlyCollection<Span>
    {
        private readonly ReadOnlyCollection<Span> _children;

        public ContainerSpan(params Span[] children)
        {
            _children = new ReadOnlyCollection<Span>(children);
        }

        public override int ContentLength => _children.Sum(span => span.ContentLength);

        public IEnumerator<Span> GetEnumerator() => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _children.Count;
    }
}