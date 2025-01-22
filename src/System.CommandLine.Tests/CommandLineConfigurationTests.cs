// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public class CommandLineConfigurationTests
{
    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_option_aliases_on_the_root_command()
    {
        var option1 = new Option<string>("--dupe");
        var option2 = new Option<string>("-y");
        option2.Aliases.Add("--dupe");

        var command = new RootCommand
        {
            option1,
            option2
        };

        var config = new CommandLineConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CommandLineConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Duplicate alias '--dupe' found on command '{command.Name}'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_option_aliases_on_a_subcommand()
    {
        var option1 = new Option<string>("--dupe");
        var option2 = new Option<string>("--ok");
        option2.Aliases.Add("--dupe");

        var command = new RootCommand
        {
            new Command("subcommand")
            {
                option1,
                option2
            }
        };

        var config = new CommandLineConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CommandLineConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("Duplicate alias '--dupe' found on command 'subcommand'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_subcommand_aliases_on_the_root_command()
    {
        var command1 = new Command("dupe");
        var command2 = new Command("not-a-dupe");
        command2.Aliases.Add("dupe");

        var rootCommand = new RootCommand
        {
            command1,
            command2
        };

        var config = new CommandLineConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CommandLineConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Duplicate alias 'dupe' found on command '{rootCommand.Name}'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_subcommand_aliases_on_a_subcommand()
    {
        var command = new RootCommand
        {
            new Command("subcommand")
            {
                new Command("dupe"),
                new Command("not-a-dupe") { Aliases = { "dupe" } }
            }
        };

        var config = new CommandLineConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CommandLineConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("Duplicate alias 'dupe' found on command 'subcommand'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_sibling_command_and_option_aliases_collide_on_the_root_command()
    {
        var option = new Option<string>("dupe");
        var command = new Command("not-a-dupe");
        command.Aliases.Add("dupe");

        var rootCommand = new RootCommand
        {
            option,
            command
        };

        var config = new CommandLineConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CommandLineConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Duplicate alias 'dupe' found on command '{rootCommand.Name}'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_sibling_command_and_option_aliases_collide_on_a_subcommand()
    {
        var option = new Option<string>("dupe");
        var command = new Command("not-a-dupe");
        command.Aliases.Add("dupe");

        var rootCommand = new RootCommand
        {
            new Command("subcommand")
            {
                option,
                command
            }
        };

        var config = new CommandLineConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CommandLineConfigurationException>()
                .Which
                .Message
                .Should()
                .Be("Duplicate alias 'dupe' found on command 'subcommand'.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_there_are_duplicate_sibling_global_option_aliases_on_the_root_command()
    {
        var option1 = new Option<string>("--dupe") { Recursive = true };
        var option2 = new Option<string>("-y") { Recursive = true };
        option2.Aliases.Add("--dupe");

        var command = new RootCommand();
        command.Options.Add(option1);
        command.Options.Add(option2);

        var config = new CommandLineConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CommandLineConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Duplicate alias '--dupe' found on command '{command.Name}'.");
    }

    [Fact]
    public void ThrowIfInvalid_does_not_throw_if_global_option_alias_is_the_same_as_local_option_alias()
    {
        var rootCommand = new RootCommand
        {
            new Command("subcommand")
            {
                new Option<string>("--dupe")
            }
        };
        rootCommand.Options.Add(new Option<string>("--dupe") { Recursive = true });

        var config = new CommandLineConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfInvalid_does_not_throw_if_global_option_alias_is_the_same_as_subcommand_alias()
    {
        var rootCommand = new RootCommand
        {
            new Command("subcommand")
            {
                new Command("--dupe")
            }
        };
        rootCommand.Options.Add(new Option<string>("--dupe") { Recursive = true });

        var config = new CommandLineConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_a_command_is_its_own_parent()
    {
        var command = new RootCommand();
        command.Add(command);

        var config = new CommandLineConfiguration(command);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CommandLineConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Cycle detected in command tree. Command '{command.Name}' is its own ancestor.");
    }

    [Fact]
    public void ThrowIfInvalid_throws_if_a_parentage_cycle_is_detected()
    {
        var command = new Command("command");
        var rootCommand = new RootCommand { command };
        command.Add(rootCommand);

        var config = new CommandLineConfiguration(rootCommand);

        var validate = () => config.ThrowIfInvalid();

        validate.Should()
                .Throw<CommandLineConfigurationException>()
                .Which
                .Message
                .Should()
                .Be($"Cycle detected in command tree. Command '{rootCommand.Name}' is its own ancestor.");
    }

    [Fact]
    public void It_can_be_subclassed_to_provide_additional_context()
    {
        var command = new RootCommand();
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

public class CustomAppConfiguration : CommandLineConfiguration
{
    public CustomAppConfiguration(RootCommand command) : base(command)
    {
        EnableDefaultExceptionHandler = false;
    }

    public IServiceProvider ServiceProvider { get; }
}