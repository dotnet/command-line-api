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
    public class AddCommandFromMethodTests
    {
        private object[] _receivedValues;
        private readonly TestConsole _testConsole = new TestConsole();
        private readonly ITestOutputHelper _output;

        public AddCommandFromMethodTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Boolean_parameters_will_accept_zero_arguments()
        {
            var builder = new CommandLineBuilder()
                          .AddCommandFromMethod(GetMethodInfo(nameof(Method_taking_bool)), this)
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
                          .AddCommandFromMethod(GetMethodInfo(nameof(Method_taking_bool)), this)
                          .Build();

            await builder.InvokeAsync(commandLine, _testConsole);

            _receivedValues.Should()
                           .BeEquivalentTo(expected);
        }

        internal void Method_taking_bool(bool value = false)
        {
            _receivedValues = new object[] { value };
        }

        internal void Method_taking_int_and_bool(
            int intValue = 41,
            bool boolValue = false)
        {
            _receivedValues = new object[] { intValue, boolValue };
        }

        private MethodInfo GetMethodInfo(string name)
        {
            return typeof(AddCommandFromMethodTests)
                   .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                   .Single(m => m.Name == name);
        }
    }
}
