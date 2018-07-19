namespace System.CommandLine.Views
{
    public interface IConsoleView<in T> : IConsoleView
    {
        void Render(T value);
    }
}