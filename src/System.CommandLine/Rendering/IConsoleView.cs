namespace System.CommandLine.Rendering
{
    public interface IConsoleView
    {
        IConsoleWriter ConsoleWriter { get; }
    }
}
