// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.CommandLine.Rendering.Views
{
    public class ContentView : View
    {
        public ContentView(string content) 
            : this (new ContentSpan(content))
        { }

        public ContentView(TextSpan span)
        {
            Span = span ?? throw new ArgumentNullException(nameof(span));
        }

        protected ContentView()
        { }

        protected TextSpan Span { get; set; }

        public override void Render(ConsoleRenderer renderer, Region region)
        {
            if (renderer == null) throw new ArgumentNullException(nameof(renderer));
            if (region == null) throw new ArgumentNullException(nameof(region));

            renderer.RenderToRegion(Span, region);
        }

        public override Size Measure(ConsoleRenderer renderer, Size maxSize)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (maxSize == null)
            {
                throw new ArgumentNullException(nameof(maxSize));
            }

            if (Span == null)
            {
                return new Size(0, 0);
            }

            return ConsoleRenderer.MeasureSpan(Span, maxSize);
        }

        protected void Observe<T>(IObservable<T> observable, Func<T, FormattableString> formatProvider)
        {
            if (observable == null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (formatProvider == null)
            {
                throw new ArgumentNullException(nameof(formatProvider));
            }

            observable.Subscribe(new Observer<T>(this, formatProvider));
        }

        public static ContentView FromObservable<T>(IObservable<T> observable, Func<T, FormattableString> formatProvider = null)
        {
            var rv = new ContentView();
            rv.Observe(observable, formatProvider ?? (x => $"{x}"));
            return rv;
        }

        internal static ContentView Create(object content, TextSpanFormatter formatter)
        {
            if (content == null) return new ContentView(TextSpan.Empty());
            return CreateView((dynamic)content, formatter);
        }

        private static ContentView CreateView(string stringContent, TextSpanFormatter _)
            => new(stringContent);

        private static ContentView CreateView(TextSpan span, TextSpanFormatter _) 
            => new(span);

        private static ContentView CreateView<T>(IObservable<T> observable, TextSpanFormatter _)
            => FromObservable(observable);

        private static ContentView CreateView(object value, TextSpanFormatter formatter)
            => new(formatter.Format(value));

        private class Observer<T> : IObserver<T>
        {
            private readonly ContentView _contentView;
            private readonly Func<T, FormattableString> _formatProvider;
            private readonly TextSpanFormatter _textSpanFormatter = new();

            public Observer(ContentView contentView, Func<T, FormattableString> formatProvider)
            {
                _contentView = contentView;
                _formatProvider = formatProvider;
            }

            public void OnCompleted() { }

            public void OnError(Exception error) { }

            public void OnNext(T value)
            {
                _contentView.Span = _textSpanFormatter.ParseToSpan(_formatProvider(value));
                _contentView.OnUpdated();
            }
        }
    }
}
