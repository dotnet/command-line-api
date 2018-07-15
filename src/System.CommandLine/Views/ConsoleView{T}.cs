namespace System.CommandLine.Views
{
    public abstract class ConsoleView<T> :
        ICustomFormatter,
        IFormatProvider,
        IConsoleView
    {
        protected ConsoleView(IConsole console)
        {
            Console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public abstract void Render(T value);

        protected void Render(FormattableString formattable)
        {
            System.Console.Out.WriteLine(formattable.ToString(this));
        }

        public string Column { get; } = "  ";

        public IConsole Console { get; }

        public virtual string Format(
            string format,
            object arg,
            IFormatProvider formatProvider)
        {
            if (arg == null)
            {
                return "";
            }

            // TODO: (Format) extensibility by type
            switch (arg)
            {
                case string s:
                    return s;

                default:
                    break;
            }

            return format;
        }

        public object GetFormat(Type formatType)
        {
            return this;
        }
    }
}
