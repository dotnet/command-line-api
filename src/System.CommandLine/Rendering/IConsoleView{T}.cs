namespace System.CommandLine.Rendering
{
    public interface IConsoleView<in T> : IConsoleView
    {
        void Render(T value);
    }
}
