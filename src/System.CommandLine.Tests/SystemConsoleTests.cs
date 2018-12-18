using System.CommandLine.Invocation;

namespace System.CommandLine.Tests
{
    internal class SystemConsoleTests : ConsoleTests
    {
        protected override ITerminal GetTerminal()
        {
            return SystemConsole.Create();
        }
    }
}
