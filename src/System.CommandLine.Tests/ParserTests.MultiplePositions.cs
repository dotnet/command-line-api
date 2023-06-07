// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        public class MultiplePositions
        {
            [Theory]
            [InlineData("outer xyz")]
            [InlineData("outer inner xyz")]
            public void An_argument_can_be_specified_in_more_than_one_position(string commandLine)
            {
                var argument = new CliArgument<string>("the-argument");

                var command = new CliCommand("outer")
                {
                    new CliCommand("inner")
                    {
                        argument
                    },
                    argument
                };

                var parseResult = command.Parse(commandLine);

                var argumentResult = parseResult.GetResult(argument);

                argumentResult.Should().NotBeNull();

                argumentResult
                    .GetValueOrDefault<string>()
                    .Should()
                    .Be("xyz");
            }

            [Theory]
            [InlineData("outer xyz inner")]
            [InlineData("outer inner xyz")]
            public void When_an_argument_is_shared_between_an_outer_and_inner_command_then_specifying_in_one_does_not_result_in_error_on_other(string commandLine)
            {
                var argument = new CliArgument<string>("the-argument");

                var command = new CliCommand("outer")
                {
                    new CliCommand("inner")
                    {
                        argument
                    },
                    argument
                };

                var parseResult = command.Parse(commandLine);

                parseResult.Errors.Should().BeEmpty();
            }

            [Theory]
            [InlineData("outer --the-option xyz")]
            [InlineData("outer inner --the-option xyz")]
            public void An_option_can_be_specified_in_more_than_one_position(string commandLine)
            {
                var option = new CliOption<string>("--the-option");

                var command = new CliCommand("outer")
                {
                    new CliCommand("inner")
                    {
                        option
                    },
                    option
                };

                var parseResult = command.Parse(commandLine);

                var optionResult = parseResult.GetResult(option);

                optionResult.Should().NotBeNull();

                optionResult
                    .GetValueOrDefault<string>()
                    .Should()
                    .Be("xyz");
            }

            [Theory]
            [InlineData("outer --the-option xyz inner")]
            [InlineData("outer inner --the-option xyz")]
            public void When_an_option_is_shared_between_an_outer_and_inner_command_then_specifying_in_one_does_not_result_in_error_on_other(string commandLine)
            {
                var option = new CliOption<string>("--the-option");

                var command = new CliCommand("outer")
                {
                    new CliCommand("inner")
                    {
                        option
                    },
                    option
                };

                var parseResult = command.Parse(commandLine);

                parseResult.Errors.Should().BeEmpty();
            }

            [Theory]
            [InlineData("outer inner1 reused --the-option 123", "inner1")]
            [InlineData("outer inner2 reused --the-option 456", "inner2")]
            public void A_command_can_be_specified_in_more_than_one_position(
                string commandLine,
                string expectedParent)
            {
                var reusedCommand = new CliCommand("reused");
                reusedCommand.SetAction((_) => { });
                reusedCommand.Add(new CliOption<string>("--the-option"));

                var outer = new CliCommand("outer")
                {
                    new CliCommand("inner1")
                    {
                        reusedCommand
                    },
                    new CliCommand("inner2")
                    {
                        reusedCommand
                    }
                };

                var result = outer.Parse(commandLine);

                result.Errors.Should().BeEmpty();
                result.CommandResult
                    .Parent
                    .Should()
                    .BeOfType<CommandResult>()
                    .Which
                    .Command
                    .Name
                    .Should()
                    .Be(expectedParent);
            }

            [Fact]
            public void An_option_can_have_multiple_parents_with_the_same_name()
            {
                var option = new CliOption<string>("--the-option");

                var sprocket = new CliCommand("sprocket")
                {
                    new CliCommand("add")
                    {
                        option
                    }
                };

                var widget = new CliCommand("widget")
                {
                    new CliCommand("add")
                    {
                        option
                    }
                };

                var root = new CliRootCommand
                {
                    sprocket,
                    widget
                };

                option.Parents
                      .Select(p => p.Name)
                      .Should()
                      .BeEquivalentTo("add", "add");

                option.Parents
                      .SelectMany(p => p.Parents)
                      .Select(p => p.Name)
                      .Should()
                      .BeEquivalentTo("sprocket", "widget");
            }
        }
    }
}
