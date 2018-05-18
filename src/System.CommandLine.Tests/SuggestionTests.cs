// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System;
using System.CommandLine.Builder;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class SuggestionTests
    {
        [Fact]
        public void Option_Suggest_returns_argument_suggestions_if_configured()
        {
            var option = new OptionDefinition(
                "--hello",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder()
                                          .AddSuggestions("one", "two", "three")
                                          .ExactlyOne());

            var suggestions = option.Suggest(option.Parse("--hello"));

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_Suggest_returns_available_option_aliases()
        {
            var command = Create.Command("command", "a command",
                                         new OptionDefinition(
                                             "--one",
                                             "option one",
                                             argumentDefinition: null),
                                         new OptionDefinition(
                                             "--two",
                                             "option two",
                                             argumentDefinition: null),
                                         new OptionDefinition(
                                             "--three",
                                             "option three",
                                             argumentDefinition: null));

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands()
        {
            var command = Create.Command("command", "a command",
                                         Create.Command("one", "subcommand one"),
                                         Create.Command("two", "subcommand two"),
                                         Create.Command("three", "subcommand three"));

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands_and_option_aliases()
        {
            var command = Create.Command("command", "a command",
                                         Create.Command("subcommand", "subcommand"),
                                         new OptionDefinition(
                                             "--option",
                                             "option",
                                             argumentDefinition: null));

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("subcommand", "--option");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands_and_option_aliases_and_configured_arguments()
        {
            var command = Create.Command("command", "a command",
                                         new ArgumentDefinitionBuilder()
                                               .AddSuggestions("command-argument")
                                               .OneOrMore(),
                                         Create.Command("subcommand", "subcommand"),
                                         new OptionDefinition(
                                             "--option",
                                             "option",
                                             argumentDefinition: null));

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should()
                       .BeEquivalentTo("subcommand", "--option", "command-argument");
        }

        [Fact]
        public void An_command_can_be_hidden_from_completions_by_leaving_its_help_empty()
        {
            var command = Create.Command(
                "the-command", "Does things.",
                new OptionDefinition(
                    "--hide-me",
                    "",
                    argumentDefinition: null),
                new OptionDefinition(
                    "-n",
                    "Not hidden",
                    argumentDefinition: null));

            var suggestions = command.Parse("the-command ").Suggestions();

            suggestions.Should().NotContain("--hide-me");
        }

        [Fact]
        public void Parser_options_can_supply_context_sensitive_matches()
        {
            var parser = new Parser(
                new OptionDefinition(
                    "--bread",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder()
                                        .FromAmong("wheat", "sourdough", "rye")
                                        .ExactlyOne()),
                new OptionDefinition(
                    "--cheese",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder()
                                        .FromAmong(
                                            "provolone",
                                            "cheddar",
                                            "cream cheese")
                                        .ExactlyOne()));

            var result = parser.Parse("--bread ");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("rye", "sourdough", "wheat");

            result = parser.Parse("--bread wheat --cheese ");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("cheddar", "cream cheese", "provolone");
        }

        [Fact]
        public void Subcommand_names_are_available_as_suggestions()
        {
            var command = Create.Command("test", "",
                                         new ArgumentDefinitionBuilder().ExactlyOne(),
                                         Create.Command("one", "Command one"),
                                         Create.Command("two", "Command two"));

            command.Parse("test ")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Both_subcommands_and_options_are_available_as_suggestions()
        {
            var command = Create.Command("test", "",
                                         new ArgumentDefinitionBuilder().ExactlyOne(),
                                         Create.Command("one", "Command one"),
                                         new OptionDefinition(
                                             "--one",
                                             "Option one",
                                             argumentDefinition: null));

            command.Parse("test ")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("one", "--one");
        }

        [Theory(Skip = "Needs discussion, Issue #19")]
        [InlineData("outer ")]
        [InlineData("outer -")]
        public void Option_suggestions_are_not_provided_without_matching_prefix(string input)
        {
            var parser = new Parser(
                Create.Command("outer", "",
                    new OptionDefinition(
                        "--one",
                        "Option one",
                        argumentDefinition: null),
                    new OptionDefinition(
                        "--two",
                        "Option two",
                        argumentDefinition: null),
                    new OptionDefinition(
                        "--three",
                        "Option three",
                        argumentDefinition: null)));

            ParseResult result = parser.Parse(input);
            result.Suggestions().Should().BeEmpty();
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                Create.Command("outer", "",
                    new OptionDefinition(
                        "--one",
                        "Option one",
                        argumentDefinition: null),
                    new OptionDefinition(
                        "--two",
                        "Option two",
                        argumentDefinition: null),
                    new OptionDefinition(
                        "--three",
                        "Option three",
                        argumentDefinition: null)));

            ParseResult result = parser.Parse("outer ");
            result.Suggestions().Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Argument_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                Create.Command("outer", "",
                    new OptionDefinition(
                        "--one",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("one-a", "one-b").ExactlyOne()),
                    new OptionDefinition(
                        "--two",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("two-a", "two-b").ExactlyOne())));

            ParseResult result = parser.Parse("outer --two ");

            result.Suggestions().Should().BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option_and_partial_input()
        {
            var parser = new Parser(
                Create.Command("outer", "",
                    Create.Command("one", "Command one"),
                    Create.Command("two", "Command two"),
                    Create.Command("three", "Command three")));

            ParseResult result = parser.Parse("outer o");

            result.Suggestions().Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Suggestions_can_be_provided_in_the_absence_of_validation()
        {
            CommandDefinition commandDefinition = Create.Command("the-command", "", new OptionDefinition(
                                                                     "-t",
                                                                     "",
                                                                     argumentDefinition: new ArgumentDefinitionBuilder()
                                                                                               .AddSuggestions("vegetable",
                                                                                                               "mineral",
                                                                                                               "animal")
                                                                                               .ExactlyOne()));

            commandDefinition.Parse("the-command -t m")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");

            commandDefinition.Parse("the-command -t something-else").Errors.Should().BeEmpty();
        }

        [Fact]
        public void Suggestions_can_be_provided_using_a_delegate()
        {
            CommandDefinition commandDefinition = Create.Command(
                "the-command", "",
                Create.Command("one", "",
                               new ArgumentDefinitionBuilder()
                                     .AddSuggestionSource((parseResult, pos) => new[]
                                     {
                                         "vegetable",
                                         "mineral",
                                         "animal"
                                     })
                                     .ExactlyOne()));

            commandDefinition.Parse("the-command one m")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var parser = new Parser(Create.Command("outer", "",
                    ArgumentDefinition.None, new OptionDefinition(
                                                              "one",
                                                              "",
                                                              argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("one-a", "one-b", "one-c")
                                                                                        .ExactlyOne()), new OptionDefinition(
                                                              "two",
                                                              "",
                                                              argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("two-a", "two-b", "two-c")
                                                                                        .ExactlyOne()), new OptionDefinition(
                                                              "three",
                                                              "",
                                                              argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("three-a", "three-b", "three-c")
                                                                                        .ExactlyOne())));

            ParseResult result = parser.Parse(new[] { "outer", "two", "b" });

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var parser = new Parser(
                Create.Command("outer", "", ArgumentDefinition.None,
                    new OptionDefinition(
                        "one",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("one-a", "one-b", "one-c")
                                                  .ExactlyOne()),
                    new OptionDefinition(
                        "two",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("two-a", "two-b", "two-c")
                                                  .ExactlyOne()),
                    new OptionDefinition(
                        "three",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("three-a", "three-b", "three-c")
                                                  .ExactlyOne())));

            ParseResult result = parser.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var parser = new Parser(
                Create.Command("outer", "", ArgumentDefinition.None,
                    Create.Command("one", "",
                            new ArgumentDefinitionBuilder().FromAmong("one-a", "one-b", "one-c")
                                .ExactlyOne()),
                    Create.Command("two", "",
                            new ArgumentDefinitionBuilder().FromAmong("two-a", "two-b", "two-c")
                                .ExactlyOne()),
                    Create.Command("three", "",
                            new ArgumentDefinitionBuilder().FromAmong("three-a", "three-b", "three-c")
                                .ExactlyOne()))
                );

            ParseResult result = parser.Parse(new[] { "outer", "two", "b" });

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var parser = new Parser(
                Create.Command("outer", "",
                    Create.Command("one", "",
                        new ArgumentDefinitionBuilder().FromAmong("one-a", "one-b", "one-c")
                            .ExactlyOne()),
                    Create.Command("two", "",
                        new ArgumentDefinitionBuilder().FromAmong("two-a", "two-b", "two-c")
                            .ExactlyOne()),
                    Create.Command("three", "",
                        new ArgumentDefinitionBuilder().FromAmong("three-a", "three-b", "three-c")
                            .ExactlyOne()))
            );

            ParseResult result = parser.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_position_is_unspecified_then_TextToMatch_matches_partial_argument_at_end_of_command_line()
        {
            CommandDefinition commandDefinition = Create.Command("the-command", "",
                new OptionDefinition(
                    "--option1",
                    "",
                    argumentDefinition: null),
                new OptionDefinition(
                    "--option2",
                    "",
                    argumentDefinition: null));

            string textToMatch = commandDefinition.Parse("the-command t")
                                     .TextToMatch();

            textToMatch.Should().Be("t");
        }

        [Fact]
        public void When_position_is_unspecified_and_command_line_ends_with_a_space_then_TextToMatch_returns_empty()
        {
            CommandDefinition commandDefinition = Create.Command("the-command", "",
                new OptionDefinition(
                    "--option1",
                    "",
                    argumentDefinition: null),
                new OptionDefinition(
                    "--option2",
                    "",
                    argumentDefinition: null));

            string textToMatch = commandDefinition.Parse("the-command t ")
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
            CommandDefinition commandDefinition = Create.Command("the-command", "", new ArgumentDefinitionBuilder().ZeroOrMore());

            string textToMatch = commandDefinition.Parse(input.Replace("$", ""))
                                     .TextToMatch(input.IndexOf("$", StringComparison.Ordinal));

            textToMatch.Should().Be("one");
        }
    }
}
