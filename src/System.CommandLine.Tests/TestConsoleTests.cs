namespace System.CommandLine.Tests
{
    public class TestConsoleTests : ConsoleTests
    {
        protected override IConsole GetConsole() => new TestConsole();
    }
}