using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ConfigureFromTypeTests
    {
        private readonly TestConsole _testConsole = new TestConsole();
        private readonly ITestOutputHelper _output;

        public ConfigureFromTypeTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task ConfifgureFromType_GivenSimpleClass_InvokeSuccessfully()
        {

            var builder = new CommandLineBuilder()
                          .ConfigureFromType<CommandLineBuilder,WorkingModel>(
                                typeof(WorkingModel).GetMethod(nameof(WorkingModel.Invoke)))
                          .Build();

            int sampleValue = 42;

            var parseResult = builder.Parse($"{CommandLineBuilder.ExeName} --sample-property {sampleValue} --other-property {sampleValue}");

            _output.WriteLine(parseResult.Diagram());

            int result  = await builder.InvokeAsync(parseResult, _testConsole);

            result.Should().Be(sampleValue+sampleValue);
        }

        //[Theory]
        //[InlineData("--value true", true)]
        //[InlineData("--value false", false)]
        //[InlineData("--value:true", true)]
        //[InlineData("--value:false", false)]
        //[InlineData("--value=true", true)]
        //[InlineData("--value=false", false)]
        //public async Task Boolean_parameters_will_accept_one_argument(string commandLine, bool expected)
        //{
        //    var builder = new CommandLineBuilder()
        //                  .ConfigureFromMethod(GetMethodInfo(nameof(Method_taking_bool)), this)
        //                  .Build();

        //    await builder.InvokeAsync(commandLine, _testConsole);

        //    _receivedValues.Should()
        //                   .BeEquivalentTo(expected);
        //}

        //[Fact]
        //public async Task Single_parameter_arguments_generate_aliases_that_accept_a_single_dash_prefix()
        //{
        //    var builder = new CommandLineBuilder()
        //                  .ConfigureFromMethod(GetMethodInfo(nameof(Method_with_single_letter_parameters)), this)
        //                  .Build();

        //    await builder.InvokeAsync("-x 123 -y 456", _testConsole);

        //    _receivedValues.Should()
        //                   .BeEquivalentTo(new[] { 123, 456 });
        //}

        //[Fact]
        //public async Task When_method_returns_void_then_return_code_is_0()
        //{
        //    var builder = new CommandLineBuilder()
        //                  .ConfigureFromMethod(GetMethodInfo(nameof(Method_returning_void)), this)
        //                  .Build();

        //    var result = await builder.InvokeAsync("", _testConsole);

        //    result.Should().Be(0);
        //}

        //[Fact]
        //public async Task When_method_returns_int_then_return_code_is_set_to_return_value()
        //{
        //    var builder = new CommandLineBuilder()
        //                  .ConfigureFromMethod(GetMethodInfo(nameof(Method_returning_int)), this)
        //                  .Build();

        //    var result = await builder.InvokeAsync("-i 123", _testConsole);

        //    result.Should().Be(123);
        //}

        //[Fact]
        //public async Task When_method_returns_Task_of_int_then_return_code_is_set_to_return_value()
        //{
        //    var builder = new CommandLineBuilder()
        //                  .ConfigureFromMethod(GetMethodInfo(nameof(Method_returning_Task_of_int)), this)
        //                  .Build();

        //    var result = await builder.InvokeAsync("-i 123", _testConsole);

        //    result.Should().Be(123);
        //}

        public class WorkingModel
        {
            public WorkingModel(int sampleProperty)
            {
                SampleProperty = sampleProperty;
            }
            public int SampleProperty { get; set; }
            public int OtherProperty { get; set; }

            public int Invoke() { return SampleProperty + OtherProperty; }

        }

        public class ErroringModel
        {
            public int SampleProperty { get; set; }
            public int SampleReadOnlyProperty { get; } = 42;

        }



        private PropertyInfo GetPropertyInfo(string name)
        {
            return typeof(ConfigureFromTypeTests)
                   .GetProperty(name);
        }
    }
}
