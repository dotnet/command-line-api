﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Help;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests
{
    public class HelpOptionTests
    {
        [Fact]
        public async Task Help_option_writes_help_for_the_specified_command()
        {
            Command command = new RootCommand
            {
                new Command("command")
                {
                    new Command("subcommand")
                }
            };

            CommandLineConfiguration config = new(command)
            {
                Output = new StringWriter()
            };

            var result = command.Parse("command subcommand --help", config);

            await result.InvokeAsync();

            config.Output.ToString().Should().Contain($"{RootCommand.ExecutableName} command subcommand [options]");
        }
         
        [Fact]
        public async Task Help_option_interrupts_execution_of_the_specified_command()
        {
            var wasCalled = false;
            var command = new Command("command") { new HelpOption() };
            var subcommand = new Command("subcommand");
            subcommand.SetAction(_ => wasCalled = true);
            command.Subcommands.Add(subcommand);

            CommandLineConfiguration config = new(command)
            {
                Output = new StringWriter()
            };

            await command.Parse("command subcommand --help", config).InvokeAsync();

            wasCalled.Should().BeFalse();
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public async Task Help_option_accepts_default_values(string value)
        {
            CommandLineConfiguration config = new(new Command("command") { new HelpOption() })
            {
                Output = new StringWriter()
            };

            StringWriter console = new();
            config.Output = console;

            await config.InvokeAsync($"command {value}");

            console.ToString().Should().Contain("Usage:");
        }

        [Fact]
        public async Task Help_option_does_not_display_when_option_defined_with_same_alias()
        {
            var command = new Command("command");
            command.Options.Add(new Option<bool>("-h"));

            CommandLineConfiguration config = new(command)
            {
                Output = new StringWriter()
            };

            await command.Parse("command -h", config).InvokeAsync();

            config.Output.ToString().Should().BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_root_command()
        {
            RootCommand rootCommand = new();

            var result = rootCommand.Parse("-h");

            result.Errors
                  .Should()
                  .BeEmpty();
        }
        
        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_subcommand()
        {
            var root = new RootCommand
            {
                new Command("subcommand"),
            };

            var result = root.Parse("subcommand -h");

            result.Errors
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_a_command_with_subcommands()
        {
            var root = new RootCommand
            {
                new Command("subcommand"),
            };

            var result = root.Parse("-h");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void There_are_no_parse_errors_when_help_is_invoked_on_a_command_with_required_options()
        {
            var command = new RootCommand
            {
                new Option<string>("-x")
                {
                    IsRequired = true
                },
            };

            var result = command.Parse("-h");

            result.Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData("/lost")]
        [InlineData("--confused")]
        public async Task HelpOption_with_custom_aliases_uses_aliases(string helpAlias)
        {
            RootCommand command = new()
            {
                new HelpOption("/lost", "--confused")
            };
            CommandLineConfiguration config = new(command)
            {
                Output = new StringWriter()
            };

            await config.InvokeAsync(helpAlias);

            config.Output.ToString().Should().Contain("Usage:");
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("/h")]
        [InlineData("--help")]
        [InlineData("-?")]
        [InlineData("/?")]
        public async Task Help_option_with_custom_aliases_default_aliases_replaced(string helpAlias)
        {
            RootCommand command = new();
            command.Options.Clear();
            command.Options.Add(new HelpOption("--confused"));

            CommandLineConfiguration config = new(command)
            {
                Output = new StringWriter(),
                EnableParseErrorReporting = false
            };

            await config.InvokeAsync(helpAlias);

            config.Output.ToString().Should().Be("");
        }
    }
}