// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
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
        public void Option_GetSuggestions_returns_argument_suggestions_if_configured()
        {
            var option = new Option("--hello", arity: ArgumentArity.ExactlyOne)
                .AddSuggestions("one", "two", "three");

            var suggestions = option.GetSuggestions();

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_GetSuggestions_returns_available_option_aliases()
        {
            IReadOnlyCollection<Symbol> symbols = new[] {
                new Option("--one", "option one"),
                new Option("--two", "option two"),
                new Option("--three", "option three")
            };
            var command1 = new Command(
                "command",
                "a command"
            );

            foreach (var symbol in symbols)
            {
                command1.Add(symbol);
            }

            var command = command1;

            var suggestions = command.GetSuggestions();

            suggestions.Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Command_GetSuggestions_returns_available_subcommands()
        {
            var command = new Command("command")
            {
                new Command("one"),
                new Command("two"),
                new Command("three")
            };

            var suggestions = command.GetSuggestions();

            suggestions.Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_GetSuggestions_returns_available_subcommands_and_option_aliases()
        {
            var command = new Command("command")
            {
                new Command("subcommand"),
                new Option("--option")
            };

            var suggestions = command.GetSuggestions();

            suggestions.Should().BeEquivalentTo("subcommand", "--option");
        }

        [Fact]
        public void Command_GetSuggestions_returns_available_subcommands_and_option_aliases_and_configured_arguments()
        {
            var command = new Command("command")
            {
                new Command("subcommand", "subcommand"),
                new Option("--option", "option"),
                new Argument
                {
                    Arity = ArgumentArity.OneOrMore,
                    Suggestions = { "command-argument" }
                }
            };

            var suggestions = command.GetSuggestions();

            suggestions.Should()
                       .BeEquivalentTo("subcommand", "--option", "command-argument");
        }

        [Fact]
        public void Command_GetSuggestions_without_text_to_match_orders_alphabetically()
        {
            var command = new Command("command")
            {
                new Command("andmythirdsubcommand"),
                new Command("mysubcommand"),
                new Command("andmyothersubcommand"),
            };

            var suggestions = command.GetSuggestions();

            suggestions.Should().BeEquivalentSequenceTo("andmyothersubcommand", "andmythirdsubcommand", "mysubcommand");
        }

        [Fact]
        public void Command_GetSuggestions_does_not_return_argument_names()
        {
            var command = new Command("command")
            {
                new Argument("the-argument")
            };

            var suggestions = command.GetSuggestions();

            suggestions.Should().NotContain("the-argument");
        }

        [Fact]
        public void Command_GetSuggestions_with_text_to_match_orders_by_match_position_then_alphabetically()
        {
            var command = new Command("command")
            {
                new Command("andmythirdsubcommand"),
                new Command("mysubcommand"),
                new Command("andmyothersubcommand"),
            };

            var suggestions = command.GetSuggestions("my");

            suggestions.Should().BeEquivalentSequenceTo("mysubcommand", "andmyothersubcommand", "andmythirdsubcommand");
        }

        [Fact]
        public void When_an_option_has_a_default_value_it_will_still_be_suggested()
        {
            var parser = new Parser(
                new Option<string>("--apple", getDefaultValue: () => "cortland"),
                new Option<string>("--banana"),
                new Option<string>("--cherry"));

            var result = parser.Parse("");

            _output.WriteLine(result.ToString());

            result.GetSuggestions()
                  .Should()
                  .BeEquivalentTo("--apple",
                                  "--banana",
                                  "--cherry");
        }

        [Fact]
        public void Command_Getsuggestions_can_access_ParseResult()
        {
            var parser = new Parser(
                new Option<string>("--origin"),
                new Option<string>("--clone")
                .AddSuggestions((parseResult, match) =>
                {
                    var opt1Value = parseResult?.ValueForOption<string>("--origin");
                    return opt1Value != null ? new[] { opt1Value } : Array.Empty<string>();
                }));

            var result = parser.Parse("--origin test --clone ");

            _output.WriteLine(result.ToString());

            result.GetSuggestions()
                  .Should()
                  .BeEquivalentTo("test");
        }

        [Fact]
        public void Command_Getsuggestions_can_access_ParseResult_reverse_order()
        {
            var parser = new Parser(
                new Option<string>("--origin"),
                new Option<string>("--clone")
                .AddSuggestions((parseResult, match) =>
                {
                    var opt1Value = parseResult?.ValueForOption<string>("--origin");
                    return opt1Value != null ? new[] { opt1Value } : Array.Empty<string>();
                }));

            var result = parser.Parse("--clone  --origin test");

            _output.WriteLine(result.ToString());

            result.GetSuggestions(8)
                  .Should()
                  .BeEquivalentTo("test");
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

            var commandLine = "--apple grannysmith";
            var result = parser.Parse(commandLine);

            result.GetSuggestions(commandLine.Length + 1)
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

            result.GetSuggestions()
                  .Should()
                  .NotContain(new[]{"apple", "banana", "cherry"});
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_commands_aliases_will_not_be_suggested()
        {
            var apple = new Command("apple")
            {
                new Option("--cortland")
            };
            apple.AddAlias("apl");

            var banana = new Command("banana")
            {
                new Option("--cavendish")
            };
            banana.AddAlias("bnn");

            var rootCommand = new RootCommand
            {
                apple,
                banana
            };

            var result = rootCommand.Parse("banana ");

            result.GetSuggestions()
                  .Should()
                  .NotContain(new[] { "apl", "bnn" });
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_options_will_be_suggested()
        {
            var command = new RootCommand("parent")
            {
                new Command("child"), 
                new Option("--parent-option"), 
                new Argument<string>()
            };

            var commandLine = "child";
            var parseResult = command.Parse(commandLine);

            parseResult
                .GetSuggestions(commandLine.Length + 1)
                .Should()
                .Contain("--parent-option");
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_options_with_argument_limit_reached_will_be_not_be_suggested()
        {
            var command = new RootCommand("parent")
            {
                new Command("child"),
                new Option<string>("--parent-option"),
                new Argument<string>()
            };

            var commandLine = "--parent-option 123 child";
            var parseResult = command.Parse(commandLine);

            parseResult
                .GetSuggestions(commandLine.Length + 1)
                .Should()
                .NotContain("--parent-option");
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_child_options_will_be_suggested()
        {
            var command = new RootCommand("parent")
            {
                new Argument<string>(),
                new Command("child")
                {
                    new Option<string>("--child-option")
                }
            };

            var commandLine = "child ";
            var parseResult = command.Parse(commandLine);

            parseResult
                .GetSuggestions(commandLine.Length + 1)
                .Should()
                .Contain("--child-option");
        }

        [Fact]
        public void When_a_subcommand_with_subcommands_has_been_specified_then_its_sibling_commands_will_not_be_suggested()
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

            var commandLine = "cherry";
            var result = rootCommand.Parse(commandLine);

            result.GetSuggestions(commandLine.Length + 1)
                  .Should()
                  .BeEquivalentTo("rainier");
        }

        [Fact]
        public void When_one_option_has_been_partially_specified_then_nonmatching_siblings_will_not_be_suggested()
        {
            var command = new Command("the-command")
            {
                new Option("--apple"),
                new Option("--banana"),
                new Option("--cherry")
            };

            var input = "a";
            var result = command.Parse(input);

            result.GetSuggestions(input.Length)
                  .Should()
                  .BeEquivalentTo("--apple",
                                  "--banana");
        }

        [Fact]
        public void An_option_can_be_hidden_from_suggestions_by_setting_IsHidden_to_true()
        {
            var command = new Command("the-command")
            {
                new Option("--hide-me")
                {
                    IsHidden = true
                },
                new Option("-n", "Not hidden")
            };

            var suggestions = command.Parse("the-command ").GetSuggestions();

            suggestions.Should().NotContain("--hide-me");
        }

        [Fact]
        public void Parser_options_can_supply_context_sensitive_matches()
        {
            var parser = new Parser(
                new Option("--bread", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("wheat", "sourdough", "rye"),
                new Option("--cheese", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("provolone", "cheddar", "cream cheese"));

            var commandLine = "--bread";
            var result = parser.Parse(commandLine);

            result.GetSuggestions(commandLine.Length + 1)
                  .Should()
                  .BeEquivalentTo("rye", "sourdough", "wheat");

            commandLine = "--bread wheat --cheese ";
            result = parser.Parse(commandLine);

            result.GetSuggestions(commandLine.Length + 1)
                  .Should()
                  .BeEquivalentTo("cheddar", "cream cheese", "provolone");
        }

        [Fact]
        public void Subcommand_names_are_available_as_suggestions()
        {
            var command = new Command("test")
            {
                new Command("one", "Command one"),
                new Command("two", "Command two"),
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };

            var commandLine = "test";
            command.Parse(commandLine)
                   .GetSuggestions(commandLine.Length + 1)
                   .Should()
                   .BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Both_subcommands_and_options_are_available_as_suggestions()
        {
            var command = new Command("test")
            {
                new Command("one"),
                new Option("--one"),
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };

            var commandLine = "test";

            command.Parse(commandLine)
                   .GetSuggestions(commandLine.Length + 1)
                   .Should()
                   .BeEquivalentTo("one", "--one");
        }

        [Theory(Skip = "Needs discussion, Issue #19")]
        [InlineData("outer ")]
        [InlineData("outer -")]
        public void Option_Getsuggestionsions_are_not_provided_without_matching_prefix(string input)
        {
            var command = new Command("outer")
            {
                new Option("--one"),
                new Option("--two"),
                new Option("--three")
            };

            var parser = new Parser(command);

            ParseResult result = parser.Parse(input);
            result.GetSuggestions().Should().BeEmpty();
        }

        [Fact]
        public void Option_Getsuggestionsions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                new Command("outer")
                {
                    new Option("--one"),
                    new Option("--two"),
                    new Option("--three")
                });

            var commandLine = "outer";
            ParseResult result = parser.Parse(commandLine);

            result.GetSuggestions(commandLine.Length + 1).Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Argument_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                new Command("outer")
                {
                    new Option("--one", arity: ArgumentArity.ExactlyOne)
                        .FromAmong("one-a", "one-b"),
                    new Option("--two", arity: ArgumentArity.ExactlyOne)
                        .FromAmong("two-a", "two-b")
                });

            var commandLine = "outer --two";
            ParseResult result = parser.Parse(commandLine);

            result.GetSuggestions(commandLine.Length + 1).Should().BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_Getsuggestionsions_can_be_based_on_the_proximate_option_and_partial_input()
        {
            var parser = new Parser(
                new Command("outer")
                {
                    new Command("one", "Command one"),
                    new Command("two", "Command two"),
                    new Command("three", "Command three")
                });

            ParseResult result = parser.Parse("outer o");

            result.GetSuggestions().Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Suggestions_can_be_provided_in_the_absence_of_validation()
        {
            var command = new Command("the-command")
                {
                    new Option("-t", arity: ArgumentArity.ExactlyOne)
                        .AddSuggestions("vegetable", "mineral", "animal")
                };

            command.Parse("the-command -t m")
                   .GetSuggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");

            command.Parse("the-command -t something-else").Errors.Should().BeEmpty();
        }

        [Fact]
        public void Command_argument_suggestions_can_be_provided_using_a_delegate()
        {
            var command = new Command("the-command")
            {
                new Command("one")
                {
                    new Argument
                        {
                            Arity = ArgumentArity.ExactlyOne,
                            Suggestions = { (_, __) => new[] { "vegetable", "mineral", "animal" } }
                        }
                }
            };

            command.Parse("the-command one m")
                   .GetSuggestions()
                   .Should()
                   .BeEquivalentTo("animal", "mineral");
        }

        [Fact]
        public void Option_argument_suggestions_can_be_provided_using_a_delegate()
        {
            var command = new Command("the-command")
            {
                new Option<string>("-x")
                    .AddSuggestions((_, __) => new [] { "vegetable", "mineral", "animal" })
            };

            var parseResult = command.Parse("the-command -x m");

            parseResult
                   .GetSuggestions()
                   .Should()
                   .BeEquivalentTo("animal", "mineral");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var command = new Command("outer")
            {
                new Option("one", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("one-a", "one-b", "one-c"),
                new Option("two", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("two-a", "two-b", "two-c"),
                new Option("three", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("three-a", "three-b", "three-c")
            };

            var parser = new CommandLineBuilder()
                         .AddCommand(command)
                         .Build();

            var result = parser.Parse("outer two b" );

            result.GetSuggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var command = new Command("outer")
            {
                new Option("one", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("one-a", "one-b", "one-c"),
                new Option("two", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("two-a", "two-b", "two-c"),
                new Option("three", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("three-a", "three-b", "three-c")
            };

            var result = command.Parse("outer two b");

            result.GetSuggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var outer = new Command("outer")
            {
                new Command("one")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }.FromAmong("one-a", "one-b", "one-c")
                },
                new Command("two")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }.FromAmong("two-a", "two-b", "two-c")
                },
                new Command("three")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }.FromAmong("three-a", "three-b", "three-c")
                }
            };

            var result = outer.Parse("outer two b");

            result.GetSuggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var outer = new Command("outer")
            {
                new Command("one")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }.FromAmong("one-a", "one-b", "one-c")
                },
                new Command("two")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }.FromAmong("two-a", "two-b", "two-c")
                },
                new Command("three")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }.FromAmong("three-a", "three-b", "three-c")
                }
            };

            ParseResult result = outer.Parse("outer two b");

            result.GetSuggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void Arguments_of_type_enum_provide_enum_values_as_suggestions()
        {
            var command = new Command("the-command")
            {
                new Argument<FileMode>()
            };

            var suggestions = command.Parse("the-command create")
                                     .GetSuggestions();

            suggestions.Should().BeEquivalentTo("CreateNew", "Create", "OpenOrCreate");
        }

        [Fact]
        public void Options_that_have_been_specified_to_their_maximum_arity_are_not_suggested()
        {
            var command = new Command("command")
            {
                new Option<string>("--allows-one"),
                new Option<string[]>("--allows-many")
            };

            var commandLine = "--allows-one x";
            var suggestions = command.Parse(commandLine).GetSuggestions(commandLine.Length + 1);

            suggestions.Should().BeEquivalentTo("--allows-many");
        }

        [Fact]
        public void When_current_symbol_is_an_option_that_requires_arguments_then_parent_symbol_suggestions_are_omitted()
        {
            var parser = new CommandLineBuilder()
                         .AddOption(new Option<string>("--allows-one"))
                         .AddOption(new Option<string[]>("--allows-many"))
                         .UseSuggestDirective()
                         .Build();

            var suggestions = parser.Parse("--allows-one ").GetSuggestions();

            suggestions.Should().BeEmpty();
        }

        [Fact]
        public void Option_substring_matching_when_arguments_have_default_values()
        {
            var command = new Command("the-command")
            {
                new Option<string>("--implicit", () => "the-default"),
                new Option<string>("--not", () => "the-default")
            };

            var suggestions = command.Parse("m").GetSuggestions();

            suggestions.Should().BeEquivalentTo("--implicit");
        }

        public class TextToMatch
        {
            [Fact]
            public void When_position_is_unspecified_in_string_command_line_not_ending_with_a_space_then_it_returns_final_token()
            {
                IReadOnlyCollection<Symbol> symbols = new[]
                {
                    new Option("--option1"),
                    new Option("--option2")
                };
                var command1 = new Command(
                    "the-command",
                    ""
                );

                foreach (var symbol in symbols)
                {
                    command1.Add(symbol);
                }

                var command = command1;

                string textToMatch = command.Parse("the-command t")
                                            .TextToMatch();

                textToMatch.Should().Be("t");
            }

            [Fact]
            public void When_position_is_unspecified_in_string_command_line_ending_with_a_space_then_it_returns_empty()
            {
                IReadOnlyCollection<Symbol> symbols = new[]
                {
                    new Option("--option1"),
                    new Option("--option2")
                };
                var command1 = new Command(
                    "the-command",
                    ""
                );

                foreach (var symbol in symbols)
                {
                    command1.Add(symbol);
                }

                Command command = command1;

                var commandLine = "the-command t";
                string textToMatch = command.Parse(commandLine)
                                            .TextToMatch(commandLine.Length + 1);

                textToMatch.Should().Be("");
            }

            [Fact]
            public void When_position_is_greater_than_input_length_in_a_string_command_line_then_it_returns_empty()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>(),
                    new Option<string>("--option1").FromAmong("apple", "banana", "cherry", "durian"),
                    new Option<string>("--option2")
                };

                var textToMatch = command.Parse("the-command --option1 a")
                                         .TextToMatch(1000);

                textToMatch.Should().Be("");
            }

            [Fact]
            public void When_position_is_unspecified_in_array_command_line_and_final_token_is_unmatched_then_it_returns_final_token()
            {
                IReadOnlyCollection<Symbol> symbols = new[]
                {
                    new Option("--option1"),
                    new Option("--option2")
                };
                var command1 = new Command(
                    "the-command",
                    ""
                );

                foreach (var symbol in symbols)
                {
                    command1.Add(symbol);
                }

                var command = command1;

                string textToMatch = command.Parse("the-command", "opt")
                                            .TextToMatch();

                textToMatch.Should().Be("opt");
            }

            [Fact]
            public void When_position_is_unspecified_in_array_command_line_and_final_token_matches_an_command_then_it_returns_empty()
            {
                IReadOnlyCollection<Symbol> symbols = new[]
                {
                    new Option("--option1"),
                    new Option("--option2")
                };
                var command1 = new Command(
                    "the-command",
                    ""
                );

                foreach (var symbol in symbols)
                {
                    command1.Add(symbol);
                }

                Command command = command1;

                string textToMatch = command.Parse(new[] { "the-command" })
                                            .TextToMatch();

                textToMatch.Should().Be("");
            }

            [Fact]
            public void When_position_is_unspecified_in_array_command_line_and_final_token_matches_an_option_then_it_returns_empty()
            {
                IReadOnlyCollection<Symbol> symbols = new[]
                {
                    new Option("--option1"),
                    new Option("--option2")
                };
                var command1 = new Command(
                    "the-command",
                    ""
                );

                foreach (var symbol in symbols)
                {
                    command1.Add(symbol);
                }

                Command command = command1;

                string textToMatch = command.Parse("the-command", "--option1")
                                            .TextToMatch();

                textToMatch.Should().Be("");
            }
  
            [Fact]
            public void When_position_is_unspecified_in_array_command_line_and_final_token_matches_an_argument_then_it_returns_empty()
            {
                var command = new Command("the-command")
                {
                    new Option<string>("--option1").FromAmong("apple", "banana", "cherry", "durian"),
                    new Option<string>("--option2"),
                    new Argument<string>()
                };

                string textToMatch = command.Parse("the-command", "--option1", "a")
                                            .TextToMatch();

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
                    new Command("the-command")
                    {
                        new Argument
                        {
                            Arity = ArgumentArity.ZeroOrMore
                        }
                    };

                var position = commandLine.IndexOf("$", StringComparison.Ordinal);

                var textToMatch = command.Parse(commandLine.Replace("$", ""))
                                         .TextToMatch(position);

                textToMatch.Should().Be(expected);
            }

            [Fact(Skip = "#310")]
            public void When_there_are_multiple_arguments_then_suggestions_are_only_offered_for_the_current_argument()
            {
                Assert.True(false, "Test testname is not written yet.");
            }

            [Fact]
            public void Enum_suggestions_can_be_configured_with_list_clear()
            {
                var argument = new Argument<DayOfWeek?>();
                argument.Suggestions.Clear();
                argument.Suggestions.Add(new[] { "mon", "tues", "wed", "thur", "fri", "sat", "sun" });
                var command = new Command("the-command")
                {
                    argument
                };

                var suggestions = command.Parse("the-command s")
                                         .GetSuggestions();

                suggestions.Should().BeEquivalentTo("sat", "sun","tues");
            }

            [Fact]
            public void Enum_suggestions_can_be_configured_without_list_clear()
            {
                var command = new Command("the-command")
                {
                    new Argument<DayOfWeek?>
                    {
                        Suggestions = { "mon", "tues", "wed", "thur", "fri", "sat", "sun" }
                    }
                };

                var suggestions = command.Parse("the-command s")
                                         .GetSuggestions();

                suggestions
                    .Should()
                    .BeEquivalentTo(
                        "sat",
                        nameof(DayOfWeek.Saturday),
                        "sun", nameof(DayOfWeek.Sunday),
                        "tues",
                        nameof(DayOfWeek.Tuesday),
                        nameof(DayOfWeek.Thursday),
                        nameof(DayOfWeek.Wednesday));
            }
        }
    }
}
