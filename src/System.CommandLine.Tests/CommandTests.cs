// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Parsing;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CommandTests
    {
        private readonly CliCommand _outerCommand;

        public CommandTests()
        {
            _outerCommand = new CliCommand("outer")
            {
                new CliCommand("inner")
                {
                    new CliOption<string>("--option")
                }
            };
        }

        [Fact]
        public void Outer_command_is_identified_correctly_by_RootCommand()
        {
            var result = _outerCommand.Parse("outer inner --option argument1");

            result
                .RootCommandResult
                .Command
                .Name
                .Should()
                .Be("outer");
        }

        [Fact]
        public void Outer_command_is_identified_correctly_by_Parent_property()
        {
            var result = _outerCommand.Parse("outer inner --option argument1");

            result
                .CommandResult
                .Parent
                .Should()
                .BeOfType<CommandResult>()
                .Which
                .Command
                .Name
                .Should()
                .Be("outer");
        }

        [Fact]
        public void Inner_command_is_identified_correctly()
        {
            var result = _outerCommand.Parse("outer inner --option argument1");

            result.CommandResult
                  .Should()
                  .BeOfType<CommandResult>()
                  .Which
                  .Command
                  .Name
                  .Should()
                  .Be("inner");
        }

        [Fact]
        public void Inner_command_option_is_identified_correctly()
        {
            var result = _outerCommand.Parse("outer inner --option argument1");

            result.CommandResult
                  .Children
                  .ElementAt(0)
                  .Should()
                  .BeOfType<OptionResult>()
                  .Which
                  .Option
                  .Name
                  .Should()
                  .Be("--option");
        }

        [Fact]
        public void Inner_command_option_argument_is_identified_correctly()
        {
            var result = _outerCommand.Parse("outer inner --option argument1");

            result.CommandResult
                  .Children
                  .ElementAt(0)
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("argument1");
        }

        [Fact]
        public void Commands_at_multiple_levels_can_have_their_own_arguments()
        {
            var outer = new CliCommand("outer")
            {
                new CliArgument<string>("outer_arg")
            };
            outer.Subcommands.Add(
                new CliCommand("inner")
                {
                    new CliArgument<string[]>("inner_arg")
                });

            var result = outer.Parse("outer arg1 inner arg2 arg3");

            result.CommandResult
                  .Parent
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("arg1");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("arg2", "arg3");
        }

        [Fact]
        public void Aliases_is_aware_of_added_alias()
        {
            var command = new CliCommand("original");

            command.Aliases.Add("added");

            command.Aliases.Should().Contain("added");
        }


        [Theory]
        [InlineData("aa ")]
        [InlineData(" aa")]
        [InlineData("aa aa")]
        public void When_a_command_is_created_with_an_alias_that_contains_whitespace_then_an_informative_error_is_returned(
            string alias)
        {
            Action create = () => new CliCommand(alias);

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Contain($"Names and aliases cannot contain whitespace: \"{alias}\"");
        }

        [Theory]
        [InlineData("aa ")]
        [InlineData(" aa")]
        [InlineData("aa aa")]
        public void When_a_command_alias_is_added_and_contains_whitespace_then_an_informative_error_is_returned(
            string alias)
        {
            var command = new CliCommand("-x");

            Action addAlias = () => command.Aliases.Add(alias);

            addAlias
                .Should()
                .Throw<ArgumentException>()
                .Which
                .Message
                .Should()
                .Contain($"Names and aliases cannot contain whitespace: \"{alias}\"");
        }

        [Theory]
        [InlineData("outer", "outer")]
        [InlineData("outer arg", "outer")]
        [InlineData("outer inner", "inner")]
        [InlineData("outer arg inner", "inner")]
        [InlineData("outer arg inner arg", "inner")]
        [InlineData("outer sibling", "sibling")]
        [InlineData("outer inner inner-er", "inner-er")]
        [InlineData("outer inner arg inner-er", "inner-er")]
        [InlineData("outer inner arg inner-er arg", "inner-er")]
        [InlineData("outer arg inner arg inner-er arg", "inner-er")]
        public void ParseResult_Command_identifies_innermost_command(string input, string expectedCommand)
        {
            var outer = new CliCommand("outer")
            {
                new CliCommand("inner")
                {
                    new CliCommand("inner-er")
                },
                new CliCommand("sibling")
            };

            var result = outer.Parse(input);

            result.CommandResult.Command.Name.Should().Be(expectedCommand);
        }

        [Fact]
        public void Commands_can_have_aliases()
        {
            var command = new CliCommand("this");
            command.Aliases.Add("that");
            command.Name.Should().Be("this");
            command.Aliases.Should().BeEquivalentTo("that");
            command.Aliases.Should().BeEquivalentTo("that");

            var result = command.Parse("that");

            result.CommandResult.Command.Should().BeSameAs(command);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void RootCommand_can_have_aliases()
        {
            var command = new CliRootCommand();
            command.Aliases.Add("that");
            command.Aliases.Should().BeEquivalentTo("that");
            command.Aliases.Should().BeEquivalentTo("that");

            var result = command.Parse("that");

            result.CommandResult.Command.Should().BeSameAs(command);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Subcommands_can_have_aliases()
        {
            var subcommand = new CliCommand("this");
            subcommand.Aliases.Add("that");

            var rootCommand = new CliRootCommand
            {
                subcommand
            };

            var result = rootCommand.Parse("that");

            result.CommandResult.Command.Should().BeSameAs(subcommand);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void It_retains_argument_name_when_it_is_provided()
        {
            var command = new CliCommand("-alias")
            {
                new CliArgument<bool>("arg")
            };

            command.Arguments.Single().Name.Should().Be("arg");
        }

        [Fact]
        public void AddGlobalOption_updates_Options_property()
        {
            var option = new CliOption<string>("-x") { Recursive = true };
            var command = new CliCommand("mycommand");
            command.Options.Add(option);

            command.Options
                   .Should()
                   .Contain(option);
        }

        // https://github.com/dotnet/command-line-api/issues/1437
        [Fact]
        public void When_Options_is_referenced_before_a_global_option_is_added_then_adding_a_global_option_updates_the_Options_collection()
        {
            var option = new CliOption<string>("-x");
            var command = new CliCommand("mycommand");

            // referencing command.Options here would reproduce the above bug before the fix
            // keeping it ensures the fix works and doesn't regress
            command.Options
                .Should()
                .BeEmpty();

            option.Recursive = true;
            command.Options.Add(option);

            command.Options
                .Should()
                .Contain(option);
        }
    }
}
