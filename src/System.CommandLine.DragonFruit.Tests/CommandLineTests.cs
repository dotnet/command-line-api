using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.DragonFruit.Tests
{
    public class CommandLineTests
    {
        private string _captured;

        void TestMain(string name)
        {
            _captured = name;
        }

        [Fact]
        public async Task It_executes_method_with_string_option()
        {
            var context = new MethodContext<int>(((Action<string>)TestMain).Method) {
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

        void TestMainWithDefault(string name = "Bruce")
        {
            _captured = name;
        }

        [Fact]
        public async Task It_executes_method_with_string_option_with_default()
        {
            var context = new MethodContext<int>(((Action<string>)TestMainWithDefault).Method) {
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

    }
}
