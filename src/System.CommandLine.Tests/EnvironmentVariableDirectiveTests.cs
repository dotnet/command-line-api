using System;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace System.CommandLine.Tests
{
    public class EnvironmentVariableDirectiveTests
    {
        private static readonly Random randomizer = new Random(Seed: 456476756);

        [Fact]
        public static async Task Sets_environment_variable_to_value()
        {
            bool asserted = false;
            string variable = $"TEST_ENVIRONMENT_VARIABLE{randomizer.Next()}";
            const string value = "This is a test";
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() =>
                {
                    asserted = true;
                    Assert.Equal(value, Environment.GetEnvironmentVariable(variable));
                })
            };

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:{variable}={value}]" });

            Assert.True(asserted);
        }

        [Fact]
        public static async Task Trims_environment_variable_name()
        {
            bool asserted = false;
            string variable = $"TEST_ENVIRONMENT_VARIABLE{randomizer.Next()}";
            const string value = "This is a test";
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() =>
                {
                    asserted = true;
                    Assert.Equal(value, Environment.GetEnvironmentVariable(variable));
                })
            };

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:     {variable}    ={value}]" });

            Assert.True(asserted);
        }

        [Fact]
        public static async Task Trims_environment_variable_value()
        {
            bool asserted = false;
            string variable = $"TEST_ENVIRONMENT_VARIABLE{randomizer.Next()}";
            const string value = "This is a test";
            var rootCommand = new RootCommand
            {
                Handler = CommandHandler.Create(() =>
                {
                    asserted = true;
                    Assert.Equal(value, Environment.GetEnvironmentVariable(variable));
                })
            };

            var parser = new CommandLineBuilder(rootCommand)
                .UseEnvironmentVariableDirective()
                .Build();

            await parser.InvokeAsync(new[] { $"[env:{variable}=    {value}     ]" });

            Assert.True(asserted);
        }
    }
}
