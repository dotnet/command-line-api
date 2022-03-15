// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Parsing;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CommandTests : SymbolTests
    {
        private readonly Parser _parser;

        public CommandTests()
        {
            _parser = new Parser(
                new Command("outer")
                {
                    new Command("inner")
                    {
                        new Option<string>("--option")
                    }
                });
        }

        [Fact]
        public void Outer_command_is_identified_correctly_by_RootCommand()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result
                .RootCommandResult
                .Symbol
                .Name
                .Should()
                .Be("outer");
        }

        [Fact]
        public void Outer_command_is_identified_correctly_by_Parent_property()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result
                .CommandResult
                .Parent
                .Symbol
                .Name
                .Should()
                .Be("outer");
        }

        [Fact]
        public void Inner_command_is_identified_correctly()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result.CommandResult
                  .Symbol
                  .Name
                  .Should()
                  .Be("inner");
        }

        [Fact]
        public void Inner_command_option_is_identified_correctly()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result.CommandResult
                  .Children
                  .ElementAt(0)
                  .Symbol
                  .Name
                  .Should()
                  .Be("option");
        }

        [Fact]
        public void Inner_command_option_argument_is_identified_correctly()
        {
            var result = _parser.Parse("outer inner --option argument1");

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
            var outer = new Command("outer")
            {
                new Argument<string>()
            };
            outer.AddCommand(
                new Command("inner")
                {
                    new Argument<string[]>()
                });

            var parser = new Parser(outer);

            var result = parser.Parse("outer arg1 inner arg2 arg3");

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
            var command = new Command("original");

            command.AddAlias("added");

            command.Aliases.Should().Contain("added");
            command.HasAlias("added").Should().BeTrue();
        }


        [Theory]
        [InlineData("aa ")]
        [InlineData(" aa")]
        [InlineData("aa aa")]
        public void When_a_command_is_created_with_an_alias_that_contains_whitespace_then_an_informative_error_is_returned(
            string alias)
        {
            Action create = () => new Command(alias);

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Contain($"Alias cannot contain whitespace: \"{alias}\"");
        }

        [Theory]
        [InlineData("aa ")]
        [InlineData(" aa")]
        [InlineData("aa aa")]
        public void When_a_command_alias_is_added_and_contains_whitespace_then_an_informative_error_is_returned(
            string alias)
        {
            var command = new Command("-x");

            Action addAlias = () => command.AddAlias(alias);

            addAlias
                .Should()
                .Throw<ArgumentException>()
                .Which
                .Message
                .Should()
                .Contain($"Alias cannot contain whitespace: \"{alias}\"");
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
            var outer = new Command("outer")
            {
                new Command("inner")
                {
                    new Command("inner-er")
                },
                new Command("sibling")
            };

            var result = outer.Parse(input);

            result.CommandResult.Symbol.Name.Should().Be(expectedCommand);
        }

        [Fact]
        public void Commands_can_have_aliases()
        {
            var command = new Command("this");
            command.AddAlias("that");
            command.Aliases.Should().BeEquivalentTo("this", "that");
            command.Aliases.Should().BeEquivalentTo("this", "that");

            var result = command.Parse("that");

            result.CommandResult.Command.Should().BeSameAs(command);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void RootCommand_can_have_aliases()
        {
            var command = new RootCommand();
            command.AddAlias("that");
            command.Aliases.Should().BeEquivalentTo(RootCommand.ExecutableName, "that");
            command.Aliases.Should().BeEquivalentTo(RootCommand.ExecutableName, "that");

            var result = command.Parse("that");

            result.CommandResult.Command.Should().BeSameAs(command);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Subcommands_can_have_aliases()
        {
            var subcommand = new Command("this");
            subcommand.AddAlias("that");

            var rootCommand = new RootCommand
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
            var command = new Command("-alias")
            {
                new Argument<bool>
                {
                    Name = "arg"
                }
            };

            command.Arguments.Single().Name.Should().Be("arg");
        }

        [Fact]
        public void When_Name_is_set_to_its_current_value_then_it_is_not_removed_from_aliases()
        {
            var command = new Command("name");

            command.Name = "name";

            command.HasAlias("name").Should().BeTrue();
            command.Aliases.Should().Contain("name");
            command.Aliases.Should().Contain("name");
        }

        [Fact]
        public void AddGlobalOption_updates_Options_property()
        {
            var option = new Option<string>("-x");
            var command = new Command("mycommand");
            command.AddGlobalOption(option);

            command.Options
                   .Should()
                   .Contain(option);
        }

        // https://github.com/dotnet/command-line-api/issues/1437
        [Fact]
        public void When_Options_is_referenced_before_a_global_option_is_added_then_adding_a_global_option_updates_the_Options_collection()
        {
            var option = new Option<string>("-x");
            var command = new Command("mycommand");

            // referencing command.Options here would reproduce the above bug before the fix
            // keeping it ensures the fix works and doesn't regress
            command.Options
                .Should()
                .BeEmpty();

            command.AddGlobalOption(option);

            command.Options
                .Should()
                .Contain(option);
        }

        protected override Symbol CreateSymbol(string name) => new Command(name);
    }
}
