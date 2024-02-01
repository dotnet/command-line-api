// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Completions;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class CompletionContextTests
    {
        [Fact]
        public void CommandLineText_preserves_command_line_prior_to_splitting_when_complete_command_line_is_parsed()
        {
            var command = new CliRootCommand
            {
                new CliCommand("verb")
                {
                    new CliOption<int>("-x")
                }
            };

            var commandLine = "verb -x 123";

            var parseResult = command.Parse(commandLine);

            parseResult.GetCompletionContext()
                       .Should()
                       .BeOfType<TextCompletionContext>()
                       .Which
                       .CommandLineText
                       .Should()
                       .Be(commandLine);
        }

        [Fact]
        public void CommandLineText_is_preserved_when_adjusting_position()
        {
            var command = new CliRootCommand
            {
                new CliCommand("verb")
                {
                    new CliOption<int>("-x")
                }
            };

            var commandLine = "verb -x 123";

            var completionContext1 = (TextCompletionContext)command.Parse(commandLine).GetCompletionContext();

            var completionContext2 = completionContext1.AtCursorPosition(4);

            completionContext2.CommandLineText.Should().Be(commandLine);
        }

        [Fact]
        public void CommandLineText_is_unavailable_when_string_array_is_parsed()
        {
            var command = new CliRootCommand
            {
                new CliCommand("verb")
                {
                    new CliOption<int>("-x")
                }
            };

            var parseResult = command.Parse(new[] { "verb", "-x", "123" });

            parseResult.GetCompletionContext()
                       .Should()
                       .BeOfType<CompletionContext>();
        }

        [Fact]
        public void When_position_is_unspecified_in_string_command_line_not_ending_with_a_space_then_it_returns_final_token()
        {
            var command = new CliCommand("the-command")
            {
                new CliOption<string>("--option1"),
                new CliOption<string>("--option2")
            };

            string textToMatch = command.Parse("the-command t")
                                        .GetCompletionContext()
                                        .WordToComplete;

            textToMatch.Should().Be("t");
        }

        [Fact]
        public void When_position_is_unspecified_in_string_command_line_ending_with_a_space_then_it_returns_empty()
        {
            var command = new CliCommand("the-command")
            {
                new CliOption<string>("--option1"),
                new CliOption<string>("--option2")
            };

            var commandLine = "the-command t";
            string textToMatch = command.Parse(commandLine)
                                        .GetCompletionContext()
                                        .As<TextCompletionContext>()
                                        .AtCursorPosition(commandLine.Length + 1)
                                        .WordToComplete;

            textToMatch.Should().Be("");
        }

        [Fact]
        public void When_position_is_greater_than_input_length_in_a_string_command_line_then_it_returns_empty()
        {
            CliOption<string> option1 = new ("--option1");
            option1.AcceptOnlyFromAmong("apple", "banana", "cherry", "durian");

            var command = new CliCommand("the-command")
            {
                new CliArgument<string>("arg"),
                option1,
                new CliOption<string>("--option2")
            };

            var textToMatch = command.Parse("the-command --option1 a")
                                     .GetCompletionContext()
                                     .As<TextCompletionContext>()
                                     .AtCursorPosition(1000)
                                     .WordToComplete;

            textToMatch.Should().Be("");
        }

        [Fact]
        public void When_position_is_unspecified_in_array_command_line_and_final_token_is_unmatched_then_it_returns_final_token()
        {
            var command = new CliCommand("the-command")
            {
                new CliOption<string>("--option1"),
                new CliOption<string>("--option2")
            };

            string textToMatch = command.Parse(new[] { "the-command", "opt" })
                                        .GetCompletionContext()
                                        .WordToComplete;

            textToMatch.Should().Be("opt");
        }

        [Fact]
        public void When_position_is_unspecified_in_array_command_line_and_final_token_matches_an_command_then_it_returns_empty()
        {
            var command = new CliCommand("the-command")
            {
                new CliOption<string>("--option1"),
                new CliOption<string>("--option2")
            };

            string textToMatch = command.Parse(new[] { "the-command" })
                                        .GetCompletionContext()
                                        .WordToComplete;

            textToMatch.Should().Be("");
        }

        [Fact]
        public void When_position_is_unspecified_in_array_command_line_and_final_token_matches_an_option_then_it_returns_empty()
        {
            var command = new CliCommand("the-command")
            {
                new CliOption<string>("--option1"),
                new CliOption<string>("--option2")
            };

            string textToMatch = command.Parse(new[] { "the-command", "--option1" })
                                        .GetCompletionContext()
                                        .WordToComplete;

            textToMatch.Should().Be("");
        }

        [Fact]
        public void When_position_is_unspecified_in_array_command_line_and_final_token_matches_an_argument_then_it_returns_the_argument_value()
        {
            CliOption<string> option1 = new("--option1");
            option1.AcceptOnlyFromAmong("apple", "banana", "cherry", "durian");

            var command = new CliCommand("the-command")
            {
                option1,
                new CliOption<string>("--option2"),
                new CliArgument<string>("arg")
            };

            string textToMatch = command.Parse(new[] { "the-command", "--option1", "a" })
                                        .GetCompletionContext()
                                        .WordToComplete;

            textToMatch.Should().Be("a");
        }

        [Theory]
        [InlineData("the-command $one --two", "one")]
        [InlineData("the-command one$ --two", "one")]
        [InlineData("the-command on$e --two ", "one")]
        [InlineData(" the-command  $one --two ", "one")]
        [InlineData(" the-command  one$ --two ", "one")]
        [InlineData(" the-command  on$e --two ", "one")]
        public void When_position_is_specified_in_string_command_line_then_it_returns_argument_at_cursor_position(
            string commandLine,
            string expected)
        {
            var command =
                new CliCommand("the-command")
                {
                    new CliArgument<string[]>("arg")
                };

            var position = commandLine.IndexOf("$", StringComparison.Ordinal);

            var textToMatch = command.Parse(commandLine.Replace("$", ""))
                                     .GetCompletionContext()
                                     .As<TextCompletionContext>()
                                     .AtCursorPosition(position)
                                     .WordToComplete;

            textToMatch.Should().Be(expected);
        }
    }
}