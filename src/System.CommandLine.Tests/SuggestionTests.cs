// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class SuggestionTests
    {
        private readonly ITestOutputHelper _output;

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
                           new Argument<string[]>(new[] { "cortland" })),
                new Option("--banana", "kinds of bananas",
                           new Argument<string[]>()),
                new Option("--cherry", "kinds of cherries",
                           new Argument<string>()));

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
            var parser = new Command("command")
                         {
                             new Option("--apple"),
                             new Option("--banana"),
                             new Option("--cherry")
                         };

            var result = parser.Parse("--apple grannysmith ");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("--banana",
                                  "--cherry");
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_commands_will_not_be_suggested()
        {
            var rootCommand = new RootCommand
            {
                new Command("apple")
                {
                    new Option("--cortland")
                },
                new Command("banana")
                {
                    new Option("--cavendish")
                },
                new Command("cherry")
                {
                    new Option("--rainier")
                }
            };

            var result = rootCommand.Parse("cherry ");

            result.Suggestions()
                  .Should()
                  .NotContain(new[]{"apple", "banana", "cherry"});
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_options_will_be_suggested()
        {
            var command = new RootCommand("parent")
                          {
                              new Command("child"),
                              new Option("--parent-option")
                          };
            command.Argument = new Argument<string>();

            var parseResult = command.Parse("child ");

            parseResult
                .Suggestions()
                .Should()
                .Contain("--parent-option");
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_options_with_argument_limit_reached_will_be_not_be_suggested()
        {
            var command = new RootCommand("parent")
                          {
                              new Command("child"),
                              new Option("--parent-option", argument: new Argument<string>())
                          };
            command.Argument = new Argument<string>();

            var parseResult = command.Parse("--parent-option 123 child ");

            parseResult
                .Suggestions()
                .Should()
                .NotContain("--parent-option");
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_child_options_will_be_suggested()
        {
            var command = new RootCommand("parent")
                          {
                              new Command("child")
                              {
                                  new Option("--child-option",
                                             argument: new Argument<string>())
                              }
                          };
            command.Argument = new Argument<string>();

            var parseResult = command.Parse("child ");

            parseResult
                .Suggestions()
                .Should()
                .Contain("--child-option");
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_commands_will_not_be_suggested()
        {
            var rootCommand = new RootCommand
            {
                new Command("apple")
                {
                    new Command("cortland")
                },
                new Command("banana")
                {
                    new Command("cavendish")
                },
                new Command("cherry")
                {
                    new Command("rainier")
                }
            };

            var result = rootCommand.Parse("cherry ");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("rainier");
        }

        [Fact]
        public void When_one_option_has_been_partially_specified_then_nonmatching_siblings_will_not_be_suggested()
        {
            var parser = new CommandLineBuilder()
                         .AddOption(new Option("--apple"))
                         .AddOption(new Option("--banana"))
                         .AddOption(new Option("--cherry"))
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
        public void An_option_can_be_hidden_from_suggestions_by_setting_IsHidden_to_true()
        {
            var command = new Command(
                "the-command", "Does things.",
                new[] {
                    new Option("--hide-me", isHidden: true),
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
                                    argument: new Argument { Arity = ArgumentArity.ExactlyOne }
                                        .FromAmong("one-a", "one-b")),
                                new Option(
                                    "--two",
                                    argument: new Argument { Arity = ArgumentArity.ExactlyOne }
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
            var command = new Command("outer");
            command.AddOption(
                new Option("one",
                           argument: new Argument
                                     {
                                         Arity = ArgumentArity.ExactlyOne
                                     }.FromAmong("one-a", "one-b", "one-c"))
            );
            command.AddOption(
                new Option("two",
                           argument: new Argument
                                     {
                                         Arity = ArgumentArity.ExactlyOne
                                     }.FromAmong("two-a", "two-b", "two-c"))
            );
            command.AddOption(
                new Option("three",
                           argument: new Argument
                                     {
                                         Arity = ArgumentArity.ExactlyOne
                                     }.FromAmong("three-a", "three-b", "three-c"))
            );

            var parser = new CommandLineBuilder()
                         .AddCommand(command)
                         .Build();

            ParseResult result = parser.Parse("outer two b" );

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var command = new Command("outer");
            command.AddOption(
                new Option("one", "",
                           new Argument
                           {
                               Arity = ArgumentArity.ExactlyOne
                           }.FromAmong("one-a", "one-b", "one-c")));
            command.AddOption(
                new Option("two", "",
                           new Argument
                           {
                               Arity = ArgumentArity.ExactlyOne
                           }.FromAmong("two-a", "two-b", "two-c")));
            command.AddOption(
                new Option("three", "",
                           new Argument
                           {
                               Arity = ArgumentArity.ExactlyOne
                           }.FromAmong("three-a", "three-b", "three-c")));

            var result = command.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var outer = new Command("outer");
            var one = new Command(
                "one",
                argument: new Argument
                          {
                              Arity = ArgumentArity.ExactlyOne
                          }.FromAmong("one-a", "one-b", "one-c"));
            var two = new Command(
                "two",
                argument: new Argument
                          {
                              Arity = ArgumentArity.ExactlyOne
                          }.FromAmong("two-a", "two-b", "two-c"));
            var three = new Command(
                "three",
                argument: new Argument
                          {
                              Arity = ArgumentArity.ExactlyOne
                          }.FromAmong("three-a", "three-b", "three-c"));
            outer.AddCommand(one);
            outer.AddCommand(two);
            outer.AddCommand(three);

            ParseResult result = outer.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var outer = new Command("outer")
                        {
                            new Command(
                                "one",
                                argument: new Argument
                                          {
                                              Arity = ArgumentArity.ExactlyOne
                                          }.FromAmong("one-a", "one-b", "one-c")),
                            new Command(
                                "two",
                                argument: new Argument
                                          {
                                              Arity = ArgumentArity.ExactlyOne
                                          }.FromAmong("two-a", "two-b", "two-c")),
                            new Command(
                                "three",
                                argument: new Argument
                                          {
                                              Arity = ArgumentArity.ExactlyOne
                                          }.FromAmong("three-a", "three-b", "three-c"))
                        };

            ParseResult result = outer.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void Arguments_of_type_enum_provide_enum_values_as_suggestions()
        {
            var command = new Command("the-command",
                                      argument: new Argument<FileMode>());

            var suggestions = command.Parse("the-command create")
                                     .Suggestions();

            _output.WriteLine(string.Join("\n", suggestions));

            suggestions.Should().BeEquivalentTo("CreateNew", "Create", "OpenOrCreate");
        }

        [Fact]
        public void Options_that_have_been_specified_to_their_maximum_arity_are_not_suggested()
        {
            var command = new Command("command");
            command.AddOption(new Option("--allows-one",
                                         argument: new Argument<string>()));
            command.AddOption(new Option("--allows-many",
                                         argument: new Argument<string[]>()));

            var suggestions = command.Parse("--allows-one x ").Suggestions();

            suggestions.Should().BeEquivalentTo("--allows-many");
        }

        [Fact]
        public void When_current_symbol_is_an_option_that_requires_arguments_then_parent_symbol_suggestions_are_omitted()
        {
            var parser = new CommandLineBuilder()
                         .AddOption(new Option("--allows-one", argument: new Argument<string>()))
                         .AddOption(new Option("--allows-many", argument: new Argument<string[]>()))
                         .UseSuggestDirective()
                         .Build();

            var suggestions = parser.Parse("--allows-one ").Suggestions();

            suggestions.Should().BeEmpty();
        }

        [Fact]
        public void Option_substring_matching_when_arguments_have_default_values()
        {
            var command = new Command("the-command");
            command.AddOption(
                new Option("--implicit",
                           argument: new Argument<string>(defaultValue: "the-default")));
            command.AddOption(
                new Option("--not",
                           argument: new Argument<string>("the-default")));

            var suggestions = command.Parse("m").Suggestions();

            suggestions.Should().BeEquivalentTo("--implicit");
        }

        public class TextToMatch
        {
            [Fact]
            public void When_position_is_unspecified_in_string_command_line_not_ending_with_a_space_then_it_returns_final_token()
            {
                var command = new Command("the-command", "",
                                          new[]
                                          {
                                              new Option("--option1", ""),
                                              new Option("--option2", "")
                                          });

                string textToMatch = command.Parse("the-command t")
                                            .TextToMatch();

                textToMatch.Should().Be("t");
            }

            [Fact]
            public void When_position_is_unspecified_in_string_command_line_ending_with_a_space_then_it_returns_empty()
            {
                Command command = new Command("the-command", "",
                                              new[]
                                              {
                                                  new Option("--option1", ""),
                                                  new Option("--option2", "")
                                              });

                string textToMatch = command.Parse("the-command t ")
                                            .TextToMatch();

                textToMatch.Should().Be("");
            }

            [Fact]
            public void When_position_is_unspecified_in_array_command_line_and_final_token_is_unmatched_then_it_returns_final_token()
            {
                var command = new Command("the-command", "",
                                          new[]
                                          {
                                              new Option("--option1", ""),
                                              new Option("--option2", "")
                                          });

                string textToMatch = command.Parse("the-command", "opt")
                                            .TextToMatch();

                textToMatch.Should().Be("opt");
            }

            [Fact]
            public void When_position_is_unspecified_in_array_command_line_and_final_token_is_matches_an_command_then_it_returns_empty()
            {
                Command command = new Command("the-command", "",
                                              new[]
                                              {
                                                  new Option("--option1", ""),
                                                  new Option("--option2", "")
                                              });

                string textToMatch = command.Parse(new[] { "the-command" })
                                            .TextToMatch();

                textToMatch.Should().Be("");
            }

            [Fact]
            public void When_position_is_unspecified_in_array_command_line_and_final_token_is_matches_an_option_then_it_returns_empty()
            {
                Command command = new Command("the-command", "",
                                              new[]
                                              {
                                                  new Option("--option1", ""),
                                                  new Option("--option2", "")
                                              });

                string textToMatch = command.Parse("the-command", "--option1")
                                            .TextToMatch();

                textToMatch.Should().Be("");
            }

            [Fact]
            public void When_position_is_unspecified_in_array_command_line_and_final_token_is_matches_an_argument_then_it_returns_empty()
            {
                Command command = new Command("the-command", "",
                                              new[]
                                              {
                                                  new Option("--option1", "", new Argument<string>().FromAmong("apple", "banana", "cherry", "durian")),
                                                  new Option("--option2", "", new Argument<string>())
                                              });
                command.Argument = new Argument<string>();

                string textToMatch = command.Parse("the-command", "--option1", "a")
                                            .TextToMatch();

                textToMatch.Should().Be("a");
            }

            [Theory]
            [InlineData("the-command $one --two")]
            [InlineData("the-command one$ --two")]
            [InlineData("the-command on$e --two ")]
            [InlineData(" the-command  $one --two ")]
            [InlineData(" the-command  one$ --two ")]
            [InlineData(" the-command  on$e --two ")]
            public void When_position_is_specified_in_string_command_line_then_it_returns_argument_at_cursor_position(string input)
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
}
