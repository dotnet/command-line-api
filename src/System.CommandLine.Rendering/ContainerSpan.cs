// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public class ContainerSpan : Span, IReadOnlyList<Span>
    {
        private readonly List<Span> _children;

        public ContainerSpan(params Span[] children)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            _children = new List<Span>(children);

            RecalculateChildPositions();
        }

        public override int ContentLength => _children.Sum(span => span.ContentLength);

        public IEnumerator<Span> GetEnumerator() => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _children.Count;

        public Span this[int index] => _children[index];

        internal override void RecalculatePositions(ContainerSpan parent, int start)
        {
            base.RecalculatePositions(parent, start);

            RecalculateChildPositions();
        }

        private void RecalculateChildPositions()
        {
            var start = Start;

            for (var i = 0; i < _children.Count; i++)
            {
                var span = _children[i];
                span.RecalculatePositions(this, start);
                start += span.ContentLength;
            }
        }

        public override string ToString() => string.Join("", _children);
    }
}
