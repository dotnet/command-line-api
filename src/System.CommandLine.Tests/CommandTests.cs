// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CommandTests
    {
        private readonly Parser _parser;

        public CommandTests()
        {
            _parser = new Parser(
                new Command("outer")
                {
                    new Command("inner")
                    {
                        new Option("--option",
                                   argument: new Argument
                                             {
                                                 Arity = ArgumentArity.ExactlyOne
                                             })
                    }
                });
        }

        [Fact]
        public void Outer_command_is_identified_correctly_by_RootCommand()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result
                .RootCommandResult
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
                .Name
                .Should()
                .Be("outer");
        }

        [Fact]
        public void Inner_command_is_identified_correctly()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result.CommandResult
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
                  .Arguments
                  .Should()
                  .BeEquivalentTo("argument1");
        }

        [Fact]
        public void Commands_at_multiple_levels_can_have_their_own_arguments()
        {
            var outer = new Command("outer", 
                argument: new Argument
                          {
                              Arity = ArgumentArity.ExactlyOne
                          });
            outer.AddCommand(
                new Command("inner",
                            argument: new Argument
                                      {
                                          Arity = ArgumentArity.ZeroOrMore
                                      }));

            var parser = new Parser(outer);

            var result = parser.Parse("outer arg1 inner arg2 arg3");

            result.CommandResult
                  .Parent
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg1");

            result.CommandResult
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg2", "arg3");
        }
             
        [Theory]
        [InlineData("aa:")]
        [InlineData("aa=")]
        [InlineData("aa ")]
        [InlineData(":aa")]
        [InlineData("=aa")]
        [InlineData(" aa")]
        [InlineData("aa:aa")]
        [InlineData("aa=aa")]
        [InlineData("aa aa")]
        public void When_a_command_name_contains_a_delimiter_then_an_error_is_returned(
            string commandWithDelimiter)
        {
            Action create = () => new Parser(
                new Command(
                    commandWithDelimiter, "",
                    argument: new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }));

            create.Should().Throw<SymbolCannotContainDelimiterArgumentException>();
        }

        [Fact]
        public void When_a_command_name_contains_a_delimiter_then_the_error_is_informative()
        {
            var subject = new SymbolCannotContainDelimiterArgumentException('ツ');
            subject.Message.Should()
                .Be(@"Symbol cannot contain delimiter: ""ツ""");
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

            result.CommandResult.Name.Should().Be(expectedCommand);
        }

        [Fact]
        public void Commands_can_have_aliases()
        {
            var command = new Command("this");
            command.AddAlias("that");

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
    }
}
