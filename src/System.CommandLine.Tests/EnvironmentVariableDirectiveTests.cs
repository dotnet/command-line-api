using FluentAssertions;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace System.CommandLine.Tests
{
    public class EnvironmentVariableDirectiveTests
    {
        private static readonly Random randomizer = new(Seed: 456476756);
        private readonly string test_variable = $"TEST_ENVIRONMENT_VARIABLE{randomizer.Next()}";

        [Fact]
        public async Task Sets_environment_variable_to_value()
        {
            bool asserted = false;
            string variable = test_variable;
            const string value = "This is a test";
            var rootCommand = new RootCommand();
            rootCommand.SetHandler(() =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(variable).Should().Be(value);
            });

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:{variable}={value}]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public async Task Trims_environment_variable_name()
        {
            bool asserted = false;
            string variable = test_variable;
            const string value = "This is a test";
            var rootCommand = new RootCommand();
            rootCommand.SetHandler(() =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(variable).Should().Be(value);
            });

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:     {variable}    ={value}]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public async Task Trims_environment_variable_value()
        {
            bool asserted = false;
            string variable = test_variable;
            const string value = "This is a test";
            var rootCommand = new RootCommand();
            rootCommand.SetHandler(() =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(variable).Should().Be(value);
            });

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:{variable}=    {value}     ]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public async Task Sets_environment_variable_value_containing_equals_sign()
        {
            bool asserted = false;
            string variable = test_variable;
            const string value = "This is = a test containing equals";
            var rootCommand = new RootCommand();
            rootCommand.SetHandler(() =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(variable).Should().Be(value);
            });

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:{variable}={value}]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public async Task Ignores_environment_directive_without_equals_sign()
        {
            bool asserted = false;
            string variable = test_variable;
            var rootCommand = new RootCommand();
            rootCommand.SetHandler(() =>
            {
                asserted = true;
                Environment.GetEnvironmentVariable(variable).Should().BeNull();
            });

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:{variable}]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public static async Task Ignores_environment_directive_with_empty_variable_name()
        {
            bool asserted = false;
            string value = $"This is a test, random: {randomizer.Next()}";
            var rootCommand = new RootCommand();
            rootCommand.SetHandler(() =>
            {
                asserted = true;
                var env = Environment.GetEnvironmentVariables();
                env.Values.Cast<string>().Should().NotContain(value);
            });

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:={value}]" });

            asserted.Should().BeTrue();
        }

        [Fact]
        public static async Task Ignores_environment_directive_with_whitespace_variable_name()
        {
            bool asserted = false;
            string value = $"This is a test, random: {randomizer.Next()}";
            var rootCommand = new RootCommand();
            rootCommand.SetHandler(() =>
            {
                asserted = true;
                var env = Environment.GetEnvironmentVariables();
                env.Values.Cast<string>().Should().NotContain(value);
            });

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:    ={value}]" });

            asserted.Should().BeTrue();
        }
    }
}
