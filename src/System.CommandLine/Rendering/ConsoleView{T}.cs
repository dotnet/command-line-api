namespace System.CommandLine.Rendering
{
    public class ConsoleView<T> : IConsoleView<T>
    {
        public ConsoleView(
            ConsoleWriter writer,
            Region region = null)
        {
            ConsoleWriter = writer ?? throw new ArgumentNullException(nameof(writer));
            Region = region ?? writer.Console.GetRegion();
        }

        protected ConsoleWriter ConsoleWriter { get; }

        public Region Region { get; }

        public virtual void Render(T value)
        {
            Write(value);
        }

        public void Write(object value)
        {
            ConsoleWriter.RenderToRegion(value, Region);
        }

        public void WriteLine(object value)
        {
            Write(value);
            ConsoleWriter.Console.Out.WriteLine();
        }
    }
}
