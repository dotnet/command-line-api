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
            var option = new Option(
                "--hello",
                "",
                new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                    .WithSuggestions("one", "two", "three"));

            var suggestions = option.Suggest(option.Parse("--hello "));

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_Suggest_returns_available_option_aliases()
        {
            var command = new Command("command", "a command", new[] {
                new Option("--one", "option one"),
                new Option("--two", "option two"),
                new Option("--three", "option three")
            });

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands()
        {
            var command = new Command(
                "command", "a command",
                new[] {
                    new Command("one", "subcommand one"),
                    new Command("two", "subcommand two"),
                    new Command("three", "subcommand three")
                });

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands_and_option_aliases()
        {
            var command = new Command(
                "command", "a command",
                new Symbol[] {
                    new Command("subcommand", "subcommand"),
                    new Option("--option", "option")
                });

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should().BeEquivalentTo("subcommand", "--option");
        }

        [Fact]
        public void Command_Suggest_returns_available_subcommands_and_option_aliases_and_configured_arguments()
        {
            var command = new Command(
                "command", "a command",
                new Symbol[] {
                    new Command("subcommand", "subcommand"),
                    new Option("--option", "option")
                },
                new Argument
                    {
                        Arity = ArgumentArity.OneOrMore
                    }
                    .WithSuggestions("command-argument"));

            var suggestions = command.Suggest(command.Parse("command "));

            suggestions.Should()
                       .BeEquivalentTo("subcommand", "--option", "command-argument");
        }

        [Fact]
        public void When_an_option_has_a_default_value_it_will_still_be_suggested()
        {
            var parser = new Parser(
                new Option("--apple", "kinds of apples",
                           new Argument().WithDefaultValue(() => "grannysmith")),
                new Option("--banana", "kinds of bananas"),
                new Option("--cherry", "kinds of cherries"));

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
            var parser = new Parser(
                new Option("--apple", "kinds of apples"),
                new Option("--banana", "kinds of bananas"),
                new Option("--cherry", "kinds of cherries"));

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
            var command = new Command(
                "the-command", "Does things.",
                new[] {
                    new Option("--hide-me", ""),
                    new Option("-n", "Not hidden")
                });

            var suggestions = command.Parse("the-command ").Suggestions();

            suggestions.Should().NotContain("--hide-me");
        }

        [Fact]
        public void Parser_options_can_supply_context_sensitive_matches()
        {
            var parser = new Parser(
                new Option(
                    "--bread", "",
                    new Argument { Arity = ArgumentArity.ExactlyOne }
                        .FromAmong("wheat", "sourdough", "rye")),
                new Option(
                    "--cheese", "",
                    new Argument { Arity = ArgumentArity.ExactlyOne }
                        .FromAmong("provolone", "cheddar", "cream cheese")));

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
            var command = new Command(
                "test", "",
                new[] {
                    new Command("one", "Command one"),
                    new Command("two", "Command two")
                },
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                });

            command.Parse("test ")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Both_subcommands_and_options_are_available_as_suggestions()
        {
            var command = new Command(
                "test", "",
                new Symbol[]
                {
                    new Command("one", "Command one"),
                    new Option("--one", "Option one")
                },
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                });

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
                new Command("outer", "", new[] {
                    new Option("--one", "Option one"),
                    new Option("--two", "Option two"),
                    new Option("--three", "Option three")
                }));

            ParseResult result = parser.Parse(input);
            result.Suggestions().Should().BeEmpty();
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                new Command("outer", "", new[] {
                    new Option("--one", "Option one"),
                    new Option("--two", "Option two"),
                    new Option("--three", "Option three")
                }));

            ParseResult result = parser.Parse("outer ");

            result.Suggestions().Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Argument_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                new Command("outer", "",
                            new[]
                            {
                                new Option(
                                    "--one",
                                    "",
                                    new Argument { Arity = ArgumentArity.ExactlyOne }
                                        .FromAmong("one-a", "one-b")),
                                new Option(
                                    "--two",
                                    "",
                                    new Argument { Arity = ArgumentArity.ExactlyOne }
                                        .FromAmong("two-a", "two-b"))
                            }));

            ParseResult result = parser.Parse("outer --two ");

            result.Suggestions().Should().BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option_and_partial_input()
        {
            var parser = new Parser(
                new Command(
                    "outer", "",
                    new[] {
                        new Command("one", "Command one"),
                        new Command("two", "Command two"),
                        new Command("three", "Command three")
                    }));

            ParseResult result = parser.Parse("outer o");

            result.Suggestions().Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Suggestions_can_be_provided_in_the_absence_of_validation()
        {
            var command = new Command(
                "the-command", "",
                new[] {
                    new Option("-t", "",
                               new Argument
                                   {
                                       Arity = ArgumentArity.ExactlyOne
                                   }
                                   .WithSuggestions("vegetable", "mineral", "animal"))
                });

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
            var command = new Command(
                "the-command", "",
                new[] {
                    new Command(
                        "one", "",
                        argument: new Argument
                            {
                                Arity = ArgumentArity.ExactlyOne
                            }
                            .WithSuggestionSource((parseResult, pos) => new[] {
                                "vegetable",
                                "mineral",
                                "animal"
                            }))
                });

            command.Parse("the-command one m")
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
            Command command = new Command("the-command", "", new[] {
                new Option("--option1", ""),
                new Option("--option2", "")
            });

            string textToMatch = command.Parse("the-command t")
                                        .TextToMatch();

            textToMatch.Should().Be("t");
        }

        [Fact]
        public void When_position_is_unspecified_and_command_line_ends_with_a_space_then_TextToMatch_returns_empty()
        {
            Command command = new Command("the-command", "", new[] {
                new Option("--option1", ""),
                new Option("--option2", "")
            });

            string textToMatch = command.Parse("the-command t ")
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
            Command command =
                new Command("the-command", "",
                            argument: new Argument
                            {
                                Arity = ArgumentArity.ZeroOrMore
                            });

            string textToMatch = command.Parse(input.Replace("$", ""))
                                        .TextToMatch(input.IndexOf("$", StringComparison.Ordinal));

            textToMatch.Should().Be("one");
        }
    }
}
