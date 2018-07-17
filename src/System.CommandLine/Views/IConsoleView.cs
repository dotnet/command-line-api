namespace System.CommandLine.Views
{
    public interface IConsoleView
    {
        IConsoleWriter ConsoleWriter { get; }
    }
}
