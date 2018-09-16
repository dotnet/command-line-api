namespace System.CommandLine.Rendering.Views
{
    public class ContentView : View
    {
        public ContentView(string content)
        {
            Span = new ContentSpan(content);
        }

        private ContentView()
        { }

        private Span Span { get; set; }

        public override void Render(Region region, IRenderer renderer)
        {
            renderer.RenderToRegion(Span, region);
        }

        public override Size GetContentSize()
        {
            return new Size(Span.ContentLength, 1);
        }

        public override Size GetAdjustedSize(IRenderer renderer, Size maxSize)
        {
            return renderer.MeasureSpan(Span, maxSize);
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

        public static ContentView FromObservable<T>(IObservable<T> observable, Func<T, FormattableString> formatProvider)
        {
            var rv = new ContentView();
            rv.Observe(observable, formatProvider);
            return rv;
        }

        private class Observer<T> : IObserver<T>
        {
            private readonly ContentView _contentView;
            private readonly Func<T, FormattableString> _formatProvider;
            private readonly SpanFormatter _spanFormatter = new SpanFormatter();


            public Observer(ContentView contentView, Func<T, FormattableString> formatProvider)
            {
                _contentView = contentView;
                _formatProvider = formatProvider;
            }

            public void OnCompleted() { }

            public void OnError(Exception error) { }

            public void OnNext(T value)
            {
                _contentView.Span = _spanFormatter.ParseToSpan(_formatProvider(value));
                _contentView.OnUpdated();
            }
        }
    }
}
