using System.CommandLine.Rendering;

namespace System.CommandLine.Tests.Rendering
{
    public class AnonymousView<T> : ConsoleView<T>
    {
        private readonly Action<T, IConsoleWriter> render;

        public AnonymousView(
            IConsoleWriter writer,
            Action<T, IConsoleWriter> render) : base(writer)
        {
            this.render = render ?? throw new ArgumentNullException(nameof(render));
        }

        public override void Render(T value) => render(value, ConsoleWriter);
    }
}