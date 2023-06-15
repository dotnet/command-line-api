// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests;

public partial class ParserTests
{
    public class SetupErrors
    {
        [Fact]
        public void When_command_names_are_defined_more_than_once_on_the_same_parent_then_it_throws_on_parse()
        {
            var rootCommand = new CliRootCommand();
            rootCommand.Add(new CliCommand("one"));
            rootCommand.Add(new CliCommand("one"));

            rootCommand.Invoking(c => c.Parse("")).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void When_command_names_collide_with_command_aliases_on_the_same_parent_then_it_throws_on_parse()
        {
            var rootCommand = new CliRootCommand();
            rootCommand.Add(new CliCommand("one"));
            var commandTwo = new CliCommand("two");
            rootCommand.Add(commandTwo);
            commandTwo.Aliases.Add("one");

            rootCommand.Invoking(c => c.Parse("")).Should().Throw<ArgumentException>();
        }

        [Fact(Skip = "https://github.com/dotnet/command-line-api/issues/2223")]
        public void When_command_names_collide_with_option_names_it_throws_on_parse()
        {
            var rootCommand = new CliRootCommand();
            rootCommand.Add(new CliCommand("one"));
            rootCommand.Add(new CliOption<int>("one"));

            rootCommand.Invoking(c => c.Parse("")).Should().Throw<ArgumentException>();
        }

        [Fact(Skip = "https://github.com/dotnet/command-line-api/issues/2223")]
        public void When_command_names_collide_with_option_aliases_it_throws_on_parse()
        {
            var rootCommand = new CliRootCommand();
            rootCommand.Add(new CliCommand("one"));
            var option = new CliOption<int>("two");
            option.Aliases.Add("one");
            rootCommand.Add(option);

            rootCommand.Invoking(c => c.Parse("")).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void When_command_names_collide_with_directive_names_it_does_not_throw_on_parse()
        {
            var rootCommand = new CliRootCommand();
            rootCommand.Add(new CliCommand("one"));
            rootCommand.Add(new CliDirective("one"));
            var cliConfiguration = new CliConfiguration(rootCommand);

            rootCommand.Invoking(c => c.Parse("", cliConfiguration)).Should().NotThrow();
        }
    }
}