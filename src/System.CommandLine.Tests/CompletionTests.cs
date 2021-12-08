// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class CompletionTests
    {
        private readonly ITestOutputHelper _output;

        public CompletionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Option_GetSuggestions_returns_argument_suggestions_if_configured()
        {
            var option = new Option("--hello", arity: ArgumentArity.ExactlyOne)
                .AddCompletions("one", "two", "three");

            var completions = option.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("one", "two", "three");
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

            var completions = command.GetCompletions();

            suggestions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("--one", "--two", "--three");
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

            var completions = command.GetCompletions();

            suggestions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_GetSuggestions_returns_available_subcommands_and_option_aliases()
        {
            var command = new Command("command")
            {
                new Command("subcommand"),
                new Option("--option")
            };

            var suggestions = command.GetCompletions();

            suggestions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("subcommand", "--option");
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
                    Completions = { "command-argument" }
                }
            };

            var suggestions = command.GetCompletions();

            suggestions.Select(item => item.Label)
                       .Should()
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

            var suggestions = command.GetCompletions();

            suggestions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentSequenceTo("andmyothersubcommand", "andmythirdsubcommand", "mysubcommand");
        }

        [Fact]
        public void Command_GetSuggestions_does_not_return_argument_names()
        {
            var command = new Command("command")
            {
                new Argument("the-argument")
            };

            var suggestions = command.GetCompletions();

            suggestions
                .Select(item => item.Label)
                .Should()
                .NotContain("the-argument");
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

            var suggestions = command.Parse("my").GetCompletions();

            suggestions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentSequenceTo("mysubcommand", "andmyothersubcommand", "andmythirdsubcommand");
        }

        [Fact]
        public void When_an_option_has_a_default_value_it_will_still_be_suggested()
        {
            var parser = new RootCommand
            {
                new Option<string>("--apple", getDefaultValue: () => "cortland"),
                new Option<string>("--banana"),
                new Option<string>("--cherry")
            };

            var result = parser.Parse("");

            _output.WriteLine(result.ToString());

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("--apple",
                                  "--banana",
                                  "--cherry");
        }

        [Fact]
        public void Command_GetSuggestions_can_access_ParseResult()
        {
            var originOption = new Option<string>("--origin");

            var parser = new Parser(
                new RootCommand
                {
                    originOption,
                    new Option<string>("--clone")
                        .AddCompletions(ctx =>
                        {
                            var opt1Value = ctx.ParseResult.GetValueForOption(originOption);
                            return !string.IsNullOrWhiteSpace(opt1Value) ? new[] { opt1Value } : Array.Empty<string>();
                        })
                });

            var result = parser.Parse("--origin test --clone ");

            _output.WriteLine(result.ToString());

            result.GetCompletions()
                  .Select(item => item.Label)
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

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
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

            result.GetCompletions()
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

            result.GetCompletions()
                  .Select(item => item.Label)
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
                .GetCompletions(commandLine.Length + 1)
                .Select(item => item.Label)
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
                .GetCompletions(commandLine.Length + 1)
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
                .GetCompletions(commandLine.Length + 1)
                .Select(item => item.Label)
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

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
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

            result.GetCompletions(input.Length)
                  .Select(item => item.Label)
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

            var suggestions = command.Parse("the-command ").GetCompletions();

            suggestions.Should().NotContain("--hide-me");
        }

        [Fact]
        public void Parser_options_can_supply_context_sensitive_matches()
        {
            var parser = new RootCommand
            {
                new Option("--bread", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("wheat", "sourdough", "rye"),
                new Option("--cheese", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("provolone", "cheddar", "cream cheese")
            };

            var commandLine = "--bread";
            var result = parser.Parse(commandLine);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("rye", "sourdough", "wheat");

            commandLine = "--bread wheat --cheese ";
            result = parser.Parse(commandLine);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
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
                   .GetCompletions(commandLine.Length + 1)
                   .Select(item => item.Label)
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
                   .GetCompletions(commandLine.Length + 1)
                   .Select(item => item.Label)
                   .Should()
                   .BeEquivalentTo("one", "--one");
        }

        [Theory(Skip = "Needs discussion, Issue #19")]
        [InlineData("outer ")]
        [InlineData("outer -")]
        public void Option_GetSuggestions_are_not_provided_without_matching_prefix(string input)
        {
            var command = new Command("outer")
            {
                new Option("--one"),
                new Option("--two"),
                new Option("--three")
            };

            var parser = new Parser(command);

            ParseResult result = parser.Parse(input);
            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void Option_GetSuggestions_can_be_based_on_the_proximate_option()
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

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("--one", "--two", "--three");
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

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_GetSuggestionsions_can_be_based_on_the_proximate_option_and_partial_input()
        {
            var parser = new Parser(
                new Command("outer")
                {
                    new Command("one", "Command one"),
                    new Command("two", "Command two"),
                    new Command("three", "Command three")
                });

            ParseResult result = parser.Parse("outer o");

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Suggestions_can_be_provided_in_the_absence_of_validation()
        {
            var command = new Command("the-command")
                {
                    new Option("-t", arity: ArgumentArity.ExactlyOne)
                        .AddCompletions("vegetable", "mineral", "animal")
                };

            command.Parse("the-command -t m")
                   .GetCompletions()
                   .Select(item => item.Label)
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");

            command.Parse("the-command -t something-else")
                   .Errors
                   .Should()
                   .BeEmpty();
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
                            Completions = { _ => new[] { "vegetable", "mineral", "animal" } }
                        }
                }
            };

            command.Parse("the-command one m")
                   .GetCompletions()
                   .Select(item => item.Label)
                   .Should()
                   .BeEquivalentTo("animal", "mineral");
        }

        [Fact]
        public void Option_argument_suggestions_can_be_provided_using_a_delegate()
        {
            var command = new Command("the-command")
            {
                new Option<string>("-x")
                    .AddCompletions(_ => new [] { "vegetable", "mineral", "animal" })
            };

            var parseResult = command.Parse("the-command -x m");

            parseResult
                   .GetCompletions()
                   .Select(item => item.Label)
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

            var parser = new CommandLineBuilder(new RootCommand
                         {
                             command
                         })
                         .Build();

            var result = parser.Parse("outer two b" );

            result.GetCompletions()
                  .Select(item => item.Label)
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

            result.GetCompletions()
                  .Select(item => item.Label)
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

            result.GetCompletions()
                  .Select(item => item.Label)
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

            result.GetCompletions()
                  .Select(item => item.Label)
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
                                     .GetCompletions();

            suggestions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("CreateNew", "Create", "OpenOrCreate");
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
            var suggestions = command.Parse(commandLine).GetCompletions(commandLine.Length + 1);

            suggestions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("--allows-many");
        }

        [Fact]
        public void When_current_symbol_is_an_option_that_requires_arguments_then_parent_symbol_suggestions_are_omitted()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             new Option<string>("--allows-one"),
                             new Option<string[]>("--allows-many")
                         })
                         .UseSuggestDirective()
                         .Build();

            var suggestions = parser.Parse("--allows-one ").GetCompletions();

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

            var suggestions = command.Parse("m").GetCompletions();

            suggestions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("--implicit");
        }

        [Theory(Skip = "work in progress")]
        [InlineData("#r \"nuget: ", 11)]
        [InlineData("#r \"nuget:", 10)]
        public void It_can_provide_completions_within_quotes(string commandLine, int position)
        {
            // FIX: (testname) make this test reflect the scenario correctly
            var expectedSuggestions = new[]
            {
                "\"nuget:NewtonSoft.Json\"",
                "\"nuget:Spectre.Console\"",
                "\"nuget:Microsoft.DotNet.Interactive\""
            };

            var argument = new Argument<string>()
                .AddCompletions(expectedSuggestions);

            var r = new Command("#r")
            {
                argument
            };

            var suggestions = r.Parse(commandLine).GetCompletions(position);

            suggestions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo(expectedSuggestions);

            throw new NotImplementedException();
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

            [Fact]
            public void Enum_suggestions_can_be_configured_with_list_clear()
            {
                var argument = new Argument<DayOfWeek?>();
                argument.Completions.Clear();
                argument.Completions.Add(new[] { "mon", "tues", "wed", "thur", "fri", "sat", "sun" });
                var command = new Command("the-command")
                {
                    argument
                };

                var suggestions = command.Parse("the-command s")
                                         .GetCompletions();

                suggestions.Select(item => item.Label)
                           .Should()
                           .BeEquivalentTo("sat", "sun","tues");
            }

            [Fact]
            public void Enum_suggestions_can_be_configured_without_list_clear()
            {
                var command = new Command("the-command")
                {
                    new Argument<DayOfWeek?>
                    {
                        Completions = { "mon", "tues", "wed", "thur", "fri", "sat", "sun" }
                    }
                };

                var suggestions = command.Parse("the-command s")
                                         .GetCompletions();

                suggestions
                    .Select(item => item.Label)
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
