using System.Collections.Generic;

namespace System.CommandLine.Views
{
    public class ConsoleWriter :
        IConsoleWriter,
        ICustomFormatter
    {
        private readonly Dictionary<Type, Action<object, IConsoleWriter>> _formatters = new Dictionary<Type, Action<object, IConsoleWriter>>();

        public ConsoleWriter(IConsole console)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public IConsole Console { get; }

        public void Write(object value)
        {
            Format($"{value}");
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
                formatter(arg, this);
            }
            else
            {
                switch (arg)
                {
                    case IFormattable formattable:
                        Console.Out.Write(formattable.ToString(format, null));
                        break;

                    default:
                        Console.Out.Write(arg);
                        break;
                }
            }

            return null;
        }

        object IFormatProvider.GetFormat(Type formatType) => this;

        public void AddFormatter<T>(Action<T, IConsoleWriter> format)
        {
            _formatters.Add(typeof(T),
                            (t, _) => format((T)t, this));
        }
    }
}
