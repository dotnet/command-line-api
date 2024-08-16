// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.IO;
using FluentAssertions;
using FluentAssertions.Equivalency;
using System.Linq;
using FluentAssertions.Common;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        // TODO: Update testing strategy if we use Location in equality. Some will break
        private readonly Location dummyLocation = new("", Location.Internal, -1, null);

        private T GetValue<T>(ParseResult parseResult, CliOption<T> option)
            => parseResult.GetValue(option);

        private T GetValue<T>(ParseResult parseResult, CliArgument<T> argument)
            => parseResult.GetValue(argument);

        //[Fact]
        //public void FailureTest()
        //{
        //    Assert.True(false);
        //}

        [Fact]
        public void An_option_can_be_checked_by_object_instance()
        {
            var option = new CliOption<bool>("--flag");
            var option2 = new CliOption<bool>("--flag2");
            var rootCommand = new CliRootCommand { option, option2 };
            var result = CliParser.Parse(rootCommand, ["--flag"]);

            result.GetResult(option).Should().NotBeNull();
            result.GetResult(option)
                .Option
                .Name
                .Should()
                .BeEquivalentTo("--flag");

            result.GetResult(option2).Should().BeNull();
        }

        [Fact]
        public void Two_options_are_parsed_correctly()
        {
            var optionOne = new CliOption<bool>("-o", "--one");

            var optionTwo = new CliOption<bool>("-t", "--two");

            var rootCommand = new CliRootCommand { optionOne, optionTwo };
            var result = CliParser.Parse(rootCommand, "-o -t");
            // TODO: consider more specific test
            result.GetResult(optionOne).Should().NotBeNull();
            result.GetResult(optionTwo).Should().NotBeNull();
        }

        [Theory]
        [InlineData("-")]
        [InlineData("/")]
        public void When_a_token_is_just_a_prefix_then_an_error_is_returned(string prefix)
        {
            var rootCommand = new CliRootCommand();
            var result = CliParser.Parse(rootCommand, prefix);

            result.Errors
                .Select(e => e.Message)
                .Should()
                .Contain(LocalizationResources.UnrecognizedCommandOrArgument(prefix));
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_equals_delimiter()
        {
            var option = new CliOption<string>("-x");
            var rootCommand = new CliRootCommand { option };

            var result = CliParser.Parse(rootCommand, "-x=some-value");

            result.Errors.Should().BeEmpty();

            result.GetResult(option).Tokens.Should().ContainSingle(a => a.Value == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_equals_delimiter()
        {
            var option = new CliOption<string>("--hello");

            var rootCommand = new CliRootCommand { option };
            var result = CliParser.Parse(rootCommand, "--hello=there");

            result.Errors.Should().BeEmpty();

            result.GetResult(option).Tokens.Should().ContainSingle(a => a.Value == "there");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_colon_delimiter()
        {
            var option = new CliOption<string>("-x");

            var rootCommand = new CliRootCommand { option };
            var result = CliParser.Parse(rootCommand, "-x:some-value");

            result.Errors.Should().BeEmpty();

            result.GetResult(option).Tokens.Should().ContainSingle(a => a.Value == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_colon_delimiter()
        {
            var option = new CliOption<string>("--hello");

            var rootCommand = new CliRootCommand { option };
            var result = CliParser.Parse(rootCommand, "--hello:there");

            result.Errors.Should().BeEmpty();

            result.GetResult(option).Tokens.Should().ContainSingle(a => a.Value == "there");
        }

        [Fact]
        public void Option_short_forms_can_be_bundled()
        {
            var command = new CliCommand("the-command");
            command.Options.Add(new CliOption<bool>("-x"));
            command.Options.Add(new CliOption<bool>("-y"));
            command.Options.Add(new CliOption<bool>("-z"));

            var result = CliParser.Parse(command, "the-command -xyz");

            result.CommandResult
                .ValueResults
                .Select(o => ((CliValueResult)o).ValueSymbol.Name)
                .Should()
                .BeEquivalentTo("-x", "-y", "-z");
        }

        /*

        [Fact]
        public void Options_short_forms_do_not_get_unbundled_if_unbundling_is_turned_off()
        {
            // TODO: unmatched tokens has been moved, fix
            CliRootCommand rootCommand = new CliRootCommand()
            {
                new CliCommand("the-command")
                {
                    new CliOption<bool>("-x"),
                    new CliOption<bool>("-y"),
                    new CliOption<bool>("-z")
                }
            };

            CliConfiguration configuration = new (rootCommand)
            {
                EnablePosixBundling = false
            };

            var result = rootCommand.Parse("the-command -xyz", configuration);

            result.UnmatchedTokens
                .Should()
                .BeEquivalentTo("-xyz");
        }
        */

        [Fact]
        public void Option_long_forms_do_not_get_unbundled()
        {
            CliCommand command =
                new CliCommand("the-command")
                {
                    new CliOption<bool>("--xyz"),
                    new CliOption<bool>("-x"),
                    new CliOption<bool>("-y"),
                    new CliOption<bool>("-z")
                };

            var result = CliParser.Parse(command, "the-command --xyz");

            result.CommandResultInternal
                .Children
                .Select(o => ((CliOptionResultInternal)o).Option.Name)
                .Should()
                .BeEquivalentTo("--xyz");
        }

        [Fact]
        public void Options_do_not_get_unbundled_unless_all_resulting_options_would_be_valid_for_the_current_command()
        {
            var outer = new CliCommand("outer");
            outer.Options.Add(new CliOption<bool>("-a"));
            var inner = new CliCommand("inner")
            {
                new CliArgument<string[]>("arg")
            };
            inner.Options.Add(new CliOption<bool>("-b"));
            inner.Options.Add(new CliOption<bool>("-c"));
            outer.Subcommands.Add(inner);

            ParseResult result = CliParser.Parse(outer, "outer inner -abc");

            result.CommandResultInternal
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("-abc");
        }

        [Fact]
        public void Required_option_arguments_are_not_unbundled()
        {
            var optionA = new CliOption<string>("-a");
            var optionB = new CliOption<bool>("-b");
            var optionC = new CliOption<bool>("-c");

            var command = new CliRootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = CliParser.Parse(command, "-a -bc");

            result.GetResult(optionA)
                .Tokens
                .Should()
                .ContainSingle(t => t.Value == "-bc");
        }

        [Fact]
        public void Last_bundled_option_can_accept_argument_with_no_separator()
        {
            var optionA = new CliOption<bool>("-a");
            var optionB = new CliOption<string>("-b") { Arity = ArgumentArity.ZeroOrOne };
            var optionC = new CliOption<string>("-c") { Arity = ArgumentArity.ExactlyOne };

            var command = new CliRootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = CliParser.Parse(command, "-abcvalue");
            result.GetResult(optionA).Should().NotBeNull();
            result.GetResult(optionB).Should().NotBeNull();

            result.GetResult(optionC)
                .Tokens
                .Should()
                .ContainSingle(t => t.Value == "value");
        }

        [Fact]
        public void Last_bundled_option_can_accept_argument_with_equals_separator()
        {
            var optionA = new CliOption<bool>("-a");
            var optionB = new CliOption<string>("-b") { Arity = ArgumentArity.ZeroOrOne };
            var optionC = new CliOption<string>("-c") { Arity = ArgumentArity.ExactlyOne };

            var command = new CliRootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = CliParser.Parse(command, "-abc=value");
            result.GetResult(optionA).Should().NotBeNull();
            result.GetResult(optionB).Should().NotBeNull();

            result.GetResult(optionC)
                .Tokens
                .Should()
                .ContainSingle(t => t.Value == "value");
        }

        [Fact]
        public void Last_bundled_option_can_accept_argument_with_colon_separator()
        {
            var optionA = new CliOption<bool>("-a");
            var optionB = new CliOption<string>("-b") { Arity = ArgumentArity.ZeroOrOne };
            var optionC = new CliOption<string>("-c") { Arity = ArgumentArity.ExactlyOne };

            var command = new CliRootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = CliParser.Parse(command, "-abc:value");
            result.GetResult(optionA).Should().NotBeNull();
            result.GetResult(optionB).Should().NotBeNull();

            result.GetResult(optionC)
                .Tokens
                .Should()
                .ContainSingle(t => t.Value == "value");
        }

        [Fact]
        public void Invalid_char_in_bundle_causes_rest_to_be_interpreted_as_value()
        {
            var optionA = new CliOption<bool>("-a");
            var optionB = new CliOption<string>("-b") { Arity = ArgumentArity.ZeroOrOne };
            var optionC = new CliOption<string>("-c") { Arity = ArgumentArity.ExactlyOne };

            var command = new CliRootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = CliParser.Parse(command, "-abvcalue");
            result.GetResult(optionA).Should().NotBeNull();
            result.GetResult(optionB).Should().NotBeNull();

            result.GetResult(optionB)
                .Tokens
                .Should()
                .ContainSingle(t => t.Value == "vcalue");

            result.GetResult(optionC).Should().BeNull();
        }


        [Fact]
        public void Parser_root_Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
            var animalsOption = new CliOption<string[]>("-a", "--animals");
            var vegetablesOption = new CliOption<string[]>("-v", "--vegetables");
            var rootCommand = new CliRootCommand
            {
                animalsOption,
                vegetablesOption
            };

            var result = CliParser.Parse(rootCommand, "-a cat -v carrot -a dog");

            result.GetResult(animalsOption)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("cat", "dog");

            result.GetResult(vegetablesOption)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("carrot");
        }

        /*
        [Fact]
        public void Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
            // TODO: tests AcceptOnlyFromAmong, fix
            // TODO: This test does not appear to use AcceptOnlyFromAmong. Consider if test can just use normal strings
            var animalsOption = new CliOption<string[]>("-a", "--animals");
            animalsOption.AcceptOnlyFromAmong("dog", "cat", "sheep");
            var vegetablesOption = new CliOption<string[]>("-v", "--vegetables");
            CliCommand command =
                new CliCommand("the-command") {
                    animalsOption,
                    vegetablesOption
                };

            var result = command.Parse("the-command -a cat -v carrot -a dog");

            result.GetResult(animalsOption)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("cat", "dog");

            result.GetResult(vegetablesOption)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("carrot");
        }
        */

        [Fact]
        public void When_an_option_is_not_respecified_but_limit_is_reached_then_the_following_token_is_considered_an_argument_to_the_parent_command()
        {
            var animalsOption = new CliOption<string[]>("-a", "--animals");

            var vegetablesOption = new CliOption<string[]>("-v", "--vegetables");

            CliCommand command =
                new CliCommand("the-command")
                {
                    animalsOption,
                    vegetablesOption,
                    new CliArgument<string[]>("arg")
                };

            var result = CliParser.Parse(command, "the-command -a cat some-arg -v carrot");

            result.GetResult(animalsOption)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("cat");

            result.GetResult(vegetablesOption)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("carrot");

            result.CommandResultInternal
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("some-arg");
        }

        [Fact]
        public void Command_with_multiple_options_is_parsed_correctly()
        {
            var command = new CliCommand("outer")
            {
                new CliOption<string>("--inner1"),
                new CliOption<string>("--inner2")
            };

            var result = CliParser.Parse(command, "outer --inner1 argument1 --inner2 argument2");

            result.CommandResultInternal
                .Children
                .Should()
                .ContainSingle(o =>
                    ((CliOptionResultInternal)o).Option.Name == "--inner1" &&
                    o.Tokens.Single().Value == "argument1");
            result.CommandResultInternal
                .Children
                .Should()
                .ContainSingle(o =>
                    ((CliOptionResultInternal)o).Option.Name == "--inner2" &&
                    o.Tokens.Single().Value == "argument2");
        }

        [Fact(Skip = "Location means these are no longer equivalent.")]
        // TODO: Add comparison that ignores locations
        public void Relative_order_of_arguments_and_options_within_a_command_does_not_matter()
        {
            var command = new CliCommand("move")
            {
                new CliArgument<string[]>("arg"),
                new CliOption<string>("-X")
            };

            // option before args
            ParseResult result1 = CliParser.Parse(
                command,
                "move -X the-arg-for-option-x ARG1 ARG2");

            // option between two args
            ParseResult result2 = CliParser.Parse(
                command,
                "move ARG1 -X the-arg-for-option-x ARG2");

            // option after args
            ParseResult result3 = CliParser.Parse(
                command,
                "move ARG1 ARG2 -X the-arg-for-option-x");

            // all should be equivalent
            result1.Should()
                .BeEquivalentTo(
                    result2,
                    x => x.IgnoringCyclicReferences()
                            .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal))
                            .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.PrivateProtected)));
            result1.Should()
                .BeEquivalentTo(
                    result3,
                    x => x.IgnoringCyclicReferences()
                            .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal))
                            .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.PrivateProtected)));
        }

        // TODO: Tests tokens which is no longer exposed, and should be replaced by tests of location or removed
        /*
        [Theory]
        [InlineData("--one 1 --many 1 --many 2")]
        [InlineData("--one 1 --many 1 --many 2 arg1 arg2")]
        [InlineData("--many 1 --one 1 --many 2")]
        [InlineData("--many 2 --many 1 --one 1")]
        [InlineData("[parse] --one 1 --many 1 --many 2")]
        [InlineData("--one \"stuff in quotes\" this-is-arg1 \"this is arg2\"")]
        [InlineData("not a valid command line --one 1")]
        public void Original_order_of_tokens_is_preserved_in_ParseResult_Tokens(string commandLine)
        {
            var rawSplit = CliParser.SplitCommandLine(commandLine);

            var command = new CliCommand("the-command")
            {
                new CliArgument<string[]>("arg"),
                new CliOption<string>("--one"),
                new CliOption<string[]>("--many")
            };

            var result = CliParser.Parse(command, commandLine);

            result.Tokens.Select(t => t.Value).Should().Equal(rawSplit);
        }
        */

        /*
        [Fact]
        public void An_outer_command_with_the_same_name_does_not_capture()
        {
            // TODO: uses Diagram, fix
            var command = new CliCommand("one")
            {
                new CliCommand("two")
                {
                    new CliCommand("three")
                },
                new CliCommand("three")
            };

            ParseResult result = CliParser.Parse(command, "one two three");

            result.Diagram().Should().Be("[ one [ two [ three ] ] ]");
        }

        [Fact]
        public void An_inner_command_with_the_same_name_does_not_capture()
        {
            // TODO: uses Diagram, fix
            var command = new CliCommand("one")
            {
                new CliCommand("two")
                {
                    new CliCommand("three")
                },
                new CliCommand("three")
            };

            ParseResult result = CliParser.Parse(command, "one three");

            result.Diagram().Should().Be("[ one [ three ] ]");
        }
        */

        [Fact]
        public void When_nested_commands_all_accept_arguments_then_the_nearest_captures_the_arguments()
        {
            var command = new CliCommand("outer")
            {
                new CliArgument<string[]>("arg1"),
                new CliCommand("inner")
                {
                    new CliArgument<string[]>("arg2")
                }
            };

            var result = CliParser.Parse(command, "outer arg1 inner arg2");

            result.CommandResultInternal
                .Parent
                .Tokens.Select(t => t.Value)
                .Should()
                .BeEquivalentTo("arg1");

            result.CommandResultInternal
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("arg2");
        }

        /*
        [Fact]
        public void Nested_commands_with_colliding_names_cannot_both_be_applied()
        {
            // TODO: uses Diagram, fix
            var command = new CliCommand("outer")
            {
                new CliArgument<string>("arg1"),
                new CliCommand("non-unique")
                {
                    new CliArgument<string>("arg2")
                },
                new CliCommand("inner")
                {
                    new CliArgument<string>("arg3"),
                    new CliCommand("non-unique")
                    {
                        new CliArgument<string>("arg4")
                    }
                }
            };

            ParseResult result = command.Parse("outer arg1 inner arg2 non-unique arg3 ");

            result.Diagram().Should().Be("[ outer <arg1> [ inner <arg2> [ non-unique <arg3> ] ] ]");
        }
        */

        [Fact]
        public void When_child_option_will_not_accept_arg_then_parent_can()
        {
            var option = new CliOption<bool>("-x");
            var command = new CliCommand("the-command")
            {
                option,
                new CliArgument<string>("arg")
            };

            var result = CliParser.Parse(command, "the-command -x the-argument");

            var optionResult = result.GetResult(option);
            optionResult.Tokens.Should().BeEmpty();
            result.CommandResultInternal.Tokens.Select(t => t.Value).Should().BeEquivalentTo("the-argument");
        }

        [Fact]
        public void When_parent_option_will_not_accept_arg_then_child_can()
        {
            var option = new CliOption<string>("-x");
            var command = new CliCommand("the-command")
            {
                option
            };

            var result = CliParser.Parse(command, "the-command -x the-argument");

            result.GetResult(option).Tokens.Select(t => t.Value).Should().BeEquivalentTo("the-argument");
            result.CommandResultInternal.Tokens.Should().BeEmpty();
        }

        [Fact]
        public void Required_arguments_on_parent_commands_do_not_create_parse_errors_when_an_inner_command_is_specified()
        {
            var child = new CliCommand("child");

            var parent = new CliCommand("parent")
            {
                new CliArgument<string>("arg"),
                child
            };

            var result = CliParser.Parse(parent, "child");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Required_arguments_on_grandparent_commands_do_not_create_parse_errors_when_an_inner_command_is_specified()
        {
            var grandchild = new CliCommand("grandchild");

            var grandparent = new CliCommand("grandparent")
            {
                new CliArgument<string>("arg"),
                new CliCommand("parent")
                {
                    grandchild
                }
            };

            var result = CliParser.Parse(grandparent, "parent grandchild");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_options_with_the_same_name_are_defined_on_parent_and_child_commands_and_specified_at_the_end_then_it_attaches_to_the_inner_command()
        {
            var outer = new CliCommand("outer")
            {
                new CliCommand("inner")
                {
                    new CliOption<bool>("-x")
                },
                new CliOption<bool>("-x")
            };

            ParseResult result = CliParser.Parse(outer, "outer inner -x");

            result.CommandResultInternal
                .Parent
                .Should()
                .BeOfType<CliCommandResultInternal>()
                .Which
                .Children
                .Should()
                .AllBeAssignableTo<CliCommandResultInternal>();
            result.CommandResultInternal
                .Children
                .Should()
                .ContainSingle(o => ((CliOptionResultInternal)o).Option.Name == "-x");
        }

        [Fact]
        public void When_options_with_the_same_name_are_defined_on_parent_and_child_commands_and_specified_in_between_then_it_attaches_to_the_outer_command()
        {
            var outer = new CliCommand("outer");
            outer.Options.Add(new CliOption<bool>("-x"));
            var inner = new CliCommand("inner");
            inner.Options.Add(new CliOption<bool>("-x"));
            outer.Subcommands.Add(inner);

            var result = CliParser.Parse(outer, "outer -x inner");

            result.CommandResultInternal
                .Children
                .Should()
                .BeEmpty();
            result.CommandResultInternal
                .Parent
                .Should()
                .BeOfType<CliCommandResultInternal>()
                .Which
                .Children
                .Should()
                .ContainSingle(o => o is CliOptionResultInternal && ((CliOptionResultInternal)o).Option.Name == "-x");
        }

        /*

        [Fact]
        // TODO: tests unmatched tokens, needs fix
        public void Arguments_only_apply_to_the_nearest_command()
        {
            var outer = new CliCommand("outer")
            {
                new CliArgument<string>("arg1"),
                new CliCommand("inner")
                {
                    new CliArgument<string>("arg2")
                }
            };

            ParseResult result = outer.Parse("outer inner arg1 arg2");

            result.CommandResultInternal
                .Parent
                .Tokens
                .Should()
                .BeEmpty();
            result.CommandResultInternal
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("arg1");
            result.UnmatchedTokens
                .Should()
                .BeEquivalentTo("arg2");
        }
        */

        [Fact]
        public void Options_only_apply_to_the_nearest_command()
        {
            var outerOption = new CliOption<string>("-x");
            var innerOption = new CliOption<string>("-x");

            var outer = new CliCommand("outer")
            {
                new CliCommand("inner")
                {
                    innerOption
                },
                outerOption
            };

            var result = CliParser.Parse(outer, "outer inner -x one -x two");

            result.RootCommandResult
                .GetResult(outerOption)
                .Should()
                .BeNull();
        }

        [Fact]
        public void Subsequent_occurrences_of_tokens_matching_command_names_are_parsed_as_arguments()
        {
            var command = new CliCommand("the-command")
            {
                new CliCommand("complete")
                {
                    new CliArgument<string>("arg"),
                    new CliOption<int>("--position")
                }
            };

            ParseResult result = CliParser.Parse(
                command, new[] {
                    "the-command",
                    "complete",
                    "--position",
                    "7",
                    "the-command"
                });

            CliCommandResultInternal completeResult = result.CommandResultInternal;

            completeResult.Tokens.Select(t => t.Value).Should().BeEquivalentTo("the-command");
        }

        [Fact]
        public void Absolute_unix_style_paths_are_lexed_correctly()
        {
            const string commandText =
                @"rm ""/temp/the file.txt""";

            CliCommand command = new("rm")
            {
                new CliArgument<string[]>("arg")
            };

            var result = CliParser.Parse(command, commandText);

            result.CommandResultInternal
                .Tokens
                .Select(t => t.Value)
                .Should()
                .OnlyContain(a => a == @"/temp/the file.txt");
        }

        [Fact]
        public void Absolute_Windows_style_paths_are_lexed_correctly()
        {
            const string commandText =
                @"rm ""c:\temp\the file.txt\""";

            CliCommand command = new("rm")
            {
                new CliArgument<string[]>("arg")
            };

            ParseResult result = CliParser.Parse(command, commandText);

            result.CommandResultInternal
                .Tokens
                .Should()
                .OnlyContain(a => a.Value == @"c:\temp\the file.txt\");
        }

// TODO: Default values
/*
        [Fact]
        public void Commands_can_have_default_argument_values()
        {
            var argument = new CliArgument<string>("the-arg")
            {
                DefaultValueFactory = (_) => "default"
            };

            var command = new CliCommand("command")
            {
                argument
            };

            ParseResult result = CliParser.Parse(command, "command");

            GetValue(result, argument)
                .Should()
                .Be("default");
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_the_option_can_still_be_accessed_as_though_it_had_been_applied()
        {
            var command = new CliCommand("command");
            var option = new CliOption<string>("-o", "--option")
            {
                DefaultValueFactory = (_) => "the-default"
            };
            command.Options.Add(option);

            ParseResult result = CliParser.Parse(command, "command");

            result.GetResult(option).Should().NotBeNull();
            GetValue(result, option).Should().Be("the-default");
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_the_option_result_is_implicit()
        {
            var option = new CliOption<string>("-o", "--option")
            {
                DefaultValueFactory = (_) => "the-default"
            };

            var command = new CliCommand("command")
            {
                option
            };

            var result = CliParser.Parse(command, "command");

            result.GetResult(option)
                .Implicit
                .Should()
                .BeTrue();
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_there_are_no_tokens()
        {
            var option = new CliOption<string>("-o")
            {
                DefaultValueFactory = (_) => "the-default"
            };

            var command = new CliCommand("command")
            {
                option
            };

            var result = CliParser.Parse(command, "command");

            result.GetResult(option)
                .IdentifierToken
                .Should()
                .BeEquivalentTo(default(CliToken));
        }

        [Fact]
        public void When_an_argument_with_a_default_value_is_not_matched_then_there_are_no_tokens()
        {
            var argument = new CliArgument<string>("o")
            {
                DefaultValueFactory = (_) => "the-default"
            };

            var command = new CliCommand("command")
            {
                argument
            };
            var result = CliParser.Parse(command, "command");

            result.GetResult(argument)
                .Tokens
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void Command_default_argument_value_does_not_override_parsed_value()
        {
            var argument = new CliArgument<DirectoryInfo>("the-arg")
            {
                DefaultValueFactory = (_) => new DirectoryInfo(Directory.GetCurrentDirectory())
            };

            var command = new CliCommand("inner")
            {
                argument
            };

            var result = CliParser.Parse(command, "the-directory");

            GetValue(result, argument)
                .Name
                .Should()
                .Be("the-directory");
        }
*/

        [Fact]
        public void Unmatched_tokens_that_look_like_options_are_not_split_into_smaller_tokens()
        {
            var outer = new CliCommand("outer")
            {
                new CliCommand("inner")
                {
                    new CliArgument<string[]>("arg")
                    {
                        Arity = ArgumentArity.OneOrMore
                    }
                }
            };

            ParseResult result = CliParser.Parse(outer, "outer inner -p:RandomThing=random");

            result.CommandResultInternal
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("-p:RandomThing=random");
        }

        /*
        [Fact]
        public void The_default_behavior_of_unmatched_tokens_resulting_in_errors_can_be_turned_off()
        {
            // TODO: uses UnmatchedTokens, TreatUnmatchedTokensAsErrors, fix
            var command = new CliCommand("the-command")
            {
                new CliArgument<string>("arg")
            };
            command.TreatUnmatchedTokensAsErrors = false;

            ParseResult result = command.Parse("the-command arg1 arg2");

            result.Errors.Should().BeEmpty();

            result.UnmatchedTokens
                .Should()
                .BeEquivalentTo("arg2");
        }
        */

        [Fact]
        public void Option_and_Command_can_have_the_same_alias()
        {
            var innerCommand = new CliCommand("inner")
            {
                new CliArgument<string[]>("arg1")
            };

            var option = new CliOption<bool>("--inner");

            var outerCommand = new CliCommand("outer")
            {
                innerCommand,
                option,
                new CliArgument<string[]>("arg2")
            };

            CliParser.Parse(outerCommand, "outer inner")
                .CommandResultInternal
                .Command
                .Should()
                .BeSameAs(innerCommand);

            CliParser.Parse(outerCommand, "outer --inner")
                .CommandResultInternal
                .Command
                .Should()
                .BeSameAs(outerCommand);

            CliParser.Parse(outerCommand, "outer --inner inner")
                .CommandResultInternal
                .Command
                .Should()
                .BeSameAs(innerCommand);

            CliParser.Parse(outerCommand, "outer --inner inner")
                .CommandResultInternal
                .Parent
                .Should()
                .BeOfType<CliCommandResultInternal>()
                .Which
                .Children
                .Should()
                .Contain(o => ((CliOptionResultInternal)o).Option == option);
        }

        [Fact]
        public void Options_can_have_the_same_alias_differentiated_only_by_prefix()
        {
            var option1 = new CliOption<bool>("-a");
            var option2 = new CliOption<bool>("--a");

            var rootCommand = new CliRootCommand
            {
                option1,
                option2
            };

            CliParser.Parse(rootCommand, "-a").CommandResultInternal
                .Children
                .Select(s => ((CliOptionResultInternal)s).Option)
                .Should()
                .BeEquivalentTo(option1);
            CliParser.Parse(rootCommand, "--a").CommandResultInternal
                .Children
                .Select(s => ((CliOptionResultInternal)s).Option)
                .Should()
                .BeEquivalentTo(option2);
        }

        [Theory]
        [InlineData("-x", "\"hello\"")]
        [InlineData("-x=", "\"hello\"")]
        [InlineData("-x:", "\"hello\"")]
        [InlineData("-x", "\"\"")]
        [InlineData("-x=", "\"\"")]
        [InlineData("-x:", "\"\"")]
        public void When_an_option_argument_is_enclosed_in_double_quotes_its_value_retains_the_quotes(
            string arg1,
            string arg2)
        {
            var option = new CliOption<string[]>("-x");
            var rootCommand = new CliRootCommand { option };
            var parseResult = CliParser.Parse(rootCommand, new[] { arg1, arg2 });

            parseResult
                .GetResult(option)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo(new[] { arg2 });
        }

        // TODO: Tests tokens which is no longer exposed, and should be replaced by tests of location or removed
        /*
        [Fact] // https://github.com/dotnet/command-line-api/issues/1445
        public void Trailing_option_delimiters_are_ignored()
        {
            var rootCommand = new CliRootCommand
            {
                new CliCommand("subcommand")
                {
                    new CliOption<DirectoryInfo>("--directory")
                }
            };

            var args = new[] { "subcommand", "--directory:", @"c:\" };

            var result = CliParser.Parse(rootCommand, args);

            result.Errors.Should().BeEmpty();

            result.Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentSequenceTo(new[] { "subcommand", "--directory", @"c:\" });
        }
        */

        [Theory]
        [InlineData("-x -y")]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Option_arguments_can_start_with_prefixes_that_make_them_look_like_options(string input)
        {
            var optionX = new CliOption<string>("-x");

            var command = new CliCommand("command")
            {
                optionX,
                new CliOption<string>("-z")
            };

            var result = CliParser.Parse(command, input);

            GetValue(result, optionX).Should().Be("-y");
        }

        [Fact]
        public void Option_arguments_can_start_with_prefixes_that_make_them_look_like_bundled_options()
        {
            var optionA = new CliOption<string>("-a");
            var optionB = new CliOption<bool>("-b");
            var optionC = new CliOption<bool>("-c");

            var command = new CliRootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = CliParser.Parse(command, "-a -bc");

            GetValue(result, optionA).Should().Be("-bc");
            GetValue(result, optionB).Should().BeFalse();
            GetValue(result, optionC).Should().BeFalse();
        }

        [Fact]
        public void Option_arguments_can_match_subcommands()
        {
            var optionA = new CliOption<string>("-a");
            var rootCommand = new CliRootCommand
            {
                new CliCommand("subcommand"),
                optionA
            };

            var result = CliParser.Parse(rootCommand, "-a subcommand");

            GetValue(result, optionA).Should().Be("subcommand");
            result.CommandResultInternal.Command.Should().BeSameAs(rootCommand);
        }

        [Fact]
        public void Arguments_can_match_subcommands()
        {
            var argument = new CliArgument<string[]>("arg");
            var subcommand = new CliCommand("subcommand")
            {
                argument
            };
            var rootCommand = new CliRootCommand
            {
                subcommand
            };

            var result = CliParser.Parse(rootCommand, "subcommand one two three subcommand four");

            result.CommandResultInternal.Command.Should().BeSameAs(subcommand);

            GetValue(result, argument)
                .Should()
                .BeEquivalentSequenceTo("one", "two", "three", "subcommand", "four");
        }

        [Theory]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Option_arguments_can_match_the_aliases_of_sibling_options_when_non_space_argument_delimiter_is_used(string input)
        {
            var optionX = new CliOption<string>("-x");

            var command = new CliCommand("command")
            {
                optionX,
                new CliOption<string>("-y")
            };

            var result = CliParser.Parse(command, input);

            result.Errors.Should().BeEmpty();
            GetValue(result, optionX).Should().Be("-y");
        }

        [Fact]
        public void Single_option_arguments_that_match_option_aliases_are_parsed_correctly()
        {
            var optionX = new CliOption<string>("-x");

            var command = new CliRootCommand
            {
                optionX
            };

            var result = CliParser.Parse(command, "-x -x");

            GetValue(result, optionX).Should().Be("-x");
        }

        [Theory]
        [InlineData("-x -y")]
        [InlineData("-x true -y")]
        [InlineData("-x:true -y")]
        [InlineData("-x=true -y")]
        [InlineData("-x -y true")]
        [InlineData("-x true -y true")]
        [InlineData("-x:true -y:true")]
        [InlineData("-x=true -y:true")]
        public void Boolean_options_are_not_greedy(string commandLine)
        {
            var optX = new CliOption<bool>("-x");
            var optY = new CliOption<bool>("-y");

            var root = new CliRootCommand()
            {
                optX,
                optY,
            };

            var result = CliParser.Parse(root, commandLine);

            result.Errors.Should().BeEmpty();

            GetValue(result, optX).Should().BeTrue();
            GetValue(result, optY).Should().BeTrue();
        }

        [Fact]
        public void Multiple_option_arguments_that_match_multiple_arity_option_aliases_are_parsed_correctly()
        {
            var optionX = new CliOption<string[]>("-x");
            var optionY = new CliOption<string[]>("-y");

            var command = new CliRootCommand
            {
                optionX,
                optionY
            };

            var result = CliParser.Parse(command, "-x -x -x -y -y -x -y -y -y -x -x -y");

            GetValue(result, optionX).Should().BeEquivalentTo(new[] { "-x", "-y", "-y" });
            GetValue(result, optionY).Should().BeEquivalentTo(new[] { "-x", "-y", "-x" });
        }

        [Fact]
        public void Bundled_option_arguments_that_match_option_aliases_are_parsed_correctly()
        {
            var optionX = new CliOption<string>("-x");
            var optionY = new CliOption<bool>("-y");

            var command = new CliRootCommand
            {
                optionX,
                optionY
            };

            var result = CliParser.Parse(command, "-yxx");

            GetValue(result, optionX).Should().Be("x");
        }

        [Fact]
        public void Argument_name_is_not_matched_as_a_token()
        {
            var nameArg = new CliArgument<string>("name");
            var columnsArg = new CliArgument<IEnumerable<string>>("columns");

            var command = new CliCommand("add")
            {
                nameArg,
                columnsArg
            };

            var result = CliParser.Parse(command, "name one two three");

            GetValue(result, nameArg).Should().Be("name");
            GetValue(result, columnsArg).Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Option_aliases_do_not_need_to_be_prefixed()
        {
            var option = new CliOption<bool>("noprefix");

            var rootCommand = new CliRootCommand { option };
            var result = CliParser.Parse(rootCommand, "noprefix");

            result.GetResult(option).Should().NotBeNull();
        }

        [Fact]
        public void Boolean_options_with_no_argument_specified_do_not_match_subsequent_arguments()
        {
            var option = new CliOption<bool>("-v");

            var command = new CliCommand("command")
            {
                option
            };

            var result = CliParser.Parse(command, "-v an-argument");

            GetValue(result, option).Should().BeTrue();
        }

        /*
        [Fact]
        public void When_a_command_line_has_unmatched_tokens_they_are_not_applied_to_subsequent_options()
        {
        // TODO: uses TreatUnmatchedTokensAsErrors, fix
            var command = new CliCommand("command")
            {
                TreatUnmatchedTokensAsErrors = false
            };
            var optionX = new CliOption<string>("-x");
            command.Options.Add(optionX);
            var optionY = new CliOption<string>("-y");
            command.Options.Add(optionY);

            var result = command.Parse("-x 23 unmatched-token -y 42");

            GetValue(result, optionX).Should().Be("23");
            GetValue(result, optionY).Should().Be("42");
            result.UnmatchedTokens.Should().BeEquivalentTo("unmatched-token");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void When_a_command_line_has_unmatched_tokens_the_parse_result_action_should_depend_on_parsed_command_TreatUnmatchedTokensAsErrors(bool treatUnmatchedTokensAsErrors)
        {
            // TODO: uses TreatUnmatchedTokensAsErrors, fix
            CliRootCommand rootCommand = new();
            CliCommand subcommand = new("vstest")
            {
                new CliOption<string>("--Platform"),
                new CliOption<string>("--Framework"),
                new CliOption<string[]>("--logger")
            };
            subcommand.TreatUnmatchedTokensAsErrors = treatUnmatchedTokensAsErrors;
            rootCommand.Subcommands.Add(subcommand);

            var result = rootCommand.Parse("vstest test1.dll test2.dll");

            result.UnmatchedTokens.Should().BeEquivalentTo("test1.dll", "test2.dll");

            if (treatUnmatchedTokensAsErrors)
            {
                result.Errors.Should().NotBeEmpty();
                result.Action.Should().NotBeSameAs(result.CommandResultInternal.Command.Action);
            }
            else
            {
                result.Errors.Should().BeEmpty();
                result.Action.Should().BeSameAs(result.CommandResultInternal.Command.Action);
            }
        }

        [Fact]
        public void RootCommand_TreatUnmatchedTokensAsErrors_set_to_false_has_precedence_over_subcommands()
        {
            // TODO: uses TreatUnmatchedTokensAsErrors, fix
            CliRootCommand rootCommand = new();
            rootCommand.TreatUnmatchedTokensAsErrors = false;
            CliCommand subcommand = new("vstest")
            {
                new CliOption<string>("--Platform"),
                new CliOption<string>("--Framework"),
                new CliOption<string[]>("--logger")
            };
            subcommand.TreatUnmatchedTokensAsErrors = true; // the default, set to true to make it explicit
            rootCommand.Subcommands.Add(subcommand);

            var result = rootCommand.Parse("vstest test1.dll test2.dll");

            result.UnmatchedTokens.Should().BeEquivalentTo("test1.dll", "test2.dll");

            result.Errors.Should().BeEmpty();
            result.Action.Should().BeSameAs(result.CommandResultInternal.Command.Action);
        }
        */

        [Fact]
        public void Parse_can_not_be_called_with_null_args()
        {
            Action passNull = () => CliParser.Parse(new CliRootCommand(), args: null);

            passNull.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Command_argument_arity_can_be_a_fixed_value_greater_than_1()
        {
            var argument = new CliArgument<string[]>("arg")
            {
                Arity = new ArgumentArity(3, 3)
            };
            var command = new CliCommand("the-command")
            {
                argument
            };

            CliParser.Parse(command, "1 2 3")
                .CommandResultInternal
                .Tokens
                .Should()
                .BeEquivalentTo(
                    new CliToken("1", CliTokenType.Argument, argument, dummyLocation),
                    new CliToken("2", CliTokenType.Argument, argument, dummyLocation),
                    new CliToken("3", CliTokenType.Argument, argument, dummyLocation));
        }

        [Fact]
        public void Command_argument_arity_can_be_a_range_with_a_lower_bound_greater_than_1()
        {
            var argument = new CliArgument<string[]>("arg")
            {
                Arity = new ArgumentArity(3, 5)
            };
            var command = new CliCommand("the-command")
            {
                argument
            };

            CliParser.Parse(command, "1 2 3")
                .CommandResultInternal
                .Tokens
                .Should()
                .BeEquivalentTo(
                    new CliToken("1", CliTokenType.Argument, argument, dummyLocation),
                    new CliToken("2", CliTokenType.Argument, argument, dummyLocation),
                    new CliToken("3", CliTokenType.Argument, argument, dummyLocation));
            CliParser.Parse(command, "1 2 3 4 5")
                .CommandResultInternal
                .Tokens
                .Should()
                .BeEquivalentTo(
                    new CliToken("1", CliTokenType.Argument, argument, dummyLocation),
                    new CliToken("2", CliTokenType.Argument, argument, dummyLocation),
                    new CliToken("3", CliTokenType.Argument, argument, dummyLocation),
                    new CliToken("4", CliTokenType.Argument, argument, dummyLocation),
                    new CliToken("5", CliTokenType.Argument, argument, dummyLocation));
        }

        [Fact(Skip ="Waiting for CliError work")]
        public void When_command_arguments_are_fewer_than_minimum_arity_then_an_error_is_returned()
        {
            var command = new CliCommand("the-command")
            {
                new CliArgument<string[]>("arg")
                {
                    Arity = new ArgumentArity(2, 3)
                }
            };

            var result = CliParser.Parse(command, "1");

            result.Errors
                .Select(e => e.Message)
                .Should()
                .Contain(LocalizationResources.RequiredArgumentMissing(result.GetResult(command.Arguments[0])));
        }

        [Fact]
        public void When_command_arguments_are_greater_than_maximum_arity_then_an_error_is_returned()
        {
            var command = new CliCommand("the-command")
            {
                new CliArgument<string[]>("arg")
                {
                    Arity = new ArgumentArity(2, 3)
                }
            };

            ParseResult parseResult = CliParser.Parse(command, "1 2 3 4");

            parseResult
                .Errors
                .Select(e => e.Message)
                .Should()
                .Contain(LocalizationResources.UnrecognizedCommandOrArgument("4"));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_fixed_value_greater_than_1()
        {
            var option = new CliOption<int[]>("-x") { Arity = new ArgumentArity(3, 3) };

            var command = new CliCommand("the-command")
            {
                option
            };

            CliParser.Parse(command, "-x 1 -x 2 -x 3")
                .GetResult(option)
                .Tokens
                .Should()
                .BeEquivalentTo(
                    new CliToken("1", CliTokenType.Argument, default, dummyLocation),
                    new CliToken("2", CliTokenType.Argument, default, dummyLocation),
                    new CliToken("3", CliTokenType.Argument, default, dummyLocation));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_range_with_a_lower_bound_greater_than_1()
        {
            var option = new CliOption<string[]>("-x") { Arity = new ArgumentArity(3, 5) };

            var command = new CliCommand("the-command")
            {
                option
            };

            CliParser.Parse(command, "-x 1 -x 2 -x 3")
                .GetResult(option)
                .Tokens
                .Should()
                .BeEquivalentTo(
                    new CliToken("1", CliTokenType.Argument, default, dummyLocation),
                    new CliToken("2", CliTokenType.Argument, default, dummyLocation),
                    new CliToken("3", CliTokenType.Argument, default, dummyLocation));
            CliParser.Parse(command, "-x 1 -x 2 -x 3 -x 4 -x 5")
                .GetResult(option)
                .Tokens
                .Should()
                .BeEquivalentTo(
                    new CliToken("1", CliTokenType.Argument, default, dummyLocation),
                    new CliToken("2", CliTokenType.Argument, default, dummyLocation),
                    new CliToken("3", CliTokenType.Argument, default, dummyLocation),
                    new CliToken("4", CliTokenType.Argument, default, dummyLocation),
                    new CliToken("5", CliTokenType.Argument, default, dummyLocation));
        }

        [Fact(Skip = "Waiting for CliError work")]
        public void When_option_arguments_are_fewer_than_minimum_arity_then_an_error_is_returned()
        {
            var option = new CliOption<int[]>("-x")
            {
                Arity = new ArgumentArity(2, 3)
            };

            var command = new CliCommand("the-command")
            {
                option
            };

            var result = CliParser.Parse(command, "-x 1");

            result.Errors
                .Select(e => e.Message)
                .Should()
                .Contain(LocalizationResources.RequiredArgumentMissing(result.GetResult(option)));
        }

        [Fact]
        public void When_option_arguments_are_greater_than_maximum_arity_then_an_error_is_returned()
        {
            var command = new CliCommand("the-command")
            {
                new CliOption<int[]>("-x") { Arity = new ArgumentArity(2, 3)}
            };

            CliParser.Parse(command, "-x 1 2 3 4")
                .Errors
                .Select(e => e.Message)
                .Should()
                .Contain(LocalizationResources.UnrecognizedCommandOrArgument("4"));
        }

        // TODO: Tests tokens which is no longer exposed, and should be replaced with equivalent test using ParseResult
        /*
        [Fact]
        public void Tokens_are_not_split_if_the_part_before_the_delimiter_is_not_an_option()
        {
            var rootCommand = new CliCommand("jdbc");
            rootCommand.Add(new CliOption<string>("url"));
            var result = CliParser.Parse(rootCommand, "jdbc url \"jdbc:sqlserver://10.0.0.2;databaseName=main\"");

            result.Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("url",
                                "jdbc:sqlserver://10.0.0.2;databaseName=main");
        }
        */
        /*
        [Fact]
        public void A_subcommand_wont_overflow_when_checking_maximum_argument_capacity()
        {
            // TODO: uses GetCompletions, fix
            // Tests bug identified in https://github.com/dotnet/command-line-api/issues/997

            var argument1 = new CliArgument<string>("arg1");

            var argument2 = new CliArgument<string[]>("arg2");

            var command = new CliCommand("subcommand")
            {
                argument1,
                argument2
            };

            var rootCommand = new CliRootCommand
            {
                command
            };

            var parseResult = rootCommand.Parse("subcommand arg1 arg2");

            Action act = () => parseResult.GetCompletions();
            act.Should().NotThrow();
        }
    */

        [Theory] // https://github.com/dotnet/command-line-api/issues/1551, https://github.com/dotnet/command-line-api/issues/1533
        [InlineData("--exec-prefix", "")]
        [InlineData("--exec-prefix:", "")]
        [InlineData("--exec-prefix=", "")]
        public void Parsed_value_of_empty_string_arg_is_an_empty_string(string arg1, string arg2)
        {
            var option = new CliOption<string>("--exec-prefix")
            {
                DefaultValueFactory = _ => "/usr/local"
            };

            var rootCommand = new CliRootCommand
            {
                option
            };

            var result = CliParser.Parse(rootCommand, new[] { arg1, arg2 });

            GetValue(result, option).Should().BeEmpty();
        }

        // TODO: Tests below are from Powderhouse. Consider whether this the right location considering how large the file is
        [Fact]
        public void CommandResult_contains_argument_ValueResults()
        {
            var argument1 = new CliArgument<string>("arg1");
            var argument2 = new CliArgument<string>("arg2");
            var command = new CliCommand("subcommand")
            {
                argument1,
                argument2
            };
            var rootCommand = new CliRootCommand
            {
                command
            };

            var parseResult = CliParser.Parse(rootCommand, "subcommand Kirk Spock");

            var commandResult = parseResult.CommandResult;
            commandResult.ValueResults.Should().HaveCount(2);
            var result1 = commandResult.ValueResults[0];
            result1.GetValue<string>().Should().Be("Kirk");
            var result2 = commandResult.ValueResults[1];
            result2.GetValue<string>().Should().Be("Spock");
        }

        [Fact]
        public void CommandResult_contains_option_ValueResults()
        {
            var argument1 = new CliOption<string>("--opt1");
            var argument2 = new CliOption<string>("--opt2");
            var command = new CliCommand("subcommand")
            {
                argument1,
                argument2
            };
            var rootCommand = new CliRootCommand
            {
                command
            };

            var parseResult = CliParser.Parse(rootCommand, "subcommand arg1 --opt1 Kirk --opt2 Spock");

            var commandResult = parseResult.CommandResult;
            commandResult.ValueResults.Should().HaveCount(2);
            var result1 = commandResult.ValueResults[0];
            result1.GetValue<string>().Should().Be("Kirk");
            var result2 = commandResult.ValueResults[1];
            result2.GetValue<string>().Should().Be("Spock");
        }

        [Fact]
        public void Location_in_ValueResult_correct_for_arguments()
        {
            var argument1 = new CliArgument<string>("arg1");
            var argument2 = new CliArgument<string>("arg2");
            var command = new CliCommand("subcommand")
            {
                argument1,
                argument2
            };
            var rootCommand = new CliRootCommand
            {
                command
            };
            var expectedOuterLocation = new Location(CliExecutable.ExecutableName, Location.User, -1, null);
            var expectedLocation1 = new Location("Kirk", Location.User, 1, expectedOuterLocation);
            var expectedLocation2 = new Location("Spock", Location.User, 2, expectedOuterLocation);

            var parseResult = CliParser.Parse(rootCommand, "subcommand Kirk Spock");

            var commandResult = parseResult.CommandResult;
            var result1 = commandResult.ValueResults[0];
            var result2 = commandResult.ValueResults[1];
            result1.Locations.Single().Should().Be(expectedLocation1);
            result2.Locations.Single().Should().Be(expectedLocation2);
        }

        [Fact]
        public void Location_in_ValueResult_correct_for_options()
        {
            var option1 = new CliOption<string>("--opt1");
            var option2 = new CliOption<string>("--opt2");
            var command = new CliCommand("subcommand")
            {
                option1,
                option2
            };
            var rootCommand = new CliRootCommand
            {
                command
            };
            var expectedOuterLocation = new Location(CliExecutable.ExecutableName, Location.User, -1, null);
            var expectedLocation1 = new Location("Kirk", Location.User, 3, expectedOuterLocation);
            var expectedLocation2 = new Location("Spock", Location.User, 5, expectedOuterLocation);

            var parseResult = CliParser.Parse(rootCommand, "subcommand arg1 --opt1 Kirk --opt2 Spock");

            var commandResult = parseResult.CommandResult;
            var result1 = commandResult.ValueResults[0];
            var result2 = commandResult.ValueResults[1];
            result1.Locations.Single().Should().Be(expectedLocation1);
            result2.Locations.Single().Should().Be(expectedLocation2);
        }

        [Fact]
        public void Location_offsets_in_ValueResult_correct_for_arguments()
        {
            var argument1 = new CliArgument<string[]>("arg1");

            var command = new CliCommand("subcommand")
            {
                argument1,
            };

            var rootCommand = new CliRootCommand
            {
                command
            };
            var expectedOuterLocation = new Location(CliExecutable.ExecutableName, Location.User, -1, null);
            var expectedLocation1 = new Location("Kirk", Location.User, 1, expectedOuterLocation);
            var expectedLocation2 = new Location("Spock", Location.User, 2, expectedOuterLocation);

            var parseResult = CliParser.Parse(rootCommand, "subcommand Kirk Spock");

            var commandResult = parseResult.CommandResult;
            var result1 = commandResult.ValueResults.Single();
            result1.Locations.First().Should().Be(expectedLocation1);
            result1.Locations.Skip(1).Single().Should().Be(expectedLocation2);
        }

        [Fact]
        public void Location_offsets_in_ValueResult_correct_for_options()
        {
            var option1 = new CliOption<string[]>("--opt1");
            var command = new CliCommand("subcommand")
            {
                option1,
            };
            var rootCommand = new CliRootCommand
            {
                command
            };
            var expectedOuterLocation = new Location(CliExecutable.ExecutableName, Location.User, -1, null);
            var expectedLocation1 = new Location("Kirk", Location.User, 3, expectedOuterLocation);
            var expectedLocation2 = new Location("Spock", Location.User, 5, expectedOuterLocation);

            var parseResult = CliParser.Parse(rootCommand, "subcommand arg1 --opt1 Kirk --opt1 Spock");

            var commandResult = parseResult.CommandResult;
            var result1 = commandResult.ValueResults.Single();
            result1.Locations.First().Should().Be(expectedLocation1);
            result1.Locations.Skip(1).Single().Should().Be(expectedLocation2);
        }

        [Fact]
        public void Location_offset_correct_when_colon_or_equal_used()
        {
            var option1 = new CliOption<string>("--opt1");
            var option2 = new CliOption<string>("--opt11");
            var command = new CliCommand("subcommand")
            {
                option1,
                option2
            };
            var rootCommand = new CliRootCommand
            {
                command
            };
            var expectedOuterLocation = new Location(CliExecutable.ExecutableName, Location.User, -1, null);
            var expectedLocation1 = new Location("Kirk", Location.User, 2, expectedOuterLocation, 7);
            var expectedLocation2 = new Location("Spock", Location.User, 3, expectedOuterLocation, 8);

            var parseResult = CliParser.Parse(rootCommand, "subcommand arg1 --opt1:Kirk --opt11=Spock");

            var commandResult = parseResult.CommandResult;
            var result1 = commandResult.ValueResults[0];
            var result2 = commandResult.ValueResults[1];
            result1.Locations.Single().Should().Be(expectedLocation1);
            result2.Locations.Single().Should().Be(expectedLocation2);
        }

        [Fact]
        public void Locations_correct_for_collection()
        {
            var option1 = new CliOption<string[]>("--opt1");
            option1.AllowMultipleArgumentsPerToken = true;
            var rootCommand = new CliRootCommand
                    {
                        option1
                    };
            var expectedOuterLocation = new Location(CliExecutable.ExecutableName, Location.User, -1, null);
            var expectedLocation1 = new Location("Kirk", Location.User, 2, expectedOuterLocation);
            var expectedLocation2 = new Location("Spock", Location.User, 3, expectedOuterLocation);
            var expectedLocation3 = new Location("Uhura", Location.User, 4, expectedOuterLocation);

            var parseResult = CliParser.Parse(rootCommand, "subcommand --opt1 Kirk Spock Uhura");

            var result = parseResult.GetValueResult(option1);
            result.Locations.Should().BeEquivalentTo([expectedLocation1, expectedLocation2, expectedLocation3]);
        }

        [Fact]
        public void ParseResult_contains_argument_ValueResults()
        {
            var argument1 = new CliArgument<string>("arg1");
            var argument2 = new CliArgument<string>("arg2");
            var command = new CliCommand("subcommand")
            {
                argument1,
                argument2
            };
            var rootCommand = new CliRootCommand
            {
                command
            };

            var parseResult = CliParser.Parse(rootCommand, "subcommand Kirk Spock");


            var result1 = parseResult.GetValueResult(argument1);
            var result2 = parseResult.GetValueResult(argument2);
            result1.GetValue<string>().Should().Be("Kirk");
            result2.GetValue<string>().Should().Be("Spock");
        }

        [Fact]
        public void ParseResult_contains_option_ValueResults()
        {
            var option1 = new CliOption<string>("--opt1");
            var option2 = new CliOption<string>("--opt2");
            var command = new CliCommand("subcommand")
            {
                option1,
                option2
            };
            var rootCommand = new CliRootCommand
            {
                command
            };

            var parseResult = CliParser.Parse(rootCommand, "subcommand arg1 --opt1 Kirk --opt2 Spock");


            var result1 = parseResult.GetValueResult(option1);
            var result2 = parseResult.GetValueResult(option2);
            result1.GetValue<string>().Should().Be("Kirk");
            result2.GetValue<string>().Should().Be("Spock");
        }
    }
}
