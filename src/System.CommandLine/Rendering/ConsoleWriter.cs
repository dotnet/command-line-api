using System.Collections.Generic;

namespace System.CommandLine.Rendering
{
    public enum OutputMode
    {
        NonAnsi,
        Ansi,
        File
    }

    public class ConsoleWriter :
        ICustomFormatter,
        IFormatProvider
    {
        private readonly Dictionary<Type, Func<object, Span>> _formatters = new Dictionary<Type, Func<object, Span>>();

        public ConsoleWriter(
            IConsole console,
            OutputMode mode = OutputMode.NonAnsi)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
            Mode = mode;
        }

        public IConsole Console { get; }

        public OutputMode Mode { get; }

        public void RenderToRegion(
            object value,
            Region region)
        {
            var formatted = Format(value);

            RenderToRegion(formatted, region);
        }

        public void RenderToRegion(
            FormattableString value,
            Region region)
        {
            var formatted = Format(value);

            RenderToRegion(formatted, region);
        }

        public void RenderToRegion(
            Span span,
            Region region)
        {
            var visitor = new ContentRenderingSpanVisitor(this, region);

            visitor.Visit(span);
        }

        public virtual void WriteRawToRegion(
            string raw,
            Region region)
        {
            Console.Out.Write(raw);
        }

        string ICustomFormatter.Format(
            string format,
            object arg,
            IFormatProvider formatProvider)
        {
            if (arg == null)
            {
                return "";
            }

            return Format(arg).ToString();
        }

        object IFormatProvider.GetFormat(Type formatType) => this;

        public void AddFormatter<T>(Func<T, string> format)
        {
            _formatters.Add(typeof(T),
                            t => {
                                var formatted = format((T)t);

                                if (formatted == null)
                                {
                                    return Span.Empty;
                                }

                                return new ContentSpan(formatted);
                            });
        }

        public void AddFormatter<T>(Func<T, Span> format)
        {
            _formatters.Add(typeof(T),
                            t => format((T)t));
        }

        public Span Format(object value)
        {
            if (_formatters.TryGetValue(value.GetType(), out var formatter))
            {
                return formatter(value);
            }
            else if (value is FormattableString formattable)
            {
                var formatted = ((IFormattable)formattable).ToString("", this);

                return new ContentSpan(formatted);
            }
            else
            {
                return new ContentSpan(value.ToString());
            }
        }
    }
}
