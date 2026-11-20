// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CommandTests
    {
        private readonly Command _outerCommand;

        public CommandTests()
        {
            _outerCommand = new Command("outer")
            {
                new Command("inner")
                {
                    new Option<string>("--option")
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
            var outer = new Command("outer")
            {
                new Argument<string>("outer_arg")
            };
            outer.Subcommands.Add(
                new Command("inner")
                {
                    new Argument<string[]>("inner_arg")
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
            var command = new Command("original");

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
            Action create = () => new Command(alias);

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
            var command = new Command("-x");

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
            var outer = new Command("outer")
            {
                new Command("inner")
                {
                    new Command("inner-er")
                },
                new Command("sibling")
            };

            var result = outer.Parse(input);

            result.CommandResult.Command.Name.Should().Be(expectedCommand);
        }

        [Fact]
        public void Commands_can_have_aliases()
        {
            var command = new Command("this");
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
            var command = new RootCommand();
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
            var subcommand = new Command("this");
            subcommand.Aliases.Add("that");

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
                new Argument<bool>("arg")
            };

            command.Arguments.Single().Name.Should().Be("arg");
        }

        [Fact]
        public void AddGlobalOption_updates_Options_property()
        {
            var option = new Option<string>("-x") { Recursive = true };
            var command = new Command("mycommand");
            command.Options.Add(option);

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

            option.Recursive = true;
            command.Options.Add(option);

            command.Options
                .Should()
                .Contain(option);
        }

        [Fact]
        public void Parser_creates_only_the_lazy_subcommands_that_were_matched()
        {
            List<string> created = new();
            RootCommand root = new();
            root.Subcommands.AddLazy("a", () =>
            {
                created.Add("a");
                Command a = new("a");
                a.Subcommands.AddLazy("b", () =>
                {
                    created.Add("b");
                    return new Command("b") { new Option<string>("--option") };
                });
                a.Subcommands.AddLazy("c", () => throw new InvalidOperationException("c should not be created"));
                return a;
            });
            root.Subcommands.AddLazy("d", () => throw new InvalidOperationException("d should not be created"));

            ParseResult result = root.Parse("a b --option value");

            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("b");
            result.GetValue<string>("--option").Should().Be("value");
            created.Should().Equal("a", "b");
        }

        [Fact]
        public void Lazy_subcommand_can_be_matched_by_alias_and_gets_the_declared_aliases()
        {
            RootCommand root = new();
            root.Subcommands.AddLazy("solution", ["sln"], () => new Command("solution"));

            ParseResult result = root.Parse("sln");

            result.Errors.Should().BeEmpty();
            result.CommandResult.Command.Name.Should().Be("solution");
            result.CommandResult.Command.Aliases.Should().Equal("sln");
            result.CommandResult.Command.Parents.Should().Equal(root);
        }

        [Fact]
        public void Lazy_subcommand_factory_returning_command_with_different_name_throws()
        {
            RootCommand root = new();
            root.Subcommands.AddLazy("a", () => new Command("b"));

            Action parse = () => root.Parse("a");

            parse.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void Lazy_subcommands_are_created_on_access_but_not_on_count()
        {
            int factoryCalls = 0;
            Command eager = new("eager");
            RootCommand root = new() { eager };
            root.Subcommands.AddLazy("lazy", () =>
            {
                factoryCalls++;
                return new Command("lazy");
            });

            root.Subcommands.Count.Should().Be(2);
            root.Subcommands.Contains(eager).Should().BeTrue();
            factoryCalls.Should().Be(0);

            root.Subcommands.Select(c => c.Name).Should().Equal("eager", "lazy");
            root.Subcommands[1].Should().BeSameAs(root.Subcommands.Single(c => c.Name == "lazy"));
            factoryCalls.Should().Be(1);
        }

        [Fact]
        public void Help_lists_lazy_subcommands_without_creating_their_subcommands()
        {
            RootCommand root = new();
            root.Subcommands.AddLazy("a", () =>
            {
                Command a = new("a", "description of a");
                a.Subcommands.AddLazy("b", () => throw new InvalidOperationException("b should not be created"));
                return a;
            });
            StringWriter output = new();

            root.Parse("--help").Invoke(new() { Output = output });

            output.ToString().Should().Contain("description of a");
        }
    }
}
