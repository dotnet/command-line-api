// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class AddHelpTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public async Task AddHelp_writes_help_for_the_specified_command()
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand("command", "",
                                command => command.AddCommand("subcommand"))
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command subcommand --help");

            await parser.InvokeAsync(result, _console);

            _console.Out.ToString().Should().StartWith($"Usage: {CommandLineBuilder.ExeName} command subcommand");
        }

        [Fact]
        public async Task AddHelp_interrupts_execution_of_the_specified_command()
        {
            var wasCalled = false;

            var parser =
                new CommandLineBuilder()
                    .AddCommand("command", "",
                                command => command.AddCommand("subcommand")
                                                  .OnExecute<string>(_ => wasCalled = true))
                    .AddHelp()
                    .Build();


            await parser.InvokeAsync("command subcommand --help", _console);

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public async Task AddHelp_allows_help_for_all_configured_prefixes()
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand("command", "")
                    .AddHelp()
                    .UsePrefixes(new[] { "~" })
                    .Build();


            await parser.InvokeAsync("command ~help", _console);

            _console.Out.ToString().Should().StartWith("Usage:");
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public async Task AddHelp_accepts_default_values(string value)
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand("command", "")
                    .AddHelp()
                    .Build();

            await parser.InvokeAsync($"command {value}", _console);

            _console.Out.ToString().Should().StartWith("Usage:");
        }

        [Fact]
        public async Task AddHelp_accepts_collection_of_help_options()
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand("command", "")
                    .AddHelp(new[] { "~cthulhu" })
                    .Build();

            await parser.InvokeAsync("command ~cthulhu", _console);

            _console.Out.ToString().Should().StartWith("Usage:");
        }

        [Fact]
        public async Task AddHelp_does_not_display_when_option_defined_with_same_alias()
        {
            var parser =
                new CommandLineBuilder()
                    .AddCommand("command", "",
                                cmd => cmd.AddOption("-h"))
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command -h");

            await parser.InvokeAsync(result, _console);

            _console.Out.ToString().Should().BeEmpty();
        }
    }
}
