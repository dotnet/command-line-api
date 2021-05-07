// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests
{
    public class VersionOptionTests
    {
        private static readonly string version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                                                         .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                                         .InformationalVersion;

        [Fact]
        public async Task When_the_version_option_is_specified_then_the_version_is_written_to_standard_out()
        {
            var parser = new CommandLineBuilder()
                         .UseVersionOption()
                         .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--version", console);

            console.Out.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_then_invocation_is_short_circuited()
        {
            var wasCalled = false;
            var rootCommand = new RootCommand();
            rootCommand.Handler = CommandHandler.Create(() => wasCalled = true);

            var parser = new CommandLineBuilder(rootCommand)
                         .UseVersionOption()
                         .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--version", console);

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Version_option_appears_in_help()
        {
            var parser = new CommandLineBuilder()
                         .UseHelp()
                         .UseVersionOption()
                         .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--help", console);

            console.Out
                   .ToString()
                   .Should()
                   .Match("*Options:*--version*Show version information*");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_and_there_are_default_options_then_the_version_is_written_to_standard_out()
        {
            var rootCommand = new RootCommand
            {
                new Option("-x", getDefaultValue: () => true)
            };
            rootCommand.Handler = CommandHandler.Create(() => { });

            var parser = new CommandLineBuilder(rootCommand)
                .UseVersionOption()
                .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--version", console);

            console.Out.ToString().Should().Be($"{version}{NewLine}");
        }

        [Fact]
        public async Task When_the_version_option_is_specified_and_there_are_default_arguments_then_the_version_is_written_to_standard_out()
        {
            var rootCommand = new RootCommand
            {
                new Argument<bool>("x", getDefaultValue: () => true)
            };
            rootCommand.Handler = CommandHandler.Create(() => { });

            var parser = new CommandLineBuilder(rootCommand)
                .UseVersionOption()
                .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--version", console);

            console.Out.ToString().Should().Be($"{version}{NewLine}");
        }

        [Theory]
        [InlineData("--version -x")]
        [InlineData("--version subcommand")]
        public void Version_is_not_valid_with_other_tokens(string commandLine)
        {
            var rootCommand = new RootCommand
            {
                new Command("subcommand")
                {
                    Handler = CommandHandler.Create(() => { })
                },
                new Option("-x")
            };
            rootCommand.Handler = CommandHandler.Create(() => { });

            var parser = new CommandLineBuilder(rootCommand)
                .UseVersionOption()
                .Build();

            var console = new TestConsole();

            var result = parser.Invoke(commandLine, console);

            console.Out
                   .ToString()
                   .Should()
                   .NotContain(version);

            console.Error
                   .ToString()
                   .Should()
                   .Contain("--version option cannot be combined with other arguments.");

            result.Should().NotBe(0);
        }

        [Fact]
        public void When_the_version_is_not_valid_it_returns_a_custom_result_code()
        {
            var rootCommand = new RootCommand
            {
                new Command("subcommand")
                {
                    Handler = CommandHandler.Create(() => { })
                },
                new Option("-x")
            };

            var parser = new CommandLineBuilder(rootCommand)
                .UseVersionOption(errorExitCode: 42)
                .Build();

            int result = parser.Invoke("--version -x");

            result.Should().Be(42);
        }

        [Fact]
        public void Version_option_is_not_added_to_subcommands()
        {
            var rootCommand = new RootCommand
            {
                new Command("subcommand")
                {
                    Handler = CommandHandler.Create(() => { })
                },
            };
            rootCommand.Handler = CommandHandler.Create(() => { });

            var parser = new CommandLineBuilder(rootCommand)
                         .UseVersionOption()
                         .Build();

            parser.Configuration
                  .RootCommand
                  .Children
                  .GetByAlias("subcommand")
                  .As<Command>()
                  .Options
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public async Task Version_not_added_if_it_exists()
        {
            // Adding an option multiple times can occur two ways in
            // real world scenarios - invocation can be invoked twice
            // or the author may have their own version switch but
            // still want other defaults.
            var parser = new CommandLineBuilder()
                         .UseVersionOption()
                         .UseVersionOption()
                         .Build();

            var console = new TestConsole();

            await parser.InvokeAsync("--version", console);

            console.Out.ToString().Should().Be($"{version}{NewLine}");
        }
    }
}
