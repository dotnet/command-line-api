using System;
using System.Collections.Generic;
using System.Text;

namespace System.CommandLine.Rendering.Views
{
    public class ContentView<T> : ContentView
    {
        public ContentView(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public override Size Measure(IRenderer renderer, Size maxSize)
        {
            EnsureSpanCreated(renderer);
            return base.Measure(renderer, maxSize);
        }

        public override void Render(IRenderer renderer, Region region)
        {
            EnsureSpanCreated(renderer);
            base.Render(renderer, region);
        }

        private void EnsureSpanCreated(IRenderer renderer)
        {
            if (Span == null)
            {
                Span = renderer.Formatter.Format(Value);
            }
        }
    }
}
