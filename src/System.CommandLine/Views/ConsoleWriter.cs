using System.Collections.Generic;

namespace System.CommandLine.Views
{
    public class ConsoleWriter :
        IConsoleWriter,
        ICustomFormatter
    {
        private readonly Dictionary<Type, Func<object, string>> _formatters = new Dictionary<Type, Func<object, string>>();

        public ConsoleWriter(IConsole console)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public IConsole Console { get; }

        public void Write(object value)
        {
            Console.Out.Write(Format($"{value}"));
        }

        public void WriteLine(object value)
        {
            Write(value);
            Console.Out.WriteLine();
        }

        protected virtual string Format(FormattableString value)
        {
            var formatProvider = (IFormatProvider)this;

            return ((IFormattable)value).ToString("", formatProvider);
        }

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

            if (_formatters.TryGetValue(arg.GetType(), out var formatter))
            {
                return formatter(arg);
            }

            return arg.ToString();
        }

        object IFormatProvider.GetFormat(Type formatType) => this;

        public void AddFormatter<T>(Func<T, string> format)
        {
            _formatters.Add(typeof(T),
                            t => format((T)t));
        }
    }
}
