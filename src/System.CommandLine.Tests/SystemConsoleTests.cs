using System.CommandLine.Rendering;

namespace System.CommandLine.Tests
{
    internal class SystemConsoleTests : ConsoleTests
    {
        protected override ITerminal GetConsole()
        {
            return new ConsoleRenderer().Console as ITerminal;
        }
    }
}
