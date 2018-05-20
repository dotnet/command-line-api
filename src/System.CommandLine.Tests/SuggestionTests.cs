// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.CommandLine.Builder;
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

            var suggestions = option.Suggest(option.Parse("--hello "));

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_Suggest_returns_available_option_aliases()
        {
            var command = new CommandDefinition("command", "a command", new[] {
                new OptionDefinition(
                    "--one",
                    "option one",
                    argumentDefinition: null), new OptionDefinition(
                    "--two",
                    "option two",
                    argumentDefinition: null), new OptionDefinition(
                    "--three",
                    "option three",
                    argumentDefinition: null)
            });

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands()
        {
            var command = new CommandDefinition("command", "a command", new[] { new CommandDefinition("one", "subcommand one", ArgumentDefinition.None), new CommandDefinition("two", "subcommand two", ArgumentDefinition.None), new CommandDefinition("three", "subcommand three", ArgumentDefinition.None) });

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands_and_option_aliases()
        {
            var command = new CommandDefinition(
                "command", "a command",
                new SymbolDefinition[] {
                    new CommandDefinition("subcommand", "subcommand", ArgumentDefinition.None),
                    new OptionDefinition(
                        "--option",
                        "option")
                });

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("subcommand", "--option");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands_and_option_aliases_and_configured_arguments()
        {
            var command = new CommandDefinition("command", "a command", new[] { new CommandDefinition("subcommand", "subcommand", ArgumentDefinition.None), (SymbolDefinition) new OptionDefinition(
                "--option",
                "option",
                argumentDefinition: null) }, new ArgumentDefinitionBuilder()
                                             .AddSuggestions("command-argument")
                                             .OneOrMore());

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should()
                       .BeEquivalentTo("subcommand", "--option", "command-argument");
        }

        [Fact]
        public void An_command_can_be_hidden_from_completions_by_leaving_its_help_empty()
        {
            var command = new CommandDefinition("the-command", "Does things.", new[] {
                new OptionDefinition(
                    "--hide-me",
                    "",
                    argumentDefinition: null), new OptionDefinition(
                    "-n",
                    "Not hidden",
                    argumentDefinition: null)
            });

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
            var command = new CommandDefinition("test", "", new[] { new CommandDefinition("one", "Command one", ArgumentDefinition.None), (SymbolDefinition) new CommandDefinition("two", "Command two", ArgumentDefinition.None) }, new ArgumentDefinitionBuilder().ExactlyOne());

            command.Parse("test ")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Both_subcommands_and_options_are_available_as_suggestions()
        {
            var command = new CommandDefinition("test", "", new[] { new CommandDefinition("one", "Command one", ArgumentDefinition.None), (SymbolDefinition) new OptionDefinition(
                "--one",
                "Option one",
                argumentDefinition: null) }, new ArgumentDefinitionBuilder().ExactlyOne());

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
                new CommandDefinition("outer", "", new[] {
                    new OptionDefinition(
                        "--one",
                        "Option one",
                        argumentDefinition: null), new OptionDefinition(
                        "--two",
                        "Option two",
                        argumentDefinition: null), new OptionDefinition(
                        "--three",
                        "Option three",
                        argumentDefinition: null)
                }));

            ParseResult result = parser.Parse(input);
            result.Suggestions().Should().BeEmpty();
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                new CommandDefinition("outer", "", new[] {
                    new OptionDefinition(
                        "--one",
                        "Option one",
                        argumentDefinition: null), new OptionDefinition(
                        "--two",
                        "Option two",
                        argumentDefinition: null), new OptionDefinition(
                        "--three",
                        "Option three",
                        argumentDefinition: null)
                }));

            ParseResult result = parser.Parse("outer ");
            result.Suggestions().Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Argument_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                new CommandDefinition("outer", "", new[] {
                    new OptionDefinition(
                        "--one",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("one-a", "one-b").ExactlyOne()), new OptionDefinition(
                        "--two",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("two-a", "two-b").ExactlyOne())
                }));

            ParseResult result = parser.Parse("outer --two ");

            result.Suggestions().Should().BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option_and_partial_input()
        {
            var parser = new Parser(
                new CommandDefinition("outer", "", new[] { new CommandDefinition("one", "Command one", ArgumentDefinition.None), new CommandDefinition("two", "Command two", ArgumentDefinition.None), new CommandDefinition("three", "Command three", ArgumentDefinition.None) }));

            ParseResult result = parser.Parse("outer o");

            result.Suggestions().Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Suggestions_can_be_provided_in_the_absence_of_validation()
        {
            CommandDefinition commandDefinition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "-t",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder()
                                        .AddSuggestions("vegetable",
                                                        "mineral",
                                                        "animal")
                                        .ExactlyOne())
            });

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
            CommandDefinition commandDefinition = new CommandDefinition("the-command", "", new[] {
                new CommandDefinition("one", "", symbolDefinitions: null, argumentDefinition: new ArgumentDefinitionBuilder()
                                                                                              .AddSuggestionSource((parseResult, pos) => new[]
                                                                                              {
                                                                                                  "vegetable",
                                                                                                  "mineral",
                                                                                                  "animal"
                                                                                              })
                                                                                              .ExactlyOne())
            });

            commandDefinition.Parse("the-command one m")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var parser = new ParserBuilder()
                         .AddCommand(
                             "outer", "",
                             outer => outer.AddOption(
                                               "one", "",
                                               args => args.FromAmong("one-a", "one-b", "one-c")
                                                           .ExactlyOne())
                                           .AddOption(
                                               "two", "",
                                               args => args.FromAmong("two-a", "two-b", "two-c")
                                                           .ExactlyOne())
                                           .AddOption(
                                               "three", "",
                                               args => args.FromAmong("three-a", "three-b", "three-c")
                                                           .ExactlyOne()))
                         .Build();

            ParseResult result = parser.Parse(new[] { "outer", "two", "b" });

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var parser = new ParserBuilder()
                         .AddCommand(
                             "outer", "",
                             outer => outer.AddOption(
                                               "one", "",
                                               args => args.FromAmong("one-a", "one-b", "one-c")
                                                           .ExactlyOne())
                                           .AddOption(
                                               "two", "",
                                               args => args.FromAmong("two-a", "two-b", "two-c")
                                                           .ExactlyOne())
                                           .AddOption(
                                               "three", "",
                                               args => args.FromAmong("three-a", "three-b", "three-c")
                                                           .ExactlyOne()))
                         .Build();

            ParseResult result = parser.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var parser = new ParserBuilder()
                         .AddCommand(
                             "outer", "",
                             outer => outer.AddCommand(
                                               "one", "",
                                               arguments: args => args.FromAmong("one-a", "one-b", "one-c")
                                                                      .ExactlyOne())
                                           .AddCommand(
                                               "two", "",
                                               arguments: args => args.FromAmong("two-a", "two-b", "two-c")
                                                                      .ExactlyOne())
                                           .AddCommand(
                                               "three", "",
                                               arguments: args => args.FromAmong("three-a", "three-b", "three-c")
                                                                      .ExactlyOne()))
                         .Build();

            ParseResult result = parser.Parse(new[] { "outer", "two", "b" });

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var parser = new ParserBuilder()
                         .AddCommand(
                             "outer", "",
                             outer => outer.AddCommand(
                                               "one", "",
                                               arguments: args => args.FromAmong("one-a", "one-b", "one-c")
                                                                      .ExactlyOne())
                                           .AddCommand(
                                               "two", "",
                                               arguments: args => args.FromAmong("two-a", "two-b", "two-c")
                                                                      .ExactlyOne())
                                           .AddCommand(
                                               "three", "",
                                               arguments: args => args.FromAmong("three-a", "three-b", "three-c")
                                                                      .ExactlyOne()))
                         .Build();

            ParseResult result = parser.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_position_is_unspecified_then_TextToMatch_matches_partial_argument_at_end_of_command_line()
        {
            CommandDefinition commandDefinition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "--option1",
                    "",
                    argumentDefinition: null), new OptionDefinition(
                    "--option2",
                    "",
                    argumentDefinition: null)
            });

            string textToMatch = commandDefinition.Parse("the-command t")
                                     .TextToMatch();

            textToMatch.Should().Be("t");
        }

        [Fact]
        public void When_position_is_unspecified_and_command_line_ends_with_a_space_then_TextToMatch_returns_empty()
        {
            CommandDefinition commandDefinition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "--option1",
                    "",
                    argumentDefinition: null), new OptionDefinition(
                    "--option2",
                    "",
                    argumentDefinition: null)
            });

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
            CommandDefinition commandDefinition = new CommandDefinition("the-command", "", symbolDefinitions: null, argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore());

            string textToMatch = commandDefinition.Parse(input.Replace("$", ""))
                                     .TextToMatch(input.IndexOf("$", StringComparison.Ordinal));

            textToMatch.Should().Be("one");
        }
    }
}
