using System.CommandLine.Rendering;

namespace System.CommandLine.Tests
{
    internal class SystemConsoleTests : ConsoleTests
    {
        protected override IConsole GetConsole()
        {
            return new ConsoleRenderer().Console;
        }
    }
}
