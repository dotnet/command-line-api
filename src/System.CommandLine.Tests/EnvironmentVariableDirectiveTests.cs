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
            var rootCommand = new CliRootCommand
            {
                new EnvironmentVariablesDirective()
            };
            rootCommand.SetAction(_ =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(_testVariableName).Should().Be(value);
            });

            var config = new CliConfiguration(rootCommand)
            {
                EnableDefaultExceptionHandler = false
            };

            await config.InvokeAsync($"[env:{_testVariableName}={value}]");

            asserted.Should().BeTrue();
        }

        [Fact]
        public async Task Sets_environment_variable_value_containing_equals_sign()
        {
            bool asserted = false;
            const string value = "1=2";
            var rootCommand = new CliRootCommand
            {
                new EnvironmentVariablesDirective()
            };
            rootCommand.SetAction(_ =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(_testVariableName).Should().Be(value);
            });

            var config = new CliConfiguration(rootCommand)
            {
                EnableDefaultExceptionHandler = false
            };

            await config.InvokeAsync($"[env:{_testVariableName}={value}]" );

            asserted.Should().BeTrue();
        }

        [Fact]
        public async Task Ignores_environment_directive_without_equals_sign()
        {
            bool asserted = false;
            string variable = _testVariableName;
            var rootCommand = new CliRootCommand
            {
                new EnvironmentVariablesDirective()
            };
            rootCommand.SetAction(_ =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(variable).Should().BeNull();
            });

            var config = new CliConfiguration(rootCommand)
            {
                EnableDefaultExceptionHandler = false
            };

            await config.InvokeAsync( $"[env:{variable}]" );

            asserted.Should().BeTrue();
        }

        [Fact]
        public static async Task Ignores_environment_directive_with_empty_variable_name()
        {
            bool asserted = false;
            string value = "value";
            var rootCommand = new CliRootCommand
            {
                new EnvironmentVariablesDirective()
            };
            rootCommand.SetAction(_ =>
            {
                asserted = true;
                var env = Environment.GetEnvironmentVariables();
                env.Values.Cast<string>().Should().NotContain(value);
            });

            var config = new CliConfiguration(rootCommand)
            {
                EnableDefaultExceptionHandler = false
            };

            var result = config.Parse($"[env:={value}]");

            await result.InvokeAsync();

            asserted.Should().BeTrue();
        }

        [Fact]
        public void It_does_not_prevent_help_from_being_invoked()
        {
            var root = new CliRootCommand();
            root.SetAction(_ => { });

            var customHelpAction = new CustomHelpAction();
            root.Options.OfType<HelpOption>().Single().Action = customHelpAction;

            var config = new CliConfiguration(root);
            root.Directives.Add(new EnvironmentVariablesDirective());

            root.Parse($"[env:{_testVariableName}=1] -h", config).Invoke();

            customHelpAction.WasCalled.Should().BeTrue();
            Environment.GetEnvironmentVariable(_testVariableName).Should().Be("1");
        }

        private class CustomHelpAction : SynchronousCliAction
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
