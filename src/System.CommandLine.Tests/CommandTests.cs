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
                        new Option("--option")
                        {
                            Argument = new Argument
                            {
                                Arity = ArgumentArity.ExactlyOne
                            }
                        }
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
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };
            outer.AddCommand(
                new Command("inner")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }
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

        [Theory]
        [InlineData("aa:", ":")]
        [InlineData("aa=", "=")]
        [InlineData(":aa", ":")]
        [InlineData("=aa", "=")]
        [InlineData("aa:aa", ":")]
        [InlineData("aa=aa", "=")]
        public void When_a_command_name_contains_a_delimiter_then_an_error_is_returned(
            string commandWithDelimiter,
            string delimiter)
        {
            Action create = () =>
            {
                new Parser(
                    new Command(commandWithDelimiter)
                    {
                        new Argument
                        {
                            Arity = ArgumentArity.ExactlyOne
                        }
                    });
            };

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be($"Command \"{commandWithDelimiter}\" is not allowed to contain a delimiter but it contains \"{delimiter}\"");
        }

        [Theory]
        [InlineData("aa ")]
        [InlineData(" aa")]
        [InlineData("aa aa")]
        public void When_a_command_is_created_with_an_alias_that_contains_whitespace_then_an_informative_error_is_returned(
            string alias)
        {
            Action create = () => new Command(alias);

            create.Should().Throw<ArgumentException>().Which.Message.Should()
                  .Be($"Command alias cannot contain whitespace: \"{alias}\"");
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

            addAlias.Should().Throw<ArgumentException>().Which.Message.Should()
                    .Be($"Command alias cannot contain whitespace: \"{alias}\"");
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
            command.RawAliases.Should().BeEquivalentTo("this", "that");

            var result = command.Parse("that");

            result.CommandResult.Command.Should().Be(command);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void RootCommand_can_have_aliases()
        {
            var command = new RootCommand();
            command.AddAlias("that");
            command.Aliases.Should().BeEquivalentTo(RootCommand.ExecutableName, "that");
            command.RawAliases.Should().BeEquivalentTo(RootCommand.ExecutableName, "that");

            var result = command.Parse("that");

            result.CommandResult.Command.Should().Be(command);
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

            result.CommandResult.Command.Should().Be(subcommand);
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void It_defaults_argument_to_alias_name_when_it_is_not_provided()
        {
            var command = new Command("-alias")
            {
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            command.Arguments.Single().Name.Should().Be("alias");
        }

        [Fact]
        public void It_retains_argument_name_when_it_is_provided()
        {
            var command = new Command("-alias")
            {
                new Argument
                {
                    Name = "arg", Arity = ArgumentArity.ZeroOrOne
                }
            };

            command.Arguments.Single().Name.Should().Be("arg");
        }
  
        [Fact]
        public void When_multiple_arguments_are_configured_then_they_must_differ_by_name()
        {
            var command = new Command("the-command")
            {
                new Argument<string>
                {
                    Name = "same"
                }
            };

            command
                .Invoking(c => c.Add(new Argument<string>
                {
                    Name = "same"
                }))
                .Should()
                .Throw<ArgumentException>()
                .And
                .Message
                .Should()
                .Be("Alias 'same' is already in use.");
        }

        [Fact]
        public void When_multiple_options_are_configured_then_they_must_differ_by_name()
        {
            var command = new Command("the-command")
            {
                new Option("--same")
            };

            command
                .Invoking(c => c.Add(new Option("--same")))
                .Should()
                .Throw<ArgumentException>()
                .And
                .Message
                .Should()
                .Be("Alias '--same' is already in use.");
        }

        [Fact]
        public void Global_options_may_be_added_with_aliases_that_conflict_with_local_options()
        {
            var command = new Command("the-command")
            {
                new Option("--same")
            };

            command
                .Invoking(c => c.AddGlobalOption(new Option("--same")))
                .Should()
                .NotThrow<ArgumentException>();
        }

        [Fact]
        public void Global_options_may_not_have_aliases_conflicting_with_other_global_option_aliases()
        {
            var command = new Command("the-command");

            command.AddGlobalOption(new Option("--same"));

            command
                .Invoking(c => c.AddGlobalOption(new Option("--same")))
                .Should()
                .Throw<ArgumentException>()
                .Which
                .Message
                .Should()
                .Be("Alias '--same' is already in use.");
        }

        [Fact]
        public void When_local_options_are_added_then_they_must_differ_from_global_options_by_name()
        {
            var command = new Command("the-command");
            command.AddGlobalOption(new Option("--same"));

            command
                .Invoking(c => c.Add(new Option("--same")))
                .Should()
                .Throw<ArgumentException>()
                .And
                .Message
                .Should()
                .Be("Alias '--same' is already in use.");
        }

        protected override Symbol CreateSymbol(string name) => new Command(name);
    }
}
