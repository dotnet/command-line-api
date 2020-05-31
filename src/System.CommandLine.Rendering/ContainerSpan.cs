// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.CommandLine.Rendering
{
    public class ContainerSpan : TextSpan, IReadOnlyList<TextSpan>
    {
        private readonly List<TextSpan> _children;

        public ContainerSpan(params TextSpan[] children)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }

            _children = new List<TextSpan>(children);

            RecalculateChildPositions();
        }

        public override int ContentLength => _children.Sum(span => span.ContentLength);

        public IEnumerator<TextSpan> GetEnumerator() => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _children.Count;

        public TextSpan this[int index] => _children[index];

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

        public override void WriteTo(TextWriter writer, OutputMode outputMode)
        {
            for (var i = 0; i < _children.Count; i++)
            {
                _children[i].WriteTo(writer, outputMode);
            }
        }

        public void Add(TextSpan child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            _children.Add(child);

            RecalculateChildPositions();
        }

        public void Add(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _children.Add(new ContentSpan(text));

            RecalculateChildPositions();
        }
    }
}
