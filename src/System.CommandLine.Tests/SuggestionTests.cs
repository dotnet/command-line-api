// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class SuggestionTests
    {
        private ITestOutputHelper _output;

        public SuggestionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Option_Suggest_returns_argument_suggestions_if_configured()
        {
            var option = new OptionDefinition(
                "--hello",
                "",
                new ArgumentDefinitionBuilder()
                    .AddSuggestions("one", "two", "three")
                    .ExactlyOne());

            var suggestions = option.Suggest(option.Parse("--hello "));

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_Suggest_returns_available_option_aliases()
        {
            var command = new CommandDefinition("command", "a command", new[] {
                new OptionDefinition("--one", "option one"),
                new OptionDefinition("--two", "option two"),
                new OptionDefinition("--three", "option three")
            });

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands()
        {
            var command = new CommandDefinition(
                "command", "a command",
                new[] {
                    new CommandDefinition("one", "subcommand one"),
                    new CommandDefinition("two", "subcommand two"),
                    new CommandDefinition("three", "subcommand three")
                });

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands_and_option_aliases()
        {
            var command = new CommandDefinition(
                "command", "a command",
                new SymbolDefinition[] {
                    new CommandDefinition("subcommand", "subcommand"),
                    new OptionDefinition("--option", "option")
                });

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("subcommand", "--option");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands_and_option_aliases_and_configured_arguments()
        {
            var command = new CommandDefinition(
                "command", "a command",
                new SymbolDefinition[] {
                    new CommandDefinition("subcommand", "subcommand"),

                    new OptionDefinition("--option", "option")
                },
                new ArgumentDefinitionBuilder()
                    .AddSuggestions("command-argument")
                    .OneOrMore());

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should()
                       .BeEquivalentTo("subcommand", "--option", "command-argument");
        }

        [Fact]
        public void When_an_option_has_a_default_value_it_will_still_be_suggested()
        {
            var parser = new CommandLineBuilder()
                         .AddOption("--apple", "kinds of apples", args => args.WithDefaultValue(() => "grannysmith"))
                         .AddOption("--banana", "kinds of bananas")
                         .AddOption("--cherry", "kinds of cherries")
                         .Build();

            var result = parser.Parse("");

            _output.WriteLine(result.ToString());

            _output.WriteLine(string.Join(Environment.NewLine, result.Suggestions()));

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("--apple",
                                  "--banana",
                                  "--cherry");
        }

        [Fact]
        public void When_one_option_has_been_specified_then_it_and_its_siblings_will_still_be_suggested()
        {
            var parser = new CommandLineBuilder()
                         .AddOption("--apple", "kinds of apples")
                         .AddOption("--banana", "kinds of bananas")
                         .AddOption("--cherry", "kinds of cherries")
                         .Build();

            var result = parser.Parse("--apple grannysmith ");

            _output.WriteLine(result.ToString());

            _output.WriteLine(string.Join(Environment.NewLine, result.Suggestions()));

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("--apple",
                                  "--banana",
                                  "--cherry");
        }

        [Fact]
        public void When_one_option_has_been_partially_specified_then_nonmatching_siblings_will_not_be_suggested()
        {
            var parser = new CommandLineBuilder()
                         .AddOption("--apple", "kinds of apples")
                         .AddOption("--banana", "kinds of bananas")
                         .AddOption("--cherry", "kinds of cherries")
                         .Build();

            var result = parser.Parse("a");

            _output.WriteLine(result.ToString());

            _output.WriteLine(string.Join(Environment.NewLine, result.Suggestions()));

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("--apple",
                                  "--banana");
        }

        [Fact]
        public void A_command_can_be_hidden_from_completions_by_leaving_its_help_empty()
        {
            var command = new CommandDefinition(
                "the-command", "Does things.",
                new[] {
                    new OptionDefinition("--hide-me", ""),
                    new OptionDefinition("-n", "Not hidden")
                });

            var suggestions = command.Parse("the-command ").Suggestions();

            suggestions.Should().NotContain("--hide-me");
        }

        [Fact]
        public void Parser_options_can_supply_context_sensitive_matches()
        {
            var parser = new Parser(
                new OptionDefinition(
                    "--bread", "",
                    new ArgumentDefinitionBuilder()
                        .FromAmong("wheat", "sourdough", "rye")
                        .ExactlyOne()),
                new OptionDefinition(
                    "--cheese", "",
                    new ArgumentDefinitionBuilder()
                        .FromAmong("provolone", "cheddar", "cream cheese")
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
            var command = new CommandDefinition(
                "test", "",
                new[] {
                    new CommandDefinition("one", "Command one"),
                    new CommandDefinition("two", "Command two")
                },
                new ArgumentDefinitionBuilder().ExactlyOne());

            command.Parse("test ")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Both_subcommands_and_options_are_available_as_suggestions()
        {
            var command = new CommandDefinition(
                "test", "",
                new SymbolDefinition[] {
                    new CommandDefinition("one", "Command one"),
                    new OptionDefinition("--one", "Option one")
                }, new ArgumentDefinitionBuilder().ExactlyOne());

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
                    new OptionDefinition("--one", "Option one"),
                    new OptionDefinition("--two", "Option two"),
                    new OptionDefinition("--three", "Option three")
                }));

            ParseResult result = parser.Parse(input);
            result.Suggestions().Should().BeEmpty();
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                new CommandDefinition("outer", "", new[] {
                    new OptionDefinition("--one", "Option one"),
                    new OptionDefinition("--two", "Option two"),
                    new OptionDefinition("--three", "Option three")
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
                        new ArgumentDefinitionBuilder()
                            .FromAmong("one-a", "one-b")
                            .ExactlyOne()),
                    new OptionDefinition(
                        "--two",
                        "",
                        new ArgumentDefinitionBuilder()
                            .FromAmong("two-a", "two-b")
                            .ExactlyOne())
                }));

            ParseResult result = parser.Parse("outer --two ");

            result.Suggestions().Should().BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option_and_partial_input()
        {
            var parser = new Parser(
                new CommandDefinition(
                    "outer", "",
                    new[] {
                        new CommandDefinition("one", "Command one"),
                        new CommandDefinition("two", "Command two"),
                        new CommandDefinition("three", "Command three")
                    }));

            ParseResult result = parser.Parse("outer o");

            result.Suggestions().Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Suggestions_can_be_provided_in_the_absence_of_validation()
        {
            var commandDefinition = new CommandDefinition(
                "the-command", "",
                new[] {
                    new OptionDefinition("-t", "",
                                         new ArgumentDefinitionBuilder()
                                             .AddSuggestions("vegetable", "mineral", "animal")
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
            var commandDefinition = new CommandDefinition(
                "the-command", "",
                new[] {
                    new CommandDefinition(
                        "one", "",
                        new ArgumentDefinitionBuilder()
                            .AddSuggestionSource((parseResult, pos) => new[] {
                                "vegetable",
                                "mineral",
                                "animal"
                            })
                            .ExactlyOne())
                });

            commandDefinition.Parse("the-command one m")
                             .Suggestions()
                             .Should()
                             .BeEquivalentTo("animal", "mineral");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var parser = new CommandLineBuilder()
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
            var parser = new CommandLineBuilder()
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
            var parser = new CommandLineBuilder()
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
            var parser = new CommandLineBuilder()
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
                new OptionDefinition("--option1", ""),
                new OptionDefinition("--option2", "")
            });

            string textToMatch = commandDefinition.Parse("the-command t")
                                                  .TextToMatch();

            textToMatch.Should().Be("t");
        }

        [Fact]
        public void When_position_is_unspecified_and_command_line_ends_with_a_space_then_TextToMatch_returns_empty()
        {
            CommandDefinition commandDefinition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition("--option1", ""),
                new OptionDefinition("--option2", "")
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
            CommandDefinition commandDefinition =
                new CommandDefinition("the-command", "",
                                      new ArgumentDefinitionBuilder().ZeroOrMore());

            string textToMatch = commandDefinition.Parse(input.Replace("$", ""))
                                                  .TextToMatch(input.IndexOf("$", StringComparison.Ordinal));

            textToMatch.Should().Be("one");
        }

    }
}
