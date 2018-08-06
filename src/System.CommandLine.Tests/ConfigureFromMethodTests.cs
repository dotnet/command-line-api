using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using FluentAssertions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ConfigureFromMethodTests
    {
        private object[] _receivedValues;
        private readonly TestConsole _testConsole = new TestConsole();
        private readonly ITestOutputHelper _output;

        public ConfigureFromMethodTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Boolean_parameters_will_accept_zero_arguments()
        {
            var builder = new CommandLineBuilder()
                          .ConfigureFromMethod(GetMethodInfo(nameof(Method_taking_bool)), this)
                          .Build();

            var result = builder.Parse($"{CommandLineBuilder.ExeName} --value");

            _output.WriteLine(result.Diagram());

            await builder.InvokeAsync(result, _testConsole);

            _receivedValues.Should()
                           .BeEquivalentTo(true);
        }

        [Theory]
        [InlineData("--value true", true)]
        [InlineData("--value false", false)]
        [InlineData("--value:true", true)]
        [InlineData("--value:false", false)]
        [InlineData("--value=true", true)]
        [InlineData("--value=false", false)]
        public async Task Boolean_parameters_will_accept_one_argument(string commandLine, bool expected)
        {
            var builder = new CommandLineBuilder()
                          .ConfigureFromMethod(GetMethodInfo(nameof(Method_taking_bool)), this)
                          .Build();

            await builder.InvokeAsync(commandLine, _testConsole);

            _receivedValues.Should()
                           .BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Single_parameter_arguments_generate_aliases_that_accept_a_single_dash_prefix()
        {
            var builder = new CommandLineBuilder()
                          .ConfigureFromMethod(GetMethodInfo(nameof(Method_with_single_letter_parameters)), this)
                          .Build();

            await builder.InvokeAsync("-x 123 -y 456", _testConsole);

            _receivedValues.Should()
                           .BeEquivalentTo(new[] { 123, 456 });
        }

        internal void Method_taking_bool(bool value = false)
        {
            _receivedValues = new object[] { value };
        }

        internal void Method_with_single_letter_parameters(
            int x,
            int y)
        {
            _receivedValues = new object[] { x, y };
        }

        private MethodInfo GetMethodInfo(string name)
        {
            return typeof(ConfigureFromMethodTests)
                   .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .Single(m => m.Name == name);
        }
    }
}
