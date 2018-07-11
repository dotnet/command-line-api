using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class UseVersionTests
    {
        private readonly ITestOutputHelper output;

        public UseVersionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public async Task testname()
        {
            var parser = new CommandLineBuilder()
                         .UseVersion()
                         .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--version", console);

            output.WriteLine(console.Out.ToString());

            // FIX (testname) write test
            throw new NotImplementedException();
        }
    }
}
