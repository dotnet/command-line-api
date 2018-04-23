// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using FluentAssertions;
using Xunit;
using static Microsoft.DotNet.Cli.CommandLine.Accept;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class SuggestionTests
    {
        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_command()
        {
            var parser = new CommandParser(Command("outer", "",
                                            Command("one", "Command one"),
                                            Command("two", "Command two"),
                                            Command("three", "Command three")));

            var result = parser.Parse("outer ");

            result.Suggestions().Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new CommandParser(Command("outer", "",
                                            Option("--one", "Option one"),
                                            Option("--two", "Option two"),
                                            Option("--three", "Option three")));

            var result = parser.Parse("outer ");

            result.Suggestions().Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Argument_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new CommandParser(
                Command("outer", "",
                        Option("--one", "", 
                            Define.Arguments().FromAmong("one-a", "one-b").ExactlyOne()),
                        Option("--two", "", 
                            Define.Arguments().FromAmong("two-a", "two-b").ExactlyOne())));

            var result = parser.Parse("outer --two ");

            result.Suggestions().Should().BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option_and_partial_input()
        {
            var parser = new CommandParser(Command("outer", "",
                                            Command("one", "Command one"),
                                            Command("two", "Command two"),
                                            Command("three", "Command three")));

            var result = parser.Parse("outer o");

            result.Suggestions().Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Suggestions_can_be_provided_in_the_absence_of_validation()
        {
            var command = Command("the-command", "",
                                  Option("-t", "",
                                      Define.Arguments()
                                          .WithSuggestions("vegetable",
                                              "mineral",
                                              "animal")
                                          .ExactlyOne()));

            command.Parse("the-command -t m")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");

            command.Parse("the-command -t something-else").Errors.Should().BeEmpty();
        }

        [Fact]
        public void Suggestions_can_be_provided_using_a_delegate()
        {
            var command = Command("the-command", "",
                                  Command("one", "",
                                      Define.Arguments().WithSuggestions("vegetable",
                                          "mineral",
                                          "animal").ExactlyOne()));

            command.Parse("the-command one m")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");
        }

        [Fact]
        public void Validations_and_suggestions_can_be_provided_using_a_delegate()
        {
            var command = Command("the-command", "",
                                  Command("one", "",
                                      Define.Arguments().FromAmong("vegetable",
                                          "mineral",
                                          "animal")
                                          .ExactlyOne()));

            command.Parse("the-command one m")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");

            command
                .Parse("the-command one fungus")
                .Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(
                    "Unrecognized command or argument 'fungus'",
                    "Required argument missing for command: one");
        }

        [Fact]
        public void When_we_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var parser = new CommandParser(
                Command("outer", "",
                    Define.Arguments().None(),
                        Option("one", "", 
                            Define.Arguments().FromAmong("one-a", "one-b", "one-c")
                                .ExactlyOne()),
                        Option("two", "", 
                            Define.Arguments().FromAmong("two-a", "two-b", "two-c")
                                .ExactlyOne()),
                        Option("three", "", 
                            Define.Arguments().FromAmong("three-a", "three-b", "three-c")
                                .ExactlyOne())));

            var result = parser.Parse(new[] { "outer", "two", "b" });

            Console.WriteLine(result.Diagram());

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var parser = new CommandParser(
                Command("outer", "",
                        NoArguments(),
                        Option("one", "", 
                            Define.Arguments().FromAmong("one-a", "one-b", "one-c")
                                .ExactlyOne()),
                        Option("two", "", 
                            Define.Arguments().FromAmong("two-a", "two-b", "two-c")
                                .ExactlyOne()),
                        Option("three", "", 
                            Define.Arguments().FromAmong("three-a", "three-b", "three-c")
                                .ExactlyOne())));

            var result = parser.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_we_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var parser = new CommandParser(
                Command("outer", "",
                        NoArguments(),
                        Command("one", "", 
                            Define.Arguments().FromAmong("one-a", "one-b", "one-c")
                                .ExactlyOne()),
                        Command("two", "", 
                            Define.Arguments().FromAmong("two-a", "two-b", "two-c")
                                .ExactlyOne()),
                        Command("three", "", 
                            Define.Arguments().FromAmong("three-a", "three-b", "three-c")
                                .ExactlyOne()))
                );

            var result = parser.Parse(new[] { "outer", "two", "b" });

            Console.WriteLine(result.Diagram());

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var parser = new CommandParser(
                Command("outer", "",
                    Command("one", "", 
                        Define.Arguments().FromAmong("one-a", "one-b", "one-c")
                            .ExactlyOne()),
                    Command("two", "", 
                        Define.Arguments().FromAmong("two-a", "two-b", "two-c")
                            .ExactlyOne()),
                    Command("three", "", 
                        Define.Arguments().FromAmong("three-a", "three-b", "three-c")
                            .ExactlyOne()))
            );

            var result = parser.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_position_is_unspecified_then_TextToMatch_matches_partial_argument_at_end_of_command_line()
        {
            var command = Command("the-command", "",
                                  Option("--option1", ""),
                                  Option("--option2", ""));

            var textToMatch = command.Parse("the-command t")
                                     .TextToMatch();

            textToMatch.Should().Be("t");
        }

        [Fact]
        public void When_position_is_unspecified_and_command_line_ends_with_a_space_then_TextToMatch_returns_empty()
        {
            var command = Command("the-command", "",
                                  Option("--option1", ""),
                                  Option("--option2", ""));

            var textToMatch = command.Parse("the-command t ")
                                     .TextToMatch();

            textToMatch.Should().Be("");
        }

        [Theory]
        [InlineData("the-command $one --two")]
        [InlineData("the-command one$ --two")]
        [InlineData("the-command on$e --two ")]
        [InlineData(" the-command  $one --two ")]
        [InlineData(" the-command  one$ --two ")]
        [InlineData(" the-command  on$e --two ")]
        public void When_position_is_specified_then_TextToMatch_matches_argument_at_cursor_position(string input)
        {
            var command = Command("the-command", "", Define.Arguments().ZeroOrMore());

            var textToMatch = command.Parse(input.Replace("$", ""))
                                     .TextToMatch(input.IndexOf("$", StringComparison.Ordinal));

            textToMatch.Should().Be("one");
        }
    }
}