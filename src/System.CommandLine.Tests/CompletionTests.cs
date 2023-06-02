// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Completions;
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
            var option = new CliOption<string>("--hello");
            option.CompletionSources.Add("one", "two", "three");

            var completions = option.GetCompletions(CompletionContext.Empty);

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_GetCompletions_returns_available_option_aliases()
        {
            var command = new CliCommand("command")
            {
                new CliOption<string>("--one") { Description = "option one" },
                new CliOption<string>("--two") { Description = "option two" },
                new CliOption<string>("--three") { Description = "option three" },
            };

            var completions = command.GetCompletions(CompletionContext.Empty);

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1563
        public void Command_GetCompletions_returns_available_option_aliases_for_global_options()
        {
            var subcommand2 = new CliCommand("command2")
            {
                new CliOption<string>("--one") { Description = "option one" },
                new CliOption<string>("--two") { Description = "option two" }
            };

            var subcommand1 = new CliCommand("command1")
            {
                subcommand2
            };

            var rootCommand = new CliCommand("root")
            {
                subcommand1
            };

            rootCommand.Options.Add(new CliOption<string>("--three") 
            { 
                Description = "option three",
                Recursive = true
            });

            var completions = subcommand2.GetCompletions(CompletionContext.Empty);

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Command_GetCompletions_returns_available_subcommands()
        {
            var command = new CliCommand("command")
            {
                new CliCommand("one"),
                new CliCommand("two"),
                new CliCommand("three")
            };

            var completions = command.GetCompletions(CompletionContext.Empty);

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Command_GetCompletions_returns_available_subcommands_and_option_aliases()
        {
            var command = new CliCommand("command")
            {
                new CliCommand("subcommand"),
                new CliOption<string>("--option")
            };

            var completions = command.GetCompletions(CompletionContext.Empty);

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("subcommand", "--option");
        }

        [Fact]
        public void Command_GetCompletions_returns_available_subcommands_and_option_aliases_and_configured_arguments()
        {
            var command = new CliCommand("command")
            {
                new CliCommand("subcommand", "subcommand"),
                new CliOption<bool>("--option") { Description = "option" },
                new CliArgument<string[]>("args")
                {
                    Arity = ArgumentArity.OneOrMore,
                    CompletionSources = { "command-argument" }
                }
            };

            var completions = command.GetCompletions(CompletionContext.Empty);

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("subcommand", "--option", "command-argument");
        }

        [Fact]
        public void Command_GetCompletions_without_text_to_match_orders_alphabetically()
        {
            var command = new CliCommand("command")
            {
                new CliCommand("andmythirdsubcommand"),
                new CliCommand("mysubcommand"),
                new CliCommand("andmyothersubcommand"),
            };

            var completions = command.GetCompletions(CompletionContext.Empty);

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentSequenceTo("andmyothersubcommand", "andmythirdsubcommand", "mysubcommand");
        }

        [Fact]
        public void Command_GetCompletions_does_not_return_argument_names()
        {
            var command = new CliCommand("command")
            {
                new CliArgument<string>("the-argument")
            };

            var completions = command.GetCompletions(CompletionContext.Empty);

            completions
                .Select(item => item.Label)
                .Should()
                .NotContain("the-argument");
        }

        [Fact]
        public void Command_GetCompletions_with_text_to_match_orders_by_match_position_then_alphabetically()
        {
            var command = new CliCommand("command")
            {
                new CliCommand("andmythirdsubcommand"),
                new CliCommand("mysubcommand"),
                new CliCommand("andmyothersubcommand"),
            };

            CliConfiguration simpleConfig = new (command);
            var completions = command.Parse("my", simpleConfig).GetCompletions();

            completions
                .Select(item => item.Label)
                .Should()
                .BeEquivalentSequenceTo("mysubcommand", "andmyothersubcommand", "andmythirdsubcommand");
        }

        [Fact]
        public void When_an_option_has_a_default_value_it_will_still_be_suggested()
        {
            var command = new CliCommand("test")
            {
                new CliOption<string>("--apple") { DefaultValueFactory = (_) => "cortland" },
                new CliOption<string>("--banana"),
                new CliOption<string>("--cherry")
            };

            CliConfiguration simpleConfig = new (command);
            var result = command.Parse("", simpleConfig);

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
            var originOption = new CliOption<string>("--origin");
            var cloneOption = new CliOption<string>("--clone");

            cloneOption.CompletionSources.Add(ctx =>
            {
                var opt1Value = ctx.ParseResult.GetValue(originOption);
                return !string.IsNullOrWhiteSpace(opt1Value) ? new[] { opt1Value } : Array.Empty<string>();
            });

            CliRootCommand rootCommand = new CliRootCommand
            {
                originOption,
                cloneOption
            };

            CliConfiguration simpleConfig = new (rootCommand);
            var result = rootCommand.Parse("--origin test --clone ", simpleConfig);

            _output.WriteLine(result.ToString());

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("test");
        }

        [Fact]
        public void Command_GetCompletions_include_recursive_options_of_root_command()
        {
            CliRootCommand rootCommand = new()
            {
                new CliCommand("sub")
                {
                    new CliOption<int>("--option")
                }
            };

            var result = rootCommand.Parse("sub --option 123 ");

            _output.WriteLine(result.ToString());

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("--help", "-?", "-h", "/?", "/h");
        }

        [Fact]
        public void When_one_option_has_been_specified_then_it_and_its_siblings_will_still_be_suggested()
        {
            var command = new CliCommand("command")
            {
                new CliOption<string>("--apple"),
                new CliOption<string>("--banana"),
                new CliOption<string>("--cherry")
            };

            var commandLine = "--apple grannysmith";
            CliConfiguration simpleConfig = new (command);
            var result = command.Parse(commandLine, simpleConfig);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("--banana",
                                  "--cherry");
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_commands_will_not_be_suggested()
        {
            var rootCommand = new CliRootCommand
            {
                new CliCommand("apple")
                {
                    new CliOption<string>("--cortland")
                },
                new CliCommand("banana")
                {
                    new CliOption<string>("--cavendish")
                },
                new CliCommand("cherry")
                {
                    new CliOption<string>("--rainier")
                }
            };
            CliConfiguration simpleConfig = new (rootCommand);

            var result = rootCommand.Parse("cherry ", simpleConfig);

            result.GetCompletions()
                  .Should()
                  .NotContain(new[]{"apple", "banana", "cherry"});
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_commands_aliases_will_not_be_suggested()
        {
            var apple = new CliCommand("apple")
            {
                new CliOption<string>("--cortland")
            };
            apple.Aliases.Add("apl");

            var banana = new CliCommand("banana")
            {
                new CliOption<string>("--cavendish")
            };
            banana.Aliases.Add("bnn");

            var rootCommand = new CliRootCommand
            {
                apple,
                banana
            };
            CliConfiguration simpleConfig = new (rootCommand);

            var result = rootCommand.Parse("banana ", simpleConfig);

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .NotContain(new[] { "apl", "bnn" });
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1494
        public void When_a_subcommand_has_been_specified_then_its_sibling_options_will_not_be_suggested()
        {
            var command = new CliRootCommand("parent")
            {
                new CliCommand("child"), 
                new CliOption<string>("--parent-option")
            };

            var commandLine = "child";
            CliConfiguration simpleConfig = new (command);
            var parseResult = command.Parse(commandLine, simpleConfig);

            parseResult
                .GetCompletions(commandLine.Length + 1)
                .Select(item => item.Label)
                .Should()
                .NotContain("--parent-option");
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_sibling_options_with_argument_limit_reached_will_be_not_be_suggested()
        {
            var command = new CliRootCommand("parent")
            {
                new CliCommand("child"),
                new CliOption<string>("--parent-option"),
                new CliArgument<string>("arg")
            };

            var commandLine = "--parent-option 123 child";
            CliConfiguration simpleConfig = new (command);
            var parseResult = command.Parse(commandLine, simpleConfig);

            parseResult
                .GetCompletions(commandLine.Length + 1)
                .Should()
                .NotContain("--parent-option");
        }

        [Fact]
        public void When_a_subcommand_has_been_specified_then_its_child_options_will_be_suggested()
        {
            var command = new CliRootCommand("parent")
            {
                new CliArgument<string>("arg"),
                new CliCommand("child")
                {
                    new CliOption<string>("--child-option")
                }
            };

            var commandLine = "child ";
            CliConfiguration simpleConfig = new (command);
            var parseResult = command.Parse(commandLine, simpleConfig);

            parseResult
                .GetCompletions(commandLine.Length + 1)
                .Select(item => item.Label)
                .Should()
                .Contain("--child-option");
        }

        [Fact]
        public void When_a_subcommand_with_subcommands_has_been_specified_then_its_sibling_commands_will_not_be_suggested()
        {
            var rootCommand = new CliRootCommand
            {
                new CliCommand("apple")
                {
                    new CliCommand("cortland")
                },
                new CliCommand("banana")
                {
                    new CliCommand("cavendish")
                },
                new CliCommand("cherry")
                {
                    new CliCommand("rainier")
                }
            };

            var commandLine = "cherry";
            CliConfiguration simpleConfig = new (rootCommand);
            var result = rootCommand.Parse(commandLine, simpleConfig);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("--help", "-?", "-h", "/?", "/h", "rainier");
        }

        [Fact]
        public void When_one_option_has_been_partially_specified_then_nonmatching_siblings_will_not_be_suggested()
        {
            var command = new CliCommand("the-command")
            {
                new CliOption<string>("--apple"),
                new CliOption<string>("--banana"),
                new CliOption<string>("--cherry")
            };

            var input = "a";
            CliConfiguration simpleConfig = new (command);
            var result = command.Parse(input, simpleConfig);

            result.GetCompletions(input.Length)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("--apple",
                                  "--banana");
        }

        [Fact]
        public void An_option_can_be_hidden_from_completions_by_setting_IsHidden_to_true()
        {
            var command = new CliCommand("the-command")
            {
                new CliOption<string>("--hide-me")
                {
                    Hidden = true
                },
                new CliOption<string>("-n") { Description = "Not hidden" }
            };

            CliConfiguration simpleConfig = new (command);
            var completions = command.Parse("the-command ", simpleConfig).GetCompletions();

            completions.Select(item => item.Label).Should().NotContain("--hide-me");
        }

        [Fact]
        public void Parser_options_can_supply_context_sensitive_matches()
        {
            var command = new CliRootCommand
            {
                CreateOptionWithAcceptOnlyFromAmong(name: "--bread", "wheat", "sourdough", "rye"),
                CreateOptionWithAcceptOnlyFromAmong(name: "--cheese", "provolone", "cheddar", "cream cheese")
            };

            var commandLine = "--bread";
            CliConfiguration simpleConfig = new (command);
            var result = command.Parse(commandLine, simpleConfig);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("rye", "sourdough", "wheat");

            commandLine = "--bread wheat --cheese ";
            result = command.Parse(commandLine, simpleConfig);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("cheddar", "cream cheese", "provolone");
        }

        [Fact]
        public void Subcommand_names_are_available_as_suggestions()
        {
            var command = new CliCommand("test")
            {
                new CliCommand("one", "Command one"),
                new CliCommand("two", "Command two"),
                new CliArgument<string>("arg")
            };

            var commandLine = "test";
            CliConfiguration simpleConfig = new (command);
            command.Parse(commandLine, simpleConfig)
                   .GetCompletions(commandLine.Length + 1)
                   .Select(item => item.Label)
                   .Should()
                   .BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Both_subcommands_and_options_are_available_as_suggestions()
        {
            var command = new CliCommand("test")
            {
                new CliCommand("one"),
                new CliOption<string>("--one"),
                new CliArgument<string>("arg")
            };

            var commandLine = "test";
            CliConfiguration simpleConfig = new (command);
            command.Parse(commandLine, simpleConfig)
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
            var command = new CliCommand("outer")
            {
                new CliOption<string>("--one"),
                new CliOption<string>("--two"),
                new CliOption<string>("--three")
            };

            CliConfiguration simpleConfig = new (command);
            ParseResult result = command.Parse(input, simpleConfig);
            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void Option_GetCompletions_can_be_based_on_the_proximate_option()
        {
            CliCommand outer = new CliCommand("outer")
            {
                new CliOption<string>("--one"),
                new CliOption<string>("--two"),
                new CliOption<string>("--three")
            };

            var commandLine = "outer";
            CliConfiguration simpleConfig = new (outer);
            ParseResult result = outer.Parse(commandLine, simpleConfig);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Argument_completions_can_be_based_on_the_proximate_option()
        {
            var outer = new CliCommand("outer")
            {
                CreateOptionWithAcceptOnlyFromAmong(name: "--one", "one-a", "one-b"),
                CreateOptionWithAcceptOnlyFromAmong(name: "--two", "two-a", "two-b")
            };

            var commandLine = "outer --two";
            CliConfiguration simpleConfig = new (outer);
            ParseResult result = outer.Parse(commandLine, simpleConfig);

            result.GetCompletions(commandLine.Length + 1)
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_GetCompletions_can_be_based_on_the_proximate_option_and_partial_input()
        {
            var outer = new CliCommand("outer")
            {
                new CliCommand("one", "Command one"),
                new CliCommand("two", "Command two"),
                new CliCommand("three", "Command three")
            };

            CliConfiguration simpleConfig = new (outer);
            ParseResult result = outer.Parse("outer o", simpleConfig);

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Completions_can_be_provided_in_the_absence_of_validation()
        {
            CliOption<string> option = new ("-t");
            option.CompletionSources.Add("vegetable", "mineral", "animal");

            var command = new CliCommand("the-command")
            {
                option
            };

            CliConfiguration simpleConfig = new (command);
            command.Parse("the-command -t m", simpleConfig)
                   .GetCompletions()
                   .Select(item => item.Label)
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");

            command.Parse("the-command -t something-else", simpleConfig)
                   .Errors
                   .Should()
                   .BeEmpty();
        }

        [Fact]
        public void Command_argument_completions_can_be_provided_using_a_delegate()
        {
            var command = new CliCommand("the-command")
            {
                new CliCommand("one")
                {
                    new CliArgument<string>("arg")
                        {
                            CompletionSources = { _ => new[] { "vegetable", "mineral", "animal" } }
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
            var option = new CliOption<string>("-x");
            option.CompletionSources.Add(_ => new[] { "vegetable", "mineral", "animal" });

            var command = new CliCommand("the-command")
            {
                option
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
            var command = new CliCommand("outer")
            {
                CreateOptionWithAcceptOnlyFromAmong(name: "one", "one-a", "one-b", "one-c"),
                CreateOptionWithAcceptOnlyFromAmong(name: "two", "two-a", "two-b", "two-c"),
                CreateOptionWithAcceptOnlyFromAmong(name: "three", "three-a", "three-b", "three-c")
            };

            var configuration = new CliConfiguration(command);

            var result = command.Parse("outer two b", configuration);

            result.GetCompletions()
                  .Select(item => item.Label)
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_parsing_from_array_then_argument_completions_are_based_on_the_proximate_option()
        {
            var command = new CliCommand("outer")
            {
                CreateOptionWithAcceptOnlyFromAmong(name: "one", "one-a", "one-b", "one-c"),
                CreateOptionWithAcceptOnlyFromAmong(name: "two", "two-a", "two-b", "two-c"),
                CreateOptionWithAcceptOnlyFromAmong(name: "three", "three-a", "three-b", "three-c")
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
            var outer = new CliCommand("outer")
            {
                new CliCommand("one")
                {
                    CreateArgumentWithAcceptOnlyFromAmong("one-a", "one-b", "one-c")
                },
                new CliCommand("two")
                {
                    CreateArgumentWithAcceptOnlyFromAmong("two-a", "two-b", "two-c")
                },
                new CliCommand("three")
                {
                    CreateArgumentWithAcceptOnlyFromAmong("three-a", "three-b", "three-c")
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
            var outer = new CliCommand("outer")
            {
                new CliCommand("one")
                {
                    CreateArgumentWithAcceptOnlyFromAmong("one-a", "one-b", "one-c")
                },
                new CliCommand("two")
                {
                    CreateArgumentWithAcceptOnlyFromAmong("two-a", "two-b", "two-c")
                },
                new CliCommand("three")
                {
                    CreateArgumentWithAcceptOnlyFromAmong("three-a", "three-b", "three-c")
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
            var command = new CliRootCommand
            {
                CreateOptionWithAcceptOnlyFromAmong(name: "--framework", "net7.0"),
                CreateOptionWithAcceptOnlyFromAmong(name: "--language", "C#"),
                new CliOption<string>("--langVersion")
            };
            var configuration = new CliConfiguration(command);
            var completions = command.Parse("--framework net7.0 --l", configuration).GetCompletions();

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("--language", "--langVersion");
        }

        [Fact] 
        public void When_parsing_from_array_if_the_proximate_option_is_completed_then_completions_consider_other_option_tokens()
        {
            var command = new CliRootCommand
            {
                CreateOptionWithAcceptOnlyFromAmong(name: "--framework", "net7.0"),
                CreateOptionWithAcceptOnlyFromAmong(name: "--language", "C#"),
                new CliOption<string>("--langVersion")
            };
            var configuration = new CliConfiguration(command);
            var completions = command.Parse(new[]{"--framework","net7.0","--l"}, configuration).GetCompletions();

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("--language", "--langVersion");
        }

        [Fact]
        public void Arguments_of_type_enum_provide_enum_values_as_suggestions()
        {
            var command = new CliCommand("the-command")
            {
                new CliArgument<FileMode>("arg")
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
            var command = new CliCommand("command")
            {
                new CliOption<string>("--allows-one"),
                new CliOption<string[]>("--allows-many")
            };

            var commandLine = "--allows-one x";
            CliConfiguration simpleConfig = new (command);
            var completions = command.Parse(commandLine, simpleConfig).GetCompletions(commandLine.Length + 1);

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("--allows-many");
        }

        [Fact]
        public void When_current_symbol_is_an_option_that_requires_arguments_then_parent_symbol_completions_are_omitted()
        {
            var configuration = new CliConfiguration(new CliRootCommand
                         {
                             new CliOption<string>("--allows-one"),
                             new CliOption<string[]>("--allows-many")
                         })
            {
                Directives = { new SuggestDirective() } 
            };

            var completions = configuration.Parse("--allows-one ").GetCompletions();

            completions.Should().BeEmpty();
        }

        [Fact]
        public void Option_substring_matching_when_arguments_have_default_values()
        {
            var command = new CliCommand("the-command")
            {
                new CliOption<string>("--implicit") { DefaultValueFactory = (_) => "the-default" },
                new CliOption<string>("--not") { DefaultValueFactory = (_) => "the-default" }
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

            var argument = new CliArgument<string>("arg");
            argument.CompletionSources.Add(expectedSuggestions);

            var r = new CliCommand("#r")
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
            var argument = new CliArgument<DayOfWeek>("day");
            argument.CompletionSources.Clear();
            argument.CompletionSources.Add(new[] { "mon", "tues", "wed", "thur", "fri", "sat", "sun" });
            var command = new CliCommand("the-command")
            {
                argument
            };
            CliConfiguration simpleConfig = new (command);
            var completions = command.Parse("the-command s", simpleConfig)
                                     .GetCompletions();

            completions.Select(item => item.Label)
                       .Should()
                       .BeEquivalentTo("sat", "sun", "tues");
        }

        [Fact]
        public void Default_completions_can_be_appended_to()
        {
            var command = new CliCommand("the-command")
            {
                new CliArgument<DayOfWeek>("day")
                {
                    CompletionSources = { "mon", "tues", "wed", "thur", "fri", "sat", "sun" }
                }
            };

            CliConfiguration simpleConfig = new (command);
            var completions = command.Parse("the-command s", simpleConfig)
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
            var option = new CliOption<string>("-x") { Description = description };

            var completions = new CliCommand("test") { option }.GetCompletions(CompletionContext.Empty);

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
            var subcommand = new CliCommand("-x", description);

            var completions = new CliCommand("test") { subcommand }.GetCompletions(CompletionContext.Empty);

            completions.Should().ContainSingle()
                       .Which
                       .Detail
                       .Should()
                       .Be(description);
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1629
        public void When_option_completions_are_available_then_they_are_suggested_when_a_validation_error_occurs()
        {
            CliOption<DayOfWeek> option = new ("--day");
            CliRootCommand rootCommand = new () { option };
            CliConfiguration simpleConfig = new (rootCommand);

            var result = rootCommand.Parse("--day SleepyDay", simpleConfig);

            result.Errors
                  .Should()
                  .ContainSingle()
                  .Which
                  .Message
                  .Should()
                  .Be(
                      $"Cannot parse argument 'SleepyDay' for option '--day' as expected type 'System.DayOfWeek'. Did you mean one of the following?{NewLine}Friday{NewLine}Monday{NewLine}Saturday{NewLine}Sunday{NewLine}Thursday{NewLine}Tuesday{NewLine}Wednesday");
        }

        private static CliArgument<string> CreateArgumentWithAcceptOnlyFromAmong(params string[] values)
        {
            CliArgument<string> argument = new("arg");
            argument.AcceptOnlyFromAmong(values);
            return argument;
        }

        private static CliOption<string> CreateOptionWithAcceptOnlyFromAmong(string name, params string[] values)
        {
            CliOption<string> option = new(name);
            option.AcceptOnlyFromAmong(values);
            return option;
        }
    }
}
