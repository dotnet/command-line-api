// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public class CliConfigurationTests
{
    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_option_aliases_on_the_root_command()
    {
        var option1 = new CliOption<string>("--dupe");
        var option2 = new CliOption<string>("-y");
        option2.Aliases.Add("--dupe");

        var command = new CliRootCommand
        {
            option1,
            option2
        };

        var config = new CliConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CliConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Duplicate alias '--dupe' found on command '{command.Name}'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_option_aliases_on_a_subcommand()
    {
        var option1 = new CliOption<string>("--dupe");
        var option2 = new CliOption<string>("--ok");
        option2.Aliases.Add("--dupe");

        var command = new CliRootCommand
        {
            new CliCommand("subcommand")
            {
                option1,
                option2
            }
        };

        var config = new CliConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CliConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("Duplicate alias '--dupe' found on command 'subcommand'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_subcommand_aliases_on_the_root_command()
    {
        var command1 = new CliCommand("dupe");
        var command2 = new CliCommand("not-a-dupe");
        command2.Aliases.Add("dupe");

        var rootCommand = new CliRootCommand
        {
            command1,
            command2
        };

        var config = new CliConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CliConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Duplicate alias 'dupe' found on command '{rootCommand.Name}'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_subcommand_aliases_on_a_subcommand()
    {
        var command = new CliRootCommand
        {
            new CliCommand("subcommand")
            {
                new CliCommand("dupe"),
                new CliCommand("not-a-dupe") { Aliases = { "dupe" } }
            }
        };

        var config = new CliConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CliConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("Duplicate alias 'dupe' found on command 'subcommand'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_sibling_command_and_option_aliases_collide_on_the_root_command()
    {
        var option = new CliOption<string>("dupe");
        var command = new CliCommand("not-a-dupe");
        command.Aliases.Add("dupe");

        var rootCommand = new CliRootCommand
        {
            option,
            command
        };

        var config = new CliConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CliConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Duplicate alias 'dupe' found on command '{rootCommand.Name}'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_sibling_command_and_option_aliases_collide_on_a_subcommand()
    {
        var option = new CliOption<string>("dupe");
        var command = new CliCommand("not-a-dupe");
        command.Aliases.Add("dupe");

        var rootCommand = new CliRootCommand
        {
            new CliCommand("subcommand")
            {
                option,
                command
            }
        };

        var config = new CliConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CliConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("Duplicate alias 'dupe' found on command 'subcommand'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_global_option_aliases_on_the_root_command()
    {
        var option1 = new CliOption<string>("--dupe") { Recursive = true };
        var option2 = new CliOption<string>("-y") { Recursive = true };
        option2.Aliases.Add("--dupe");

        var command = new CliRootCommand();
        command.Options.Add(option1);
        command.Options.Add(option2);

        var config = new CliConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CliConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Duplicate alias '--dupe' found on command '{command.Name}'.");
    }

    [Fact]
    public void ThrowIfInvalid_does_not_throw_if_global_option_alias_is_the_same_as_local_option_alias()
    {
        var rootCommand = new CliRootCommand
        {
            new CliCommand("subcommand")
            {
                new CliOption<string>("--dupe")
            }
        };
        rootCommand.Options.Add(new CliOption<string>("--dupe") { Recursive = true });

        var config = new CliConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfInvalid_does_not_throw_if_global_option_alias_is_the_same_as_subcommand_alias()
    {
        var rootCommand = new CliRootCommand
        {
            new CliCommand("subcommand")
            {
                new CliCommand("--dupe")
            }
        };
        rootCommand.Options.Add(new CliOption<string>("--dupe") { Recursive = true });

        var config = new CliConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_a_command_is_its_own_parent()
    {
        var command = new CliRootCommand();
        command.Add(command);

        var config = new CliConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CliConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Cycle detected in command tree. Command '{command.Name}' is its own ancestor.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_a_parentage_cycle_is_detected()
    {
        var command = new CliCommand("command");
        var rootCommand = new CliRootCommand { command };
        command.Add(rootCommand);

        var config = new CliConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CliConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Cycle detected in command tree. Command '{rootCommand.Name}' is its own ancestor.");
    }

    [Fact]
    public void It_can_be_subclassed_to_provide_additional_context()
    {
        var command = new CliRootCommand();
        var commandWasInvoked = false;
        command.SetAction(parseResult =>
        {
            var appConfig = (CustomAppConfiguration)parseResult.Configuration;

            // access custom config

            commandWasInvoked = true;

            return 0;
        });

        var config = new CustomAppConfiguration(command);

        config.Invoke("");

        commandWasInvoked.Should().BeTrue();
    }
}

public class CustomAppConfiguration : CliConfiguration
{
    public CustomAppConfiguration(CliRootCommand command) : base(command)
    {
        EnableDefaultExceptionHandler = false;
    }

    public IServiceProvider ServiceProvider { get; }
}