// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using static System.Environment;

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
        public void Option_GetCompletions_returns_argument_completions_if_configured()
        {
            var option = new Option<string>("--hello")
                .AddCompletions("one", "two", "three");

            var completions = option.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_GetCompletions_returns_available_option_aliases()
        {
            var command = new Command("command")
            {
                new Option<string>("--one", "option one"),
                new Option<string>("--two", "option two"),
                new Option<string>("--three", "option three")
            };

            var completions = command.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1563
        public void Command_GetCompletions_returns_available_option_aliases_for_global_options()
        {
            var subcommand = new Command("command")
            {
                new Option<string>("--one", "option one"),
                new Option<string>("--two", "option two")
            };

            var rootCommand = new RootCommand
            {
                subcommand
            };

            rootCommand.AddGlobalOption(new Option<string>("--three", "option three"));

            var completions = subcommand.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Command_GetCompletions_returns_available_subcommands()
        {
            var command = new Command("command")
            {
                new Command("one"),
                new Command("two"),
                new Command("three")
            };

            var completions = command.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_GetCompletions_returns_available_subcommands_and_option_aliases()
        {
            var command = new Command("command")
            {
                new Command("subcommand"),
                new Option<string>("--option")
            };

            var completions = command.GetCompletions();

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("subcommand", "--option");
        }

        [Fact]
        public void Command_GetCompletions_returns_available_subcommands_and_option_aliases_and_configured_arguments()
        {
            var command = new Command("command")
            {
                new Command("subcommand", "subcommand"),
                new Option<bool>("--option", "option"),
                new Argument<string[]>
                {
                    Arity = ArgumentArity.OneOrMore,
                    Completions = { "command-argument" }
                }
            };

            var completions = command.GetCompletions();

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("subcommand", "--option", "command-argument");
        }

        [Fact]
        public void Command_GetCompletions_without_text_to_match_orders_alphabetically()
        {
            var command = new Command("command")
            {
                new Command("andmythirdsubcommand"),
                new Command("mysubcommand"),
                new Command("andmyothersubcommand"),
            };

            var completions = command.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentSequenceTo("andmyothersubcommand", "andmythirdsubcommand", "mysubcommand");
        }

        [Fact]
        public void Command_GetCompletions_does_not_return_argument_names()
        {
            var command = new Command("command")
            {
                new Argument<string>("the-argument")
            };

            var completions = command.GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .NotContain("the-argument");
        }

        [Fact]
        public void Command_GetCompletions_with_text_to_match_orders_by_match_position_then_alphabetically()
        {
            var command = new Command("command")
            {
                new Command("andmythirdsubcommand"),
                new Command("mysubcommand"),
                new Command("andmyothersubcommand"),
            };

            var completions = command.Parse("my").GetCompletions();

            completions
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
        public void Command_GetCompletions_can_access_ParseResult()
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
                             new Option<string>("--apple"),
                             new Option<string>("--banana"),
                             new Option<string>("--cherry")
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
                    new Option<string>("--cortland")
                },
                new Command("banana")
                {
                    new Option<string>("--cavendish")
                },
                new Command("cherry")
                {
                    new Option<string>("--rainier")
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
                new Option<string>("--cortland")
            };
            apple.AddAlias("apl");

            var banana = new Command("banana")
            {
                new Option<string>("--cavendish")
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

        [Fact] // https://github.com/dotnet/command-line-api/issues/1494
        public void When_a_subcommand_has_been_specified_then_its_sibling_options_will_not_be_suggested()
        {
            var command = new RootCommand("parent")
            {
                new Command("child"), 
                new Option<string>("--parent-option")
            };

            var commandLine = "child";
            var parseResult = command.Parse(commandLine);

            parseResult
                .GetCompletions(commandLine.Length + 1)
                .Select(item => item.Label)
                .Should()
                .NotContain("--parent-option");
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
                new Option<string>("--apple"),
                new Option<string>("--banana"),
                new Option<string>("--cherry")
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
        public void An_option_can_be_hidden_from_completions_by_setting_IsHidden_to_true()
        {
            var command = new Command("the-command")
            {
                new Option<string>("--hide-me")
                {
                    IsHidden = true
                },
                new Option<string>("-n", "Not hidden")
            };

            var completions = command.Parse("the-command ").GetCompletions();

            completions.Select(item => item.Label).Should().NotContain("--hide-me");
        }

        [Fact]
        public void Parser_options_can_supply_context_sensitive_matches()
        {
            var parser = new RootCommand
            {
                new Option<string>("--bread").FromAmong("wheat", "sourdough", "rye"),
                new Option<string>("--cheese").FromAmong("provolone", "cheddar", "cream cheese")
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
                new Argument<string>()
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
                new Option<string>("--one"),
                new Argument<string>()
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
        public void Option_GetCompletions_are_not_provided_without_matching_prefix(string input)
        {
            var command = new Command("outer")
            {
                new Option<string>("--one"),
                new Option<string>("--two"),
                new Option<string>("--three")
            };

            var parser = new Parser(command);

            ParseResult result = parser.Parse(input);
            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void Option_GetCompletions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                new Command("outer")
                {
                    new Option<string>("--one"),
                    new Option<string>("--two"),
                    new Option<string>("--three")
                });

            var commandLine = "outer";
            ParseResult result = parser.Parse(commandLine);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Argument_completions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                new Command("outer")
                {
                    new Option<string>("--one").FromAmong("one-a", "one-b"),
                    new Option<string>("--two").FromAmong("two-a", "two-b")
                });

            var commandLine = "outer --two";
            ParseResult result = parser.Parse(commandLine);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_GetCompletions_can_be_based_on_the_proximate_option_and_partial_input()
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
        public void Completions_can_be_provided_in_the_absence_of_validation()
        {
            var command = new Command("the-command")
                {
                    new Option<string>("-t")
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
        public void Command_argument_completions_can_be_provided_using_a_delegate()
        {
            var command = new Command("the-command")
            {
                new Command("one")
                {
                    new Argument<string>
                        {
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
        public void Option_argument_completions_can_be_provided_using_a_delegate()
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
        public void When_caller_does_the_tokenizing_then_argument_completions_are_based_on_the_proximate_option()
        {
            var command = new Command("outer")
            {
                new Option<string>("one")
                    .FromAmong("one-a", "one-b", "one-c"),
                new Option<string>("two")
                    .FromAmong("two-a", "two-b", "two-c"),
                new Option<string>("three")
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
        public void When_parsing_from_array_then_argument_completions_are_based_on_the_proximate_option()
        {
            var command = new Command("outer")
            {
                new Option<string>("one")
                    .FromAmong("one-a", "one-b", "one-c"),
                new Option<string>("two")
                    .FromAmong("two-a", "two-b", "two-c"),
                new Option<string>("three")
                    .FromAmong("three-a", "three-b", "three-c")
            };

            var result = command.Parse("outer two b");

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_parsing_from_text_then_argument_completions_are_based_on_the_proximate_command()
        {
            var outer = new Command("outer")
            {
                new Command("one")
                {
                    new Argument<string>().FromAmong("one-a", "one-b", "one-c")
                },
                new Command("two")
                {
                    new Argument<string>().FromAmong("two-a", "two-b", "two-c")
                },
                new Command("three")
                {
                    new Argument<string>().FromAmong("three-a", "three-b", "three-c")
                }
            };

            var result = outer.Parse("outer two b");

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_parsing_from_array_then_argument_completions_are_based_on_the_proximate_command()
        {
            var outer = new Command("outer")
            {
                new Command("one")
                {
                    new Argument<string>().FromAmong("one-a", "one-b", "one-c")
                },
                new Command("two")
                {
                    new Argument<string>().FromAmong("two-a", "two-b", "two-c")
                },
                new Command("three")
                {
                    new Argument<string>().FromAmong("three-a", "three-b", "three-c")
                }
            };

            ParseResult result = outer.Parse("outer two b");

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1518
        public void When_parsing_from_text_if_the_proximate_option_is_completed_then_completions_consider_other_option_tokens()
        {
            var command = new RootCommand
            {
                new Option<string>("--framework").FromAmong("net7.0"),
                new Option<string>("--language").FromAmong("C#"),
                new Option<string>("--langVersion")
            };
            var parser = new CommandLineBuilder(command).Build();
            var completions = parser.Parse("--framework net7.0 --l").GetCompletions();

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("--language", "--langVersion");
        }

        [Fact] 
        public void When_parsing_from_array_if_the_proximate_option_is_completed_then_completions_consider_other_option_tokens()
        {
            var command = new RootCommand
            {
                new Option<string>("--framework").FromAmong("net7.0"),
                new Option<string>("--language").FromAmong("C#"),
                new Option<string>("--langVersion")
            };
            var parser = new CommandLineBuilder(command).Build();
            var completions = parser.Parse(new[]{"--framework","net7.0","--l"}).GetCompletions();

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("--language", "--langVersion");
        }

        [Fact]
        public void Arguments_of_type_enum_provide_enum_values_as_suggestions()
        {
            var command = new Command("the-command")
            {
                new Argument<FileMode>()
            };

            var completions = command.Parse("the-command create")
                                     .GetCompletions();

            completions
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
            var completions = command.Parse(commandLine).GetCompletions(commandLine.Length + 1);

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("--allows-many");
        }

        [Fact]
        public void When_current_symbol_is_an_option_that_requires_arguments_then_parent_symbol_completions_are_omitted()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             new Option<string>("--allows-one"),
                             new Option<string[]>("--allows-many")
                         })
                         .UseSuggestDirective()
                         .Build();

            var completions = parser.Parse("--allows-one ").GetCompletions();

            completions.Should().BeEmpty();
        }

        [Fact]
        public void Option_substring_matching_when_arguments_have_default_values()
        {
            var command = new Command("the-command")
            {
                new Option<string>("--implicit", () => "the-default"),
                new Option<string>("--not", () => "the-default")
            };

            var completions = command.Parse("m").GetCompletions();

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("--implicit");
        }

        [Theory(Skip = "work in progress")]
        [InlineData("#r \"nuget: ", 11)]
        [InlineData("#r \"nuget:", 10)]
        public void It_can_provide_completions_within_quotes(string commandLine, int position)
        {
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

            var completions = r.Parse(commandLine).GetCompletions(position);

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo(expectedSuggestions);

            throw new NotImplementedException();
        }

        [Fact]
        public void Default_completions_can_be_cleared_and_replaced()
        {
            var argument = new Argument<DayOfWeek>();
            argument.Completions.Clear();
            argument.Completions.Add(new[] { "mon", "tues", "wed", "thur", "fri", "sat", "sun" });
            var command = new Command("the-command")
            {
                argument
            };

            var completions = command.Parse("the-command s")
                                     .GetCompletions();

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("sat", "sun", "tues");
        }

        [Fact]
        public void Default_completions_can_be_appended_to()
        {
            var command = new Command("the-command")
            {
                new Argument<DayOfWeek>
                {
                    Completions = { "mon", "tues", "wed", "thur", "fri", "sat", "sun" }
                }
            };

            var completions = command.Parse("the-command s")
                                     .GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo(
                    "sat",
                    nameof(DayOfWeek.Saturday),
                    "sun", 
                    nameof(DayOfWeek.Sunday),
                    "tues",
                    nameof(DayOfWeek.Tuesday),
                    nameof(DayOfWeek.Thursday),
                    nameof(DayOfWeek.Wednesday));
        }

        [Fact]
        public void Completions_for_options_provide_a_description()
        {
            var description = "The option before -y.";
            var option = new Option<string>("-x", description);

            var completions = new RootCommand { option }.GetCompletions();

            completions.Should().ContainSingle()
                       .Which
                       .Detail
                       .Should()
                       .Be(description);
        }
        
        [Fact]
        public void Completions_for_subcommands_provide_a_description()
        {
            var description = "The description for the subcommand";
            var subcommand = new Command("-x", description);

            var completions = new RootCommand { subcommand }.GetCompletions();

            completions.Should().ContainSingle()
                       .Which
                       .Detail
                       .Should()
                       .Be(description);
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1629
        public void When_option_completions_are_available_then_they_are_suggested_when_a_validation_error_occurs()
        {
            var option = new Option<DayOfWeek>("--day");

            var result = option.Parse("--day SleepyDay");

            result.Errors
                  .Should()
                  .ContainSingle()
                  .Which
                  .Message
                  .Should()
                  .Be(
                      $"Cannot parse argument 'SleepyDay' for option '--day' as expected type 'System.DayOfWeek'. Did you mean one of the following?{NewLine}Friday{NewLine}Monday{NewLine}Saturday{NewLine}Sunday{NewLine}Thursday{NewLine}Tuesday{NewLine}Wednesday");
        }
    }
}
