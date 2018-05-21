// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class AddHelpTests
    {
        private readonly TestConsole _console = new TestConsole();

        [Fact]
        public void AddHelp_writes_help_for_the_specified_command()
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "",
                                command => command.AddCommand("subcommand"))
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command subcommand --help");

            result.Invoke(_console);

            _console.Out.ToString().Should().StartWith("Usage: command subcommand");
        }

        [Fact]
        public void AddHelp_interrupts_execution_of_the_specified_command()
        {
            var wasCalled = false;

            var parser =
                new ParserBuilder()
                    .AddCommand("command", "",
                                command => command.AddCommand("subcommand")
                                                  .OnExecute<string>(_ => wasCalled = true))
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command subcommand --help");

            result.Invoke(new TestConsole());

            wasCalled.Should().BeFalse();
        }

        [Fact]
        public void AddHelp_allows_help_for_all_configured_prefixes()
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp()
                    .UsePrefixes(new[] { "~" })
                    .Build();

            var result = parser.Parse("command ~help");
            result.Invoke(_console);

            _console.Out.ToString().Should().StartWith("Usage:");
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public void AddHelp_accepts_default_values(string value)
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp()
                    .Build();

            var result = parser.Parse($"command {value}");
            result.Invoke(_console);

            _console.Out.ToString().Should().StartWith("Usage:");
        }

        [Fact]
        public void AddHelp_accepts_collection_of_help_options()
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "")
                    .AddHelp(new[] { "~cthulhu" })
                    .Build();

            var result = parser.Parse("command ~cthulhu");

            result.Invoke(_console);

            _console.Out.ToString().Should().StartWith("Usage:");
        }

        [Fact]
        public void AddHelp_does_not_display_when_option_defined_with_same_alias()
        {
            var parser =
                new ParserBuilder()
                    .AddCommand("command", "",
                                cmd => cmd.AddOption("-h"))
                    .AddHelp()
                    .Build();

            var result = parser.Parse("command -h");

            result.Invoke(_console);

            _console.Out.ToString().Should().BeEmpty();
        }
    }
}
