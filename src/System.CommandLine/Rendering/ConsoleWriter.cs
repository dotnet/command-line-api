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

        public virtual void WriteToRegion(
            string formatted,
            Region viewport)
        {
            var wrapped = string.Join(NewLine,
                                      formatted.Wrap(
                                          viewport.Width,
                                          viewport.Height));

            Console.Out.Write(wrapped);
        }

        protected virtual string Format(FormattableString value) =>
            ((IFormattable)value).ToString("", this);

        public void WriteLine() => Console.Out.WriteLine();

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
                            t => format((T)t));
        }

        public string Format(object value)
        {
            if (_formatters.TryGetValue(value.GetType(), out var formatter))
            {
                return formatter(value);
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
