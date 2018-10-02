using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.CommandLine.Rendering.Views
{
    public class ContentView : View
    {
        public ContentView(string content)
        {
            Span = new ContentSpan(content);
        }

        public ContentView(Span span)
        {
            Span = span ?? throw new ArgumentNullException(nameof(span));
        }

        protected ContentView()
        { }

        protected Span Span { get; set; }

        public override void Render(IRenderer renderer, Region region)
        {
            if (Span == null) return;

            renderer.RenderToRegion(Span, region);
        }

        public override Size Measure(IRenderer renderer, Size maxSize)
        {
            if (Span == null) return new Size(0, 0);
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

        public static ContentView FromObservable<T>(IObservable<T> observable, Func<T, FormattableString> formatProvider = null)
        {
            var rv = new ContentView();
            rv.Observe(observable, formatProvider ?? (x => $"{x}"));
            return rv;
        }

        internal static ContentView FromObservable(Type observableType, object observable)
        {
            //TODO: cache this?
            MethodInfo fromObservableMethod =
                typeof(ContentView)
                    .GetMethods(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static)
                    .SingleOrDefault(method => method.Name == nameof(FromObservable) && method.IsGenericMethod)
                ?? throw new InvalidOperationException(
                    $"Could not find {nameof(ContentView)}.{nameof(FromObservable)}()");

            var parameter = Expression.Parameter(typeof(object));

            var parameterAsObservable = Expression.Convert(parameter, typeof(IObservable<>).MakeGenericType(observableType));
            MethodInfo genericMethod = fromObservableMethod.MakeGenericMethod(observableType);
            Type formatProviderType = typeof(Func<,>).MakeGenericType(observableType, typeof(FormattableString));
            var invokeMethod = Expression.Call(genericMethod, parameterAsObservable, Expression.Constant(null, formatProviderType));

            return Expression.Lambda<Func<object, ContentView>>(invokeMethod, parameter).Compile()(observable);
        }

        internal static ContentView Create(object content, SpanFormatter formatter)
        {
            switch (content)
            {
                case null: return new ContentView { Span = Span.Empty() };
                case string stringContent: return new ContentView(stringContent);
                case Span span: return new ContentView(span);
                default:
                    {
                        if (GetObservableType(content.GetType()) is Type observableType)
                        {
                            return FromObservable(observableType, content);
                        }
                        return new ContentView(formatter.Format(content));
                    }
            }
        }

        private static Type GetObservableType(Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IObservable<>))
                {
                    return interfaceType.GetGenericArguments()[0];
                }
            }

            return null;
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
