// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;

namespace System.CommandLine.Rendering
{
    public abstract class Span
    {
        public static Span Empty() => new ContentSpan("");

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

        public override string ToString()
        {
            return ToString(OutputMode.PlainText);
        }

        public virtual string ToString(OutputMode outputMode)
        {
            var writer = new StringWriter();

            WriteTo(writer, outputMode);

            return writer.ToString();
        }

        public virtual void WriteTo(TextWriter writer, OutputMode outputMode)
        {
            writer.Write(ToString());
        }
    }
}