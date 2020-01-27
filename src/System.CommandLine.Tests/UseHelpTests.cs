// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class UseHelpTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task UseHelp_writes_help_for_the_specified_command()
        {
            var command = new Command("command");
            var subcommand = new Command("subcommand");
            command.AddCommand(subcommand);

            var parser =
                new CommandLineBuilder()
                    .AddCommand(command)
                    .UseHelp()
                    .Build();

            var result = parser.Parse("command subcommand --help");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().Contain($"{RootCommand.ExecutableName} command subcommand");
        }

        [Fact]
        public async Task UseHelp_interrupts_execution_of_the_specified_command()
        {
            var wasCalled = false;
            var command = new Command("command");
            var subcommand = new Command("subcommand");
            subcommand.Handler = CommandHandler.Create(() => wasCalled = true);
            command.AddCommand(subcommand);

            var parser =
                new CommandLineBuilder()
                    .AddCommand(command)
                    .UseHelp()
                    .Build();

            await parser.InvokeAsync("command subcommand --help", _console);

            wasCalled.Should().BeFalse();
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public async Task UseHelp_accepts_default_values(string value)
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand(new Command("command"))
                    .UseHelp()
                    .Build();

            await parser.InvokeAsync($"command {value}", _console);

            _console.Out.ToString().Should().StartWith("Usage:");
        }

        [Fact]
        public async Task UseHelp_accepts_custom_aliases()
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand(new Command("command"))
                    .UseHelp(new Option(new[] { "~cthulhu" }))
                    .Build();

            await parser.InvokeAsync("command ~cthulhu", _console);

            _console.Out.ToString().Should().StartWith("Usage:");
        }

        [Fact]
        public async Task UseHelp_does_not_display_when_option_defined_with_same_alias()
        {
            var command = new Command("command");
            command.AddOption(new Option("-h"));
            
            var parser =
                new CommandLineBuilder()
                    .AddCommand(command)
                    .UseHelp()
                    .Build();

            var result = parser.Parse("command -h");

            await result.InvokeAsync(_console);

            _console.Out.ToString().Should().BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_root_command()
        {
            var parser = new CommandLineBuilder()
                .UseDefaults()
                .Build();

            var result = parser.Parse("-h");

            result.Errors
                  .Should()
                  .BeEmpty();
        }
        
        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_subcommand()
        {
            var command = new RootCommand
            {
                new Command("subcommand")
            };

            var parser = new CommandLineBuilder(command)
                         .UseDefaults()
                         .Build();

            var result = parser.Parse("subcommand -h");

            result.Errors
                  .Should()
                  .BeEmpty();
        }
    }
}
