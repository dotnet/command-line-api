// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.CommandLine.Suggestions;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class SuggestionTestsGeneric
    {
        private readonly ITestOutputHelper _output;

        public SuggestionTestsGeneric(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Command_GetGenericSuggestions_returns_available_subcommands()
        {
            var command = new Command<SuggestionTestObjectPlain>("command")
            {
                new Command("one"),
                new Command<SuggestionTestObjectPlain>("two"),
                new Command("three")
            };

            var suggestions = command.GetGenericSuggestions();

            suggestions.Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_GetGenericSuggestions_returns_available_subcommands_and_option_aliases()
        {
            var command = new Command<SuggestionTestObjectPlain>("command")
            {
                new Command("subcommand"),
                new Option("--option")
            };

            var suggestions = command.GetGenericSuggestions();

            suggestions.Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo("subcommand", "--option");
        }

        [Fact]
        public void Command_GetGenericSuggestions_returns_available_subcommands_and_option_aliases_and_configured_arguments()
        {
            var command = new Command<SuggestionTestObjectPlain>("command")
            {
                new Command("subcommand", "subcommand"),
                new Option("--option", "option"),
                new Argument
                {
                    Arity = ArgumentArity.OneOrMore,
                    Suggestions = { "command-argument" }
                }
            };

            var suggestions = command.GetGenericSuggestions();

            suggestions.Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo("subcommand", "--option", "command-argument");
        }

        [Fact]
        public void Command_Getsuggestions_without_text_to_match_orders_based_on_compareto()
        {
            var command = new Command<SuggestionTestObjectPlain>("command")
            {
                new Command("A"),
                new Command("b"),
                new Command("C"),
                new Command("bb"),
            };
            var suggestions = command.GetGenericSuggestions();
            suggestions.Select(obj => obj.Suggestion)
                .Should().BeEquivalentSequenceTo("A", "b", "bb", "C" );

            var commandSuggestionsReversed = new Command<SuggestionTestObjectReverseOrder>("command")
            {
                new Command("A"),
                new Command("b"),
                new Command("C"),
                new Command("bb"),
            };
            var suggestionsReversedOrder = commandSuggestionsReversed.GetGenericSuggestions();
            suggestionsReversedOrder.Select(obj => obj.Suggestion)
                .Should().BeEquivalentSequenceTo("C", "bb", "b", "A");
        }

        [Fact]
        public void Command_Getsuggestions_does_not_return_argument_names()
        {
            var command = new Command<SuggestionTestObjectPlain>("command")
            {
                new Argument("the-argument")
            };

            var suggestions = command.GetGenericSuggestions();

            suggestions.Select(obj => obj.Suggestion).Should().NotContain("the-argument");
        }

        [Fact]
        public void Command_Getsuggestions_with_texttomatch_orders_by_comparetotexttomatch_and_then_compareto()
        {
            var command = new Command<SuggestionTestObjectPlain>("command")
            {
                new Command("andmythirdsubcommand"),
                new Command("mysubcommand"),
                new Command("andmyothersubcommand"),
            };
            var suggestions = command.GetGenericSuggestions("my");
            suggestions.Select(obj => obj.Suggestion)
                .Should().BeEquivalentSequenceTo("mysubcommand", "andmyothersubcommand", "andmythirdsubcommand");
            
            var commandSuggestionReversedOrder = new Command<SuggestionTestObjectReverseOrder>("command")
            {
                new Command("andmythirdsubcommand"),
                new Command("mysubcommand"),
                new Command("andmyothersubcommand"),
            };
            var suggestionsReversedOrder = commandSuggestionReversedOrder.GetGenericSuggestions("my");
            suggestionsReversedOrder.Select(obj => obj.Suggestion)
                .Should().BeEquivalentSequenceTo("andmythirdsubcommand", "andmyothersubcommand", "mysubcommand");
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

            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should()
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

            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("test");
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

            result.GetGenericSuggestions<SuggestionTestObjectPlain>(8)
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("test");
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

            result.GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("--banana", "--cherry");
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
                new Command<SuggestionTestObjectPlain>("banana")
                {
                    new Option("--cavendish")
                },
                new Command("cherry")
                {
                    new Option("--rainier")
                }
            };

            var result = rootCommand.Parse("cherry ");

            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().NotContain(new[] { "apple", "banana", "cherry" });
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_commands_aliases_will_not_be_suggested()
        {
            var apple = new Command<SuggestionTestObjectPlain>("apple")
            {
                new Option("--cortland")
            };
            apple.AddAlias("apl");

            var banana = new Command<SuggestionTestObjectPlain>("banana")
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

            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().NotContain(new[] { "apl", "bnn" });
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

            parseResult.GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion).Should().Contain("--parent-option");
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

            parseResult.GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion).Should().NotContain("--parent-option");
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

            parseResult.GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion).Should().Contain("--child-option");
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

            result.GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("rainier");
        }

        [Fact]
        public void When_one_option_has_been_partially_specified_then_nonmatching_siblings_will_not_be_suggested_by_default()
        {
            var command = new Command("the-command")
            {
                new Option("--apple"),
                new Option("--banana"),
                new Option("--cherry")
            };

            var input = "a";
            var result = command.Parse(input);

            result.GetGenericSuggestions<SuggestionTestObjectPlain>(input.Length)
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("--apple", "--banana");
        }

        [Fact]
        public void When_one_option_has_been_partially_specified_then_nonmatching_siblings_can_still_be_suggested_by_disabling_textmatch()
        {
            var command = new Command("the-command", enforceTextMatch: false)
            {
                new Option("--apple"),
                new Option("--banana"),
                new Option("--cherry")
            };

            var input = "a";
            var result = command.Parse(input);

            result.GetGenericSuggestions<SuggestionTestObjectPlain>(input.Length)
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("--apple", "--banana", "--cherry");
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

            command.Parse("the-command ").GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("-n");

            var command2 = new Command<SuggestionTestObjectPlain>("the-command")
            {
                new Option<string, SuggestionTestObjectPlain>("--hide-me", new Argument<string, SuggestionTestObjectPlain>())
                {
                    IsHidden = true
                },
                new Option("-n", "Not hidden")
            };

            command2.Parse("the-command ").GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("-n");
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
            result.GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                  .Select(obj => obj.Suggestion)
                  .Should()
                  .BeEquivalentTo("rye", "sourdough", "wheat");

            commandLine = "--bread wheat --cheese ";
            result = parser.Parse(commandLine);
            result.GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                  .Select(obj => obj.Suggestion)
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
                   .GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                   .Select(obj => obj.Suggestion)
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
                   .GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                   .Select(obj => obj.Suggestion)
                   .Should()
                   .BeEquivalentTo("one", "--one");
        } 

        [Fact]
        public void Command_Getsuggestion_returns_proximate_options()
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

            result.GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo("--one", "--two", "--three");
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

            result.GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo("two-a", "two-b");
        }

        [Theory]
        [InlineData(true, new[] { "one", "two" })]
        [InlineData(false, new[] { "one", "two", "three" })]
        public void Command_Getsuggestions_can_be_based_on_the_proximate_command_and_partial_input(
            bool enforceTextMatch,
            string[] expectedSuggestions)
        {
            var parser = new Parser(
                new Command("outer", enforceTextMatch: enforceTextMatch)
                {
                    new Command("one", "Command one"),
                    new Command("two", "Command two"),
                    new Command("three", "Command three")
                });

            ParseResult result = parser.Parse("outer o");

            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo(expectedSuggestions);
        }

        [Fact]
        public void Suggestions_can_be_provided_in_the_absence_of_validation()
        {
            var command = new Command<SuggestionTestObjectPlain>("the-command")
                {
                    new Option("-t", arity: ArgumentArity.ExactlyOne)
                        .AddSuggestions("vegetable", "mineral", "animal")
                };

            command.Parse("the-command -t m")
                   .GetGenericSuggestions<SuggestionTestObjectPlain>()
                   .Select(obj => obj.Suggestion)
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");

            command.Parse("the-command -t something-else").Errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true, new[] { "animal", "mineral" })]
        [InlineData(false, new[] { "animal", "mineral", "vegetable" })]
        public void Command_argument_generic_suggestions_can_be_provided_using_a_generic_delegate(
            bool enforceTextMatch,
            string[] expectedSuggestions)
        {
            var suggestList = new[] { "vegetable", "mineral", "animal" }
                .Select(suggestion => new SuggestionTestObjectPlain().Build(null, suggestion));

            var arg = new Argument<string, SuggestionTestObjectPlain>
            {
                Arity = ArgumentArity.ExactlyOne
            }.AddSuggestions((_, __) => suggestList);

            var command = new Command("the-command")
            {
                new Command<SuggestionTestObjectPlain>("one", enforceTextMatch: enforceTextMatch){ arg }
            };
            command.Parse("the-command one m")
                   .GetGenericSuggestions<SuggestionTestObjectPlain>()
                   .Select(obj => obj.Suggestion)
                   .Should()
                   .BeEquivalentTo(expectedSuggestions);

            var command2 = new Command<SuggestionTestObjectPlain>("the-command")
            {
                new Command<SuggestionTestObjectPlain>("one", enforceTextMatch: enforceTextMatch){ arg }
            };
            command2.Parse("the-command one m")
                   .GetGenericSuggestions<SuggestionTestObjectPlain>()
                   .Select(obj => obj.Suggestion)
                   .Should()
                   .BeEquivalentTo(expectedSuggestions);
        }

        [Theory]
        [InlineData(true, new[] { "animal", "mineral" })]
        [InlineData(false, new[] { "animal", "mineral", "vegetable" })]
        public void Option_argument_suggestions_can_be_provided_using_a_delegate(
            bool enforceTextMatch,
            string[] expectedSuggestions)
        {
            var suggestList = new[] { "vegetable", "mineral", "animal" }
                .Select(suggestion => new SuggestionTestObjectPlain().Build(null, suggestion));

            var command = new Command<SuggestionTestObjectPlain>("the-command")
            {
                new Option<string, SuggestionTestObjectPlain>(
                    "-x", 
                    new Argument<string, SuggestionTestObjectPlain>()
                        .AddSuggestions((_, _) => suggestList),
                    enforceTextMatch: enforceTextMatch)
            };

            var parseResult = command.Parse("the-command -x m");

            parseResult.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo(expectedSuggestions);

            var command2 = new Command("the-command")
            {
                new Option<string, SuggestionTestObjectPlain>(
                    "-x",
                    new Argument<string, SuggestionTestObjectPlain>()
                        .AddSuggestions((_, _) => suggestList),
                    enforceTextMatch: enforceTextMatch)
            };

            var parseResult2 = command2.Parse("the-command -x m");

            parseResult2.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo(expectedSuggestions);
        }

        [Theory]
        [InlineData(true, new[] { "two-b" })]
        [InlineData(false, new[] { "two-a", "two-b", "two-c" })]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option(
            bool enforceTextMatchForTwo,
            string[] expectedSuggestionsForTwo)
        {
            var command = new Command<SuggestionTestObjectPlain>("outer")
            {
                new Option("one", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("one-a", "one-b", "one-c"),
                new Option("two", arity: ArgumentArity.ExactlyOne, enforceTextMatch: enforceTextMatchForTwo)
                    .FromAmong("two-a", "two-b", "two-c"),
                new Option("three", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("three-a", "three-b", "three-c")
            };

            var parser = new CommandLineBuilder()
                         .AddCommand(command)
                         .Build();

            var result = parser.Parse("outer two b");
            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo(expectedSuggestionsForTwo);

            var result2 = parser.Parse("outer three c");
            result2.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("three-c");

        }

        [Fact]
        public void When_caller_does_the_tokenizing_option_from_among_suggestions_nonmatching_can_still_show_by_disabling_textmatch()
        {
            var command = new Command<SuggestionTestObjectPlain>("outer")
            {
                new Option("one", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("one-a", "one-b", "one-c"),
                new Option("two", arity: ArgumentArity.ExactlyOne, enforceTextMatch: false)
                    .FromAmong("two-a", "two-b", "two-c"),
                new Option("three", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("three-a", "three-b", "three-c")
            };

            var parser = new CommandLineBuilder()
                         .AddCommand(command)
                         .Build();

            var result = parser.Parse("outer two b");
            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("two-a", "two-b", "two-c");

            var result2 = parser.Parse("outer three c");
            result2.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("three-c");
        }

        [Theory]
        [InlineData(true, new[] { "two-b" })]
        [InlineData(false, new[] { "two-a", "two-b", "two-c" })]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option(
            bool enforceTextMatchForTwo,
            string[] expectedSuggestionsForTwo)
        {
            var command = new Command<SuggestionTestObjectPlain>("outer")
            {
                new Option("one", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("one-a", "one-b", "one-c"),
                new Option("two", arity: ArgumentArity.ExactlyOne, enforceTextMatch: enforceTextMatchForTwo)
                    .FromAmong("two-a", "two-b", "two-c"),
                new Option("three", arity: ArgumentArity.ExactlyOne)
                    .FromAmong("three-a", "three-b", "three-c")
            };

            var result = command.Parse("outer two b");
            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo(expectedSuggestionsForTwo);

            var result2 = command.Parse("outer three c");
            result2.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("three-c");
        }

        [Theory]
        [InlineData(true, new[] { "two-b" })]
        [InlineData(false, new[] { "two-a", "two-b", "two-c" })]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command(
            bool enforceTextMatchForTwo,
            string[] expectedSuggestionsForTwo)
        {
            var outer = new Command<SuggestionTestObjectPlain>("outer")
            {
                new Command<SuggestionTestObjectPlain>("one")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }.FromAmong("one-a", "one-b", "one-c")
                },
                new Command<SuggestionTestObjectPlain>("two", enforceTextMatch: enforceTextMatchForTwo)
                {
                    new Argument<string, SuggestionTestObjectPlain>
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

            var parser = new CommandLineBuilder().AddCommand(outer).Build();

            var result = parser.Parse("outer one a");
            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("one-a");

            result = parser.Parse("outer two b");
            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo(expectedSuggestionsForTwo);

            result = parser.Parse("outer three c");
            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("three-c");
        }

        [Theory]
        [InlineData(true, new[] { "two-b" })]
        [InlineData(false, new[] { "two-a", "two-b", "two-c" })]
        public void When_caller_does_not_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command(
            bool enforceTextMatchForTwo,
            string[] expectedSuggestionsForTwo)
        {
            var outer = new Command<SuggestionTestObjectPlain>("outer")
            {
                new Command<SuggestionTestObjectPlain>("one")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }.FromAmong("one-a", "one-b", "one-c")
                },
                new Command<SuggestionTestObjectPlain>("two", enforceTextMatch: enforceTextMatchForTwo)
                {
                    new Argument<string, SuggestionTestObjectPlain>
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

            ParseResult result = outer.Parse("outer one a");
            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("one-a");

            result = outer.Parse("outer two b");
            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo(expectedSuggestionsForTwo);

            result = outer.Parse("outer three c");
            result.GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("three-c");
        }

        [Theory]
        [InlineData(true, new[] { "CreateNew", "Create", "OpenOrCreate" })]
        [InlineData(false, new[] { "CreateNew", "Create", "Open", "OpenOrCreate", "Truncate", "Append" })]
        public void Arguments_of_type_enum_provide_enum_values_as_suggestions(
            bool enforceTextMatch,
            string[] expectedSuggestions)
        {
            var command = new Command("the-command", enforceTextMatch: enforceTextMatch)
            {
                new Argument<FileMode>()
            };
            command.Parse("the-command create")
                .GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo(expectedSuggestions);

            var command2 = new Command<SuggestionTestObjectPlain>("the-command", enforceTextMatch: enforceTextMatch)
            {
                new Argument<FileMode, SuggestionTestObjectPlain>()
            };
            command2.Parse("the-command create")
                .GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo(expectedSuggestions);

            var command3 = new Command<SuggestionTestObjectPlain>("the-command", enforceTextMatch: enforceTextMatch)
            {
                new Argument<FileMode>()
            };
            command3.Parse("the-command create")
                .GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo(expectedSuggestions);
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
            command.Parse(commandLine)
                .GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo("--allows-many");

            var command2 = new Command<SuggestionTestObjectPlain>("command")
            {
                new Option<string>("--allows-one"),
                new Option<string[]>("--allows-many")
            };
            command2.Parse(commandLine)
                .GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo("--allows-many");

            var command3 = new Command<SuggestionTestObjectPlain>("command")
            {
                new Option<string, SuggestionTestObjectPlain>(
                    "--allows-one",
                    new Argument<string>()
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }),
                new Option<string[], SuggestionTestObjectPlain>(
                    "--allows-many",
                    new Argument<string[]>()
                    {
                        Arity = ArgumentArity.OneOrMore
                    })
            };
            command3.Parse(commandLine)
                .GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo("--allows-many");

            var command4 = new Command<SuggestionTestObjectPlain>("command")
            {
                new Option<string, SuggestionTestObjectPlain>(
                    "--allows-one",
                    new Argument<string, SuggestionTestObjectPlain>()
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }),
                new Option<string[], SuggestionTestObjectPlain>(
                    "--allows-many",
                    new Argument<string[], SuggestionTestObjectPlain>()
                    {
                        Arity = ArgumentArity.OneOrMore
                    })
            };
            command4.Parse(commandLine)
                .GetGenericSuggestions<SuggestionTestObjectPlain>(commandLine.Length + 1)
                .Select(obj => obj.Suggestion)
                .Should().BeEquivalentTo("--allows-many");
        }

        [Fact]
        public void When_current_symbol_is_an_option_that_requires_arguments_then_parent_symbol_suggestions_are_omitted()
        {
            var parser = new CommandLineBuilder()
                         .AddOption(new Option<string>("--allows-one"))
                         .AddOption(new Option<string[]>("--allows-many"))
                         .UseSuggestDirective()
                         .Build();

            var suggestions = parser.Parse("--allows-one ")
                .GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEmpty();
        }

        [Fact]
        public void Option_substring_matching_when_arguments_have_default_values()
        {
            var command = new Command("the-command")
            {
                new Option<string>("--implicit", () => "the-default"),
                new Option<string>("--not", () => "the-default")
            };

            command.Parse("m").GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("--implicit");

            var command2 = new Command<SuggestionTestObjectPlain>("the-command")
            {
                new Option<string, SuggestionTestObjectPlain>(
                    "--implicit",
                    new Argument<string, SuggestionTestObjectPlain>(() => "the-default")),
                new Option<string, SuggestionTestObjectPlain>(
                    "--not",
                    new Argument<string, SuggestionTestObjectPlain>(() => "the-default"))
            };
            command2.Parse("m").GetGenericSuggestions<SuggestionTestObjectPlain>()
                .Select(obj => obj.Suggestion).Should().BeEquivalentTo("--implicit");
        }
    }
}
