using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.DragonFruit.Tests
{
    public class CommandLineTests
    {
        private readonly ITestOutputHelper _output;
        private readonly TestConsole _console;

        public CommandLineTests(ITestOutputHelper output)
        {
            _output = output;
            _console = new TestConsole(_output);
        }

        private string _captured;

        void TestMain(string name)
        {
            _captured = name;
        }

        [Fact]
        public async Task It_executes_method_with_string_option()
        {
            var context = new InvocationContext<int>(((Action<string>)TestMain).Method, _console) {
                Object = this
            };
            var exitCode = await CommandLine.ExecuteMethodAsync(
                new[] {"--name", "Wayne"},
                context,
                "test1",
                methodDescription: null);
            exitCode.Should().Be(0);
            _captured.Should().Be("Wayne");
        }

        [Fact]
        public async Task It_shows_help_text()
        {
            var context = new InvocationContext<int>(((Action<string>)TestMain).Method, _console) {
                Object = this
            };
            var stdout = new StringBuilder();
            _console.Out = new StringWriter(stdout);
            var exitCode = await CommandLine.ExecuteMethodAsync(
                new[] {"--help"},
                context,
                "test1",
                methodDescription: null);

            exitCode.Should().Be(CommandLine.HelpExitCode);
            stdout.ToString().Should()
                .Contain("--name")
                .And.Contain("Options:");
        }

        void TestMainWithDefault(string name = "Bruce")
        {
            _captured = name;
        }

        [Fact]
        public async Task It_executes_method_with_string_option_with_default()
        {
            var context = new InvocationContext<int>(((Action<string>)TestMainWithDefault).Method, _console) {
                Object = this
            };

            var exitCode = await CommandLine.ExecuteMethodAsync(
                new[] {"--name", "Wayne"},
                context,
                "test1",
                methodDescription: null);
            exitCode.Should().Be(0);
            _captured.Should().Be("Wayne");

            exitCode = await CommandLine.ExecuteMethodAsync(
                Array.Empty<string>(),
                context,
                "test1",
                methodDescription: null);
            exitCode.Should().Be(0);
            _captured.Should().Be("Bruce");
        }

        void TestMainThatThrows()
        {
            throw new InvalidOperationException("This should not be thrown");
        }

        [Fact]
        public async Task It_shows_error_without_invoking_method()
        {
            var context = new InvocationContext<int>(((Action)TestMainThatThrows).Method, _console) {
                Object = this
            };

            var stderr = new StringBuilder();
            _console.Error = new StringWriter(stderr);

            var exitCode =
                await CommandLine.ExecuteMethodAsync(new[] {"--unknown"}, context, "test1", methodDescription: null);
            exitCode.Should().Be(CommandLine.ErrorExitCode);
            stderr.ToString()
                .Should().NotBeEmpty()
                .And
                .Contain("--unknown");
            _console.ForegroundColor.Should().Be(ConsoleColor.Red);
        }
    }
}
