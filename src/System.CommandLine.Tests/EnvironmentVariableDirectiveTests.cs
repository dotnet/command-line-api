using System.Collections;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace System.CommandLine.Tests
{
    public class EnvironmentVariableDirectiveTests
    {
        private static readonly Random _random = new();
        private readonly string _testVariableName = $"TEST_ENVIRONMENT_VARIABLE_{_random.Next()}";

        [Fact]
        public async Task Sets_environment_variable_to_value()
        {
            bool asserted = false;
            const string value = "hello";
            var rootCommand = new RootCommand
            {
                new EnvironmentVariablesDirective()
            };
            rootCommand.SetAction(_ =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(_testVariableName).Should().Be(value);
            });

            var config = new InvocationConfiguration
            {
                EnableDefaultExceptionHandler = false
            };

            await rootCommand.Parse($"[env:{_testVariableName}={value}]").InvokeAsync(config);

            asserted.Should().BeTrue();
        }

        [Fact]
        public async Task Sets_environment_variable_value_containing_equals_sign()
        {
            bool asserted = false;
            const string value = "1=2";
            var rootCommand = new RootCommand
            {
                new EnvironmentVariablesDirective()
            };
            rootCommand.SetAction(_ =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(_testVariableName).Should().Be(value);
            });

            var config = new InvocationConfiguration
            {
                EnableDefaultExceptionHandler = false
            };

            await rootCommand.Parse($"[env:{_testVariableName}={value}]" ).InvokeAsync(config);

            asserted.Should().BeTrue();
        }

        [Fact]
        public async Task Ignores_environment_directive_without_equals_sign()
        {
            bool asserted = false;
            string variable = _testVariableName;
            var rootCommand = new RootCommand
            {
                new EnvironmentVariablesDirective()
            };
            rootCommand.SetAction(_ =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(variable).Should().BeNull();
            });

            var config = new InvocationConfiguration
            {
                EnableDefaultExceptionHandler = false
            };

            await rootCommand.Parse( $"[env:{variable}]" ).InvokeAsync(config);

            asserted.Should().BeTrue();
        }

        [Fact]
        public static async Task Ignores_environment_directive_with_empty_variable_name()
        {
            bool asserted = false;
            string value = "value";
            var rootCommand = new RootCommand
            {
                new EnvironmentVariablesDirective()
            };

            IDictionary env = null;
            rootCommand.SetAction(_ =>
            {
                asserted = true;
                env = Environment.GetEnvironmentVariables();
            });

            var result = rootCommand.Parse($"[env:={value}]");

            await result.InvokeAsync();

            asserted.Should().BeTrue();
            env.Values.Cast<string>().Should().NotContain(value);
        }

        [Fact]
        public void It_does_not_prevent_help_from_being_invoked()
        {
            var root = new RootCommand();
            root.SetAction(_ => { });

            var customHelpAction = new CustomHelpAction();
            root.Options.OfType<HelpOption>().Single().Action = customHelpAction;

            root.Directives.Add(new EnvironmentVariablesDirective());

            root.Parse($"[env:{_testVariableName}=1] -h").Invoke();

            customHelpAction.WasCalled.Should().BeTrue();
            Environment.GetEnvironmentVariable(_testVariableName).Should().Be("1");
        }

        private class CustomHelpAction : SynchronousCommandLineAction
        {
            public bool WasCalled { get; private set; }

            public override int Invoke(ParseResult parseResult)
            {
                WasCalled = true;
                return 0;
            }
        }
    }
}
