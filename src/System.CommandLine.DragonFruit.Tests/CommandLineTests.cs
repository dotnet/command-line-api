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
            Action<string> action = TestMain;
            var exitCode = await CommandLine.InvokeMethodAsync(
                new[] {"--name", "Wayne"},
                _console,
                this,
                action.Method,
                new CommandHelpMetadata());
            exitCode.Should().Be(0);
            _captured.Should().Be("Wayne");
        }

        [Fact]
        public async Task It_shows_help_text()
        {
            var stdout = new StringBuilder();
            _console.Out = new StringWriter(stdout);
            Action<string> action = TestMain;

            var exitCode = await CommandLine.InvokeMethodAsync(
                new[] {"--help"},
                _console,
                this,
                action.Method,
                new CommandHelpMetadata());

            exitCode.Should().Be(CommandLine.OkExitCode);
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
            Action<string> action = TestMainWithDefault;

            var exitCode = await CommandLine.InvokeMethodAsync(
                new[] {"--name", "Wayne"},
                _console,
                this,
                action.Method,
                new CommandHelpMetadata());

            exitCode.Should().Be(0);
            _captured.Should().Be("Wayne");

            exitCode = await CommandLine.InvokeMethodAsync(
                Array.Empty<string>(),
                _console,
                this,
                action.Method,
                new CommandHelpMetadata());

            exitCode.Should().Be(0);
            _captured.Should().Be("Bruce");
        }

        void TestMainThatThrows()
        {
            throw new InvalidOperationException("This threw an error");
        }

        [Fact]
        public async Task It_shows_error_without_invoking_method()
        {
            Action action = TestMainThatThrows;

            var stderr = new StringBuilder();
            _console.Error = new StringWriter(stderr);

            var exitCode =await CommandLine.InvokeMethodAsync(
                new[] {"--unknown"},
                _console,
                this,
                action.Method,
                new CommandHelpMetadata());

            exitCode.Should().Be(CommandLine.ErrorExitCode);
            stderr.ToString()
                .Should().NotBeEmpty()
                .And
                .Contain("--unknown");
            _console.ForegroundColor.Should().Be(ConsoleColor.Red);
        }

        [Fact]
        public async Task It_handles_uncaught_exceptions()
        {
            Action action = TestMainThatThrows;

            var stderr = new StringBuilder();
            _console.Error = new StringWriter(stderr);

            var exitCode =await CommandLine.InvokeMethodAsync(
                Array.Empty<string>(),
                _console,
                this,
                action.Method,
                new CommandHelpMetadata());

            exitCode.Should().Be(CommandLine.ErrorExitCode);
            stderr.ToString()
                .Should().NotBeEmpty()
                .And
                .Contain("This threw an error");
            _console.ForegroundColor.Should().Be(ConsoleColor.Red);
        }
    }
}
