using System.Linq;

namespace System.CommandLine.Rendering
{
    public abstract class ConsoleView<T> : IConsoleView<T>
    {
        protected ConsoleView(IConsoleWriter writer)
        {
            ConsoleWriter = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public abstract void Render(T value);

        public IConsoleWriter ConsoleWriter { get; }
    }
}
