namespace System.CommandLine.Views
{
    public abstract class ConsoleView<T> : IConsoleView
    {
        protected ConsoleView(IConsoleWriter writer)
        {
            ConsoleWriter = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public abstract void Render(T value);

        protected void Render(FormattableString formattable)
        {
            ConsoleWriter.WriteLine(formattable);
        }

        public string Column { get; } = "  ";

        public IConsoleWriter ConsoleWriter { get; }
    }
}
