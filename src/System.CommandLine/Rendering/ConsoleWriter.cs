using System.Collections.Generic;
using static System.Environment;

namespace System.CommandLine.Rendering
{
    public class ConsoleWriter :
        ICustomFormatter,
        IFormatProvider
    {
        private readonly Dictionary<Type, Func<object, string>> _formatters = new Dictionary<Type, Func<object, string>>();

        public ConsoleWriter(IConsole console)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public IConsole Console { get; }

        public void FormatAndWriteToRegion(object value, Region region)
        {
            var formatted = Format(value);

            WriteRawToRegion(formatted, region);
        }

        public void FormatAndWriteToRegion(FormattableString value, Region region)
        {
            var formatted = Format(value);

            WriteRawToRegion(formatted, region);
        }

        public virtual void WriteRawToRegion(
            string raw,
            Region region)
        {
            var wrapped = string.Join(NewLine,
                                      raw.Wrap(
                                          region.Width,
                                          region.Height));

            Console.Out.Write(wrapped);
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

            return Format(arg);
        }

        object IFormatProvider.GetFormat(Type formatType) => this;

        public void AddFormatter<T>(Func<T, string> format)
        {
            _formatters.Add(typeof(T),
                            t => format((T)t) ?? "");
        }

        public string Format(object value)
        {
            if (_formatters.TryGetValue(value.GetType(), out var formatter))
            {
                return formatter(value);
            }
            else if (value is FormattableString formattable)
            {
                return ((IFormattable)formattable).ToString("", this);
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
