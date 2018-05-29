// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
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
            var builder = new ArgumentDefinitionBuilder();

            _parser = new Parser(
                new CommandDefinition("outer", "", new[] {
                    new CommandDefinition("inner", "", new[] {
                        new OptionDefinition(
                            "--option",
                            "",
                            builder.ExactlyOne())
                    })
                }));
        }

        [Fact]
        public void Outer_command_is_identified_correctly_by_RootCommand()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result
                .RootCommand
                .Name
                .Should()
                .Be("outer");
        }

        [Fact]
        public void Outer_command_is_identified_correctly_by_Parent_property()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result
                .Command
                .Parent
                .Name
                .Should()
                .Be("outer");
        }

        [Fact]
        public void Inner_command_is_identified_correctly()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result.Command
                  .Name
                  .Should()
                  .Be("inner");
        }

        [Fact]
        public void Inner_command_option_is_identified_correctly()
        {
            var result = _parser.Parse("outer inner --option argument1");

            result.Command
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

            result.Command
                  .Children
                  .ElementAt(0)
                  .Arguments
                  .Should()
                  .BeEquivalentTo("argument1");
        }

        [Fact]
        public void Commands_at_multiple_levels_can_have_their_own_arguments()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("outer", "",
                                     symbols: outer => outer.AddCommand("inner", "",
                                                                        arguments: innerArgs => innerArgs.ZeroOrMore()),
                                     arguments: outerArgs => outerArgs.ExactlyOne())
                         .Build();

            var result = parser.Parse("outer arg1 inner arg2 arg3");

            result.Command
                  .Parent
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg1");

            result.Command
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg2", "arg3");
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
            var builder = new CommandDefinitionBuilder("outer")
                                           .AddCommand("inner", "",
                                                       sibling => sibling.AddCommand("inner-er", "",
                                                                                     arguments: args => args.ZeroOrMore()))
                                           .AddCommand("sibling", "",
                                                       arguments: args => args.ZeroOrMore());
            builder.Arguments.ZeroOrMore();

            var command = builder.BuildCommandDefinition();

            var result = command.Parse(input);

            result.Command.Name.Should().Be(expectedCommand);
        }
    }
}
