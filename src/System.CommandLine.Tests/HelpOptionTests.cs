// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Help;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests;

public class HelpOptionTests
{
    [Fact]
    public async Task Help_option_writes_help_for_the_specified_command()
    {
        CliCommand command = new CliRootCommand
        {
            new CliCommand("command")
            {
                new CliCommand("subcommand")
            }
        };

        CliConfiguration config = new(command)
        {
            Output = new StringWriter()
        };

        var result = command.Parse("command subcommand --help", config);

        await result.InvokeAsync();

        config.Output.ToString().Should().Contain($"{CliRootCommand.ExecutableName} command subcommand [options]");
    }
         
    [Fact]
    public async Task Help_option_interrupts_execution_of_the_specified_command()
    {
        var wasCalled = false;
        var command = new CliCommand("command") { new HelpOption() };
        var subcommand = new CliCommand("subcommand");
        subcommand.SetAction(_ => wasCalled = true);
        command.Subcommands.Add(subcommand);

        CliConfiguration config = new(command)
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
        CliConfiguration config = new(new CliCommand("command") { new HelpOption() })
        {
            Output = new StringWriter()
        };

        StringWriter console = new();
        config.Output = console;

        await config.InvokeAsync($"command {value}");

        console.ToString().Should().ShowHelp();
    }

    [Fact]
    public async Task Help_option_does_not_display_when_option_defined_with_same_alias()
    {
        var command = new CliCommand("command");
        command.Options.Add(new CliOption<bool>("-h"));

        CliConfiguration config = new(command)
        {
            Output = new StringWriter()
        };

        await command.Parse("command -h", config).InvokeAsync();

        config.Output.ToString().Should().NotShowHelp();
    }

    [Fact]
    public void There_are_no_parse_errors_when_help_is_invoked_on_root_command()
    {
        CliRootCommand rootCommand = new();

        var result = rootCommand.Parse("-h");

        result.Errors
              .Should()
              .BeEmpty();
    }
        
    [Fact]
    public void There_are_no_parse_errors_when_help_is_invoked_on_subcommand()
    {
        var root = new CliRootCommand
        {
            new CliCommand("subcommand"),
        };

        var result = root.Parse("subcommand -h");

        result.Errors
              .Should()
              .BeEmpty();
    }

    [Fact]
    public void There_are_no_parse_errors_when_help_is_invoked_on_a_command_with_subcommands()
    {
        var root = new CliRootCommand
        {
            new CliCommand("subcommand"),
        };

        var result = root.Parse("-h");

        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void There_are_no_parse_errors_when_help_is_invoked_on_a_command_with_required_options()
    {
        var command = new CliRootCommand
        {
            new CliOption<string>("-x")
            {
                Required = true
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
        CliRootCommand command = new()
        {
            new HelpOption("/lost", "--confused")
        };
        CliConfiguration config = new(command)
        {
            Output = new StringWriter()
        };

        await config.InvokeAsync(helpAlias);

        config.Output.ToString().Should().ShowHelp();
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("/h")]
    [InlineData("--help")]
    [InlineData("-?")]
    [InlineData("/?")]
    public async Task Help_option_with_custom_aliases_does_not_recognize_default_aliases(string helpAlias)
    {
        CliRootCommand command = new();
        command.Options.Clear();
        command.Options.Add(new HelpOption("--confused"));

        CliConfiguration config = new(command)
        {
            Output = new StringWriter(),
        };

        await config.InvokeAsync(helpAlias);

        config.Output.ToString().Should().NotContain(helpAlias);
    }
}