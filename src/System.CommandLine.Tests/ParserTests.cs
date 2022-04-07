﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
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
    {public partial class RootCommandAndArg0
        {
        }
        private readonly ITestOutputHelper _output;

        public ParserTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void An_option_can_be_checked_by_object_instance()
        {
            var option = new Option<bool>("--flag");
            var option2 = new Option<bool>("--flag2");
            var result = new Parser(new RootCommand { option, option2 })
                .Parse("--flag");

            result.HasOption(option).Should().BeTrue();
            result.HasOption(option2).Should().BeFalse();
        }

        [Fact]
        public void Two_options_are_parsed_correctly()
        {
            var optionOne = new Option<bool>(new[] { "-o", "--one" });

            var optionTwo = new Option<bool>(new[] { "-t", "--two" });

            var result = new Parser(
                    new RootCommand
                    {
                        optionOne,
                        optionTwo
                    })
                .Parse("-o -t");

            result.HasOption(optionOne).Should().BeTrue();
            result.HasOption(optionTwo).Should().BeTrue();
        }

        [Theory]
        [InlineData("-")]
        [InlineData("/")]
        public void When_a_token_is_just_a_prefix_then_an_error_is_returned(string prefix)
        {
            var parser = new Parser(new RootCommand());

            var result = parser.Parse(prefix);

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain(LocalizationResources.Instance.UnrecognizedCommandOrArgument(prefix));
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_equals_delimiter()
        {
            var option = new Option<string>("-x");

            var result = option.Parse("-x=some-value");

            result.Errors.Should().BeEmpty();

            result.FindResultFor(option).Tokens.Should().ContainSingle(a => a.Value == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_equals_delimiter()
        {
            var option = new Option<string>("--hello");

            var result = option.Parse("--hello=there");

            result.Errors.Should().BeEmpty();

            result.FindResultFor(option).Tokens.Should().ContainSingle(a => a.Value == "there");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_colon_delimiter()
        {
            var option = new Option<string>("-x");

            var result = option.Parse("-x:some-value");

            result.Errors.Should().BeEmpty();

            result.FindResultFor(option).Tokens.Should().ContainSingle(a => a.Value == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_colon_delimiter()
        {
            var option = new Option<string>("--hello");

            var result = option.Parse("--hello:there");

            result.Errors.Should().BeEmpty();

            result.FindResultFor(option).Tokens.Should().ContainSingle(a => a.Value == "there");
        }

        [Fact]
        public void Option_short_forms_can_be_bundled()
        {
            var command = new Command("the-command");
            command.AddOption(new Option<bool>("-x"));
            command.AddOption(new Option<bool>("-y"));
            command.AddOption(new Option<bool>("-z"));

            var result = command.Parse("the-command -xyz");

            result.CommandResult
                  .Children
                  .Select(o => o.Symbol.Name)
                  .Should()
                  .BeEquivalentTo("x", "y", "z");
        }

        [Fact]
        public void Options_short_forms_do_not_get_unbundled_if_unbundling_is_turned_off()
        {
            var parser = new CommandLineBuilder(new RootCommand
                         {
                             new Command("the-command")
                             {
                                 new Option<bool>("-x"),
                                 new Option<bool>("-y"),
                                 new Option<bool>("-z")
                             }
                         })
                         .EnablePosixBundling(false)
                         .Build();

            var result = parser.Parse("the-command -xyz");

            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo("-xyz");
        }

        [Fact]
        public void Option_long_forms_do_not_get_unbundled()
        {
            var parser = new Parser(
                new Command("the-command")
                {
                    new Option<bool>("--xyz"),
                    new Option<bool>("-x"),
                    new Option<bool>("-y"),
                    new Option<bool>("-z")
                });

            var result = parser.Parse("the-command --xyz");

            result.CommandResult
                  .Children
                  .Select(o => o.Symbol.Name)
                  .Should()
                  .BeEquivalentTo("xyz");
        }

        [Fact]
        public void Options_do_not_get_unbundled_unless_all_resulting_options_would_be_valid_for_the_current_command()
        {
            var outer = new Command("outer");
            outer.AddOption(new Option<bool>("-a"));
            var inner = new Command("inner")
            {
                new Argument<string[]>()
            };
            inner.AddOption(new Option<bool>("-b"));
            inner.AddOption(new Option<bool>("-c"));
            outer.AddCommand(inner);

            var parser = new Parser(outer);

            ParseResult result = parser.Parse("outer inner -abc");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("-abc");
        }

        [Fact]
        public void Required_option_arguments_are_not_unbundled()
        {
            var optionA = new Option<string>("-a");
            var optionB = new Option<bool>("-b");
            var optionC = new Option<bool>("-c");

            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse("-a -bc");

            result.FindResultFor(optionA)
                  .Tokens
                  .Should()
                  .ContainSingle(t => t.Value == "-bc");
        }
    
        [Fact]
        public void Last_bundled_option_can_accept_argument_with_no_separator()
        {
            var optionA = new Option<bool>("-a");
            var optionB = new Option<string>("-b") { Arity = ArgumentArity.ZeroOrOne };
            var optionC = new Option<string>("-c") { Arity = ArgumentArity.ExactlyOne };

            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse("-abcvalue");
            result.HasOption(optionA).Should().BeTrue();
            result.HasOption(optionB).Should().BeTrue();

            result.FindResultFor(optionC)
                .Tokens
                .Should()
                .ContainSingle(t => t.Value == "value");
        }

        [Fact]
        public void Last_bundled_option_can_accept_argument_with_equals_separator()
        {
            var optionA = new Option<bool>("-a");
            var optionB = new Option<string>("-b") { Arity = ArgumentArity.ZeroOrOne };
            var optionC = new Option<string>("-c") { Arity = ArgumentArity.ExactlyOne };

            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse("-abc=value");
            result.HasOption(optionA).Should().BeTrue();
            result.HasOption(optionB).Should().BeTrue();

            result.FindResultFor(optionC)
                .Tokens
                .Should()
                .ContainSingle(t => t.Value == "value");
        }

        [Fact]
        public void Last_bundled_option_can_accept_argument_with_colon_separator()
        {
            var optionA = new Option<bool>("-a");
            var optionB = new Option<string>("-b") { Arity = ArgumentArity.ZeroOrOne };
            var optionC = new Option<string>("-c") { Arity = ArgumentArity.ExactlyOne };

            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse("-abc:value");
            result.HasOption(optionA).Should().BeTrue();
            result.HasOption(optionB).Should().BeTrue();

            result.FindResultFor(optionC)
                .Tokens
                .Should()
                .ContainSingle(t => t.Value == "value");
        }

        [Fact]
        public void Invalid_char_in_bundle_causes_rest_to_be_interpreted_as_value()
        {
            var optionA = new Option<bool>("-a");
            var optionB = new Option<string>("-b") { Arity = ArgumentArity.ZeroOrOne };
            var optionC = new Option<string>("-c") { Arity = ArgumentArity.ExactlyOne };

            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse("-abvcalue");
            result.HasOption(optionA).Should().BeTrue();
            result.HasOption(optionB).Should().BeTrue();

            result.FindResultFor(optionB)
                .Tokens
                .Should()
                .ContainSingle(t => t.Value == "vcalue");

            result.HasOption(optionC).Should().BeFalse();
        }

        [Fact]
        public void Parser_root_Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
            var animalsOption = new Option<string[]>(new[] { "-a", "--animals" });
            var vegetablesOption = new Option<string[]>(new[] { "-v", "--vegetables" });
            var parser = new RootCommand
            {
                animalsOption,
                vegetablesOption
            };

            var result = parser.Parse("-a cat -v carrot -a dog");

            result.FindResultFor(animalsOption)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("cat", "dog");

            result.FindResultFor(vegetablesOption)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo("carrot");
        }

        [Fact]
        public void Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
            var animalsOption = new Option<string[]>(new[] { "-a", "--animals" })
                .FromAmong("dog", "cat", "sheep");
            var vegetablesOption = new Option<string[]>(new[] { "-v", "--vegetables" });
            var parser = new Parser(
                new Command("the-command") {
                    animalsOption,
                    vegetablesOption
                });

            var result = parser.Parse("the-command -a cat -v carrot -a dog");

            result.FindResultFor(animalsOption)
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("cat", "dog");

            result.FindResultFor(vegetablesOption)
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("carrot");
        }

        [Fact]
        public void When_an_option_is_not_respecified_but_limit_is_reached_then_the_following_token_is_considered_an_argument_to_the_parent_command()
        {
            var animalsOption = new Option<string[]>(new[] { "-a", "--animals" });

            var vegetablesOption = new Option<string[]>(new[] { "-v", "--vegetables" });
            
            var parser = new Parser(
                new Command("the-command")
                {
                    animalsOption,
                    vegetablesOption,
                    new Argument<string[]>()
                });

            var result = parser.Parse("the-command -a cat some-arg -v carrot");

            result.FindResultFor(animalsOption)
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("cat");

            result.FindResultFor(vegetablesOption)
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("carrot");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("some-arg");
        }

        [Fact]
        public void Command_with_multiple_options_is_parsed_correctly()
        {
            var option = new Command("outer")
            {
                new Option<string>("--inner1"),
                new Option<string>("--inner2")
            };

            var parser = new Parser(option);

            var result = parser.Parse("outer --inner1 argument1 --inner2 argument2");

            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o =>
                                     o.Symbol.Name == "inner1" &&
                                     o.Tokens.Single().Value == "argument1");
            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o =>
                                     o.Symbol.Name == "inner2" &&
                                     o.Tokens.Single().Value == "argument2");
        }

        [Fact]
        public void Relative_order_of_arguments_and_options_within_a_command_does_not_matter()
        {
            var command = new Command("move")
            {
                new Argument<string[]>(),
                new Option<string>("-X")
            };

            // option before args
            ParseResult result1 = command.Parse(
                "move -X the-arg-for-option-x ARG1 ARG2");

            // option between two args
            ParseResult result2 = command.Parse(
                "move ARG1 -X the-arg-for-option-x ARG2");

            // option after args
            ParseResult result3 = command.Parse(
                "move ARG1 ARG2 -X the-arg-for-option-x");

            // all should be equivalent
            result1.Should()
                   .BeEquivalentTo(
                       result2,
                       x => x.IgnoringCyclicReferences()
                             .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal)));
            result1.Should()
                   .BeEquivalentTo(
                       result3,
                       x => x.IgnoringCyclicReferences()
                             .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal)));
        }

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
            var rawSplit = CommandLineStringSplitter.Instance.Split(commandLine);

            var command = new Command("the-command")
                          {
                              new Argument<string[]>(),
                              new Option<string>("--one"),
                              new Option<string[]>("--many")
                          };

            var result = command.Parse(commandLine);

            result.Tokens.Select(t => t.Value).Should().Equal(rawSplit);
        }

        [Fact]
        public void An_outer_command_with_the_same_name_does_not_capture()
        {
            var command = new Command("one")
                          {
                              new Command("two")
                              {
                                  new Command("three")
                              },
                              new Command("three")
                          };

            ParseResult result = command.Parse("one two three");

            result.Diagram().Should().Be("[ one [ two [ three ] ] ]");
        }

        [Fact]
        public void An_inner_command_with_the_same_name_does_not_capture()
        {
            var command = new Command("one")
                          {
                              new Command("two")
                              {
                                  new Command("three")
                              },
                              new Command("three")
                          };

            ParseResult result = command.Parse("one three");

            result.Diagram().Should().Be("[ one [ three ] ]");
        }

        [Fact]
        public void When_nested_commands_all_accept_arguments_then_the_nearest_captures_the_arguments()
        {
            var command = new Command(
                "outer")
            {
                new Argument<string[]>(),
                new Command("inner")
                {
                    new Argument<string[]>()
                }
            };

            var result = command.Parse("outer arg1 inner arg2");

            result.CommandResult
                  .Parent
                  .Tokens.Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("arg1");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("arg2");
        }

        [Fact]
        public void Nested_commands_with_colliding_names_cannot_both_be_applied()
        {
            var command = new Command("outer")
            {
                new Argument<string>(),
                new Command("non-unique")
                {
                    new Argument<string>()
                },
                new Command("inner")
                {
                    new Argument<string>(),
                    new Command("non-unique")
                    {
                        new Argument<string>()
                    }
                }
            };

            ParseResult result = command.Parse("outer arg1 inner arg2 non-unique arg3 ");

            result.Diagram().Should().Be("[ outer <arg1> [ inner <arg2> [ non-unique <arg3> ] ] ]");
        }

        [Fact]
        public void When_child_option_will_not_accept_arg_then_parent_can()
        {
            var option = new Option<bool>("-x");
            var command = new Command("the-command")
                         {
                             option,
                             new Argument<string>()
                         };

            var result = command.Parse("the-command -x the-argument");

            var optionResult = result.FindResultFor(option);
            optionResult.Tokens.Should().BeEmpty();
            result.CommandResult.Tokens.Select(t => t.Value).Should().BeEquivalentTo("the-argument");
        }

        [Fact]
        public void When_parent_option_will_not_accept_arg_then_child_can()
        {
            var option = new Option<string>("-x");
            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command -x the-argument");

            result.FindResultFor(option).Tokens.Select(t => t.Value).Should().BeEquivalentTo("the-argument");
            result.CommandResult.Tokens.Should().BeEmpty();
        }

        [Fact]
        public void Required_arguments_on_parent_commands_do_not_create_parse_errors_when_an_inner_command_is_specified()
        {
            var child = new Command("child");

            var parent = new RootCommand
            {
                new Argument<string>(),
                child
            };
            parent.Name = "parent";

            var result = parent.Parse("child");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Required_arguments_on_grandparent_commands_do_not_create_parse_errors_when_an_inner_command_is_specified()
        {
            var grandchild = new Command("grandchild");

            var grandparent = new RootCommand
            {
                new Argument<string>(),
                new Command("parent")
                {
                    grandchild
                }
            };
            grandparent.Name = "grandparent";

            var result = grandparent.Parse("parent grandchild");

            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void When_options_with_the_same_name_are_defined_on_parent_and_child_commands_and_specified_at_the_end_then_it_attaches_to_the_inner_command()
        {
            var outer = new Command("outer")
                        {
                            new Command("inner")
                            {
                                new Option<bool>("-x")
                            },
                            new Option<bool>("-x")
                        };

            ParseResult result = outer.Parse("outer inner -x");

            result.CommandResult
                  .Parent
                  .Children
                  .Should()
                  .NotContain(o => o.Symbol.Name == "x");
            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o => o.Symbol.Name == "x");
        }

        [Fact]
        public void When_options_with_the_same_name_are_defined_on_parent_and_child_commands_and_specified_in_between_then_it_attaches_to_the_outer_command()
        {
            var outer = new Command("outer");
            outer.AddOption(new Option<bool>("-x"));
            var inner = new Command("inner");
            inner.AddOption(new Option<bool>("-x"));
            outer.AddCommand(inner);

            var result = outer.Parse("outer -x inner");

            result.CommandResult
                  .Children
                  .Should()
                  .BeEmpty();
            result.CommandResult
                  .Parent
                  .Children
                  .Should()
                  .ContainSingle(o => o.Symbol.Name == "x");
        }

        [Fact]
        public void Arguments_only_apply_to_the_nearest_command()
        {
            var outer = new Command("outer")
            {
                new Argument<string>(),
                new Command("inner")
                {
                    new Argument<string>()
                }
            };

            ParseResult result = outer.Parse("outer inner arg1 arg2");

            result.CommandResult
                  .Parent
                  .Tokens
                  .Should()
                  .BeEmpty();
            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("arg1");
            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo("arg2");
        }

        [Fact]
        public void Options_only_apply_to_the_nearest_command()
        {
            var outerOption = new Option<string>("-x");
            var innerOption = new Option<string>("-x");

            var outer = new Command("outer")
                        {
                            new Command("inner")
                            {
                                innerOption
                            },
                            outerOption
                        };

            var result = outer.Parse("outer inner -x one -x two");

            result.RootCommandResult
                  .FindResultFor(outerOption)
                  .Should()
                  .BeNull();
        }

        [Fact]
        public void Subsequent_occurrences_of_tokens_matching_command_names_are_parsed_as_arguments()
        {
            var command = new Command("the-command")
            {
                new Command("complete")
                {
                    new Argument<string>(),
                    new Option<int>("--position")
                }
            };

            ParseResult result = command.Parse("the-command",
                                               "complete",
                                               "--position",
                                               "7",
                                               "the-command");

            CommandResult completeResult = result.CommandResult;

            completeResult.Tokens.Select(t => t.Value).Should().BeEquivalentTo("the-command");
        }

        [Fact]
        public void Absolute_unix_style_paths_are_lexed_correctly()
        {
            var command =
                @"rm ""/temp/the file.txt""";

            var parser = new Parser(new Command("rm")
            {
                new Argument<string[]>()
            });

            var result = parser.Parse(command);

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .OnlyContain(a => a == @"/temp/the file.txt");
        }

        [Fact]
        public void Absolute_Windows_style_paths_are_lexed_correctly()
        {
            var command =
                @"rm ""c:\temp\the file.txt\""";

            var parser = new Parser(new Command("rm")
            {
                new Argument<string[]>()
            });

            ParseResult result = parser.Parse(command);

            result.CommandResult
                  .Tokens
                  .Should()
                  .OnlyContain(a => a.Value == @"c:\temp\the file.txt\");
        }

        [Fact]
        public void Commands_can_have_default_argument_values()
        {
            var argument = new Argument<string>("the-arg", () => "default");

            var command = new Command("command")
            {
                argument
            };

            ParseResult result = command.Parse("command");

            result.GetValueForArgument(argument)
                  .Should()
                  .Be("default");
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_the_option_can_still_be_accessed_as_though_it_had_been_applied()
        {
            var command = new Command("command");
            var option = new Option<string>(new[] { "-o", "--option" }, () => "the-default");
            command.AddOption(option);

            ParseResult result = command.Parse("command");

            result.HasOption(option).Should().BeTrue();
            result.GetValueForOption(option).Should().Be("the-default");
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_the_option_result_is_implicit()
        {
            var option = new Option<string>(new[]{ "-o", "--option" }, () => "the-default");

            var command = new Command("command")
            {
                option
            };

            var result = command.Parse("command");

            result.FindResultFor(option)
                  .IsImplicit
                  .Should()
                  .BeTrue();
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_there_are_no_tokens()
        {
            var option = new Option<string>(
                "-o", 
                () => "the-default");

            var command = new Command("command")
            {
                option
            };

            var result = command.Parse("command");

            result.FindResultFor(option)
                  .Token
                  .Should()
                  .BeEquivalentTo(default(Token));
        }

        [Fact]
        public void When_an_argument_with_a_default_value_is_not_matched_then_there_are_no_tokens()
        {
            var argument = new Argument<string>(
                "o", 
                () => "the-default");

            var command = new Command("command")
            {
                argument
            };
            var result = command.Parse("command");

            result.FindResultFor(argument)
                  .Tokens
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void Command_default_argument_value_does_not_override_parsed_value()
        {
            var argument = new Argument<DirectoryInfo>(() => new DirectoryInfo(Directory.GetCurrentDirectory()))
            {
                Name = "the-arg"
            };

            var command = new Command("inner")
            {
                argument
            };

            var result = command.Parse("the-directory");

            result.GetValueForArgument(argument)
                  ?.Name
                  .Should()
                  .Be("the-directory");
        }

        [Fact]
        public void Unmatched_tokens_that_look_like_options_are_not_split_into_smaller_tokens()
        {
            var outer = new Command("outer")
            {
                new Command("inner")
                {
                    new Argument<string[]>
                    {
                        Arity = ArgumentArity.OneOrMore
                    }
                }
            };

            ParseResult result = outer.Parse("outer inner -p:RandomThing=random");

            result.CommandResult
                  .Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("-p:RandomThing=random");
        }

        [Fact]
        public void The_default_behavior_of_unmatched_tokens_resulting_in_errors_can_be_turned_off()
        {
            var command = new Command("the-command")
            {
                new Argument<string>()
            };
            command.TreatUnmatchedTokensAsErrors = false;

            ParseResult result = command.Parse("the-command arg1 arg2");

            result.Errors.Should().BeEmpty();

            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo("arg2");
        }

        [Fact]
        public void Option_and_Command_can_have_the_same_alias()
        {
            var innerCommand = new Command("inner")
            {
                new Argument<string[]>()
            };

            var option = new Option<bool>("--inner");

            var outerCommand = new Command("outer")
            {
                innerCommand,
                option,
                new Argument<string[]>()
            };

            var parser = new Parser(outerCommand);

            parser.Parse("outer inner")
                  .CommandResult
                  .Command
                  .Should()
                  .BeSameAs(innerCommand);

            parser.Parse("outer --inner")
                  .CommandResult
                  .Command
                  .Should()
                  .BeSameAs(outerCommand);

            parser.Parse("outer --inner inner")
                  .CommandResult
                  .Command
                  .Should()
                  .BeSameAs(innerCommand);

            parser.Parse("outer --inner inner")
                  .CommandResult
                  .Parent
                  .Children
                  .Should()
                  .Contain(c => c.Symbol == option);
        }

        [Fact]
        public void Options_can_have_the_same_alias_differentiated_only_by_prefix()
        {
            var option1 = new Option<bool>(new[] { "-a" });
            var option2 = new Option<bool>(new[] { "--a" });

            var parser = new RootCommand
            {
                option1, 
                option2
            };

            parser.Parse("-a").CommandResult
                  .Children
                  .Select(s => s.Symbol)
                  .Should()
                  .BeEquivalentTo(option1);
            parser.Parse("--a").CommandResult
                  .Children
                  .Select(s => s.Symbol)
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
            var option = new Option<string[]>("-x");

            var parseResult = option.Parse(new[] { arg1, arg2 });

            parseResult
                .FindResultFor(option)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo(new[] { arg2 });
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/1445
        public void Trailing_option_delimiters_are_ignored()
        {
            var rootCommand = new RootCommand
            {
                new Command("subcommand")
                {
                    new Option<DirectoryInfo>("--directory")
                }
            };

            var args = new[] { "subcommand", "--directory:", @"c:\" };

            var result = rootCommand.Parse(args);

            result.Errors.Should().BeEmpty();

            result.Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentSequenceTo(new[] { "subcommand", "--directory", @"c:\" });
        }

        [Theory]
        [InlineData("-x -y")]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Option_arguments_can_start_with_prefixes_that_make_them_look_like_options(string input)
        {
            var optionX = new Option<string>("-x");

            var command = new Command("command")
            {
                optionX,
                new Option<string>("-z")
            };

            var result = command.Parse(input);

            var valueForOption = result.GetValueForOption(optionX);

            valueForOption.Should().Be("-y");
        }
        
        [Fact]
        public void Option_arguments_can_start_with_prefixes_that_make_them_look_like_bundled_options()
        {
            var optionA = new Option<string>("-a");
            var optionB = new Option<bool>("-b");
            var optionC = new Option<bool>("-c");

            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse("-a -bc");

            result.GetValueForOption(optionA).Should().Be("-bc");
            result.GetValueForOption(optionB).Should().BeFalse();
            result.GetValueForOption(optionC).Should().BeFalse();
        }

        [Fact]
        public void Option_arguments_can_match_subcommands()
        {
            var optionA = new Option<string>("-a");
            var root = new RootCommand
            {
                new Command("subcommand"),
                optionA
            };

            var result = root.Parse("-a subcommand");

            _output.WriteLine(result.ToString());

            result.GetValueForOption(optionA).Should().Be("subcommand");
            result.CommandResult.Command.Should().BeSameAs(root);
        }

        [Fact]
        public void Arguments_can_match_subcommands()
        {
            var argument = new Argument<string[]>();
            var subcommand = new Command("subcommand")
            {
                argument
            };
            var root = new RootCommand
            {
                subcommand
            };

            var result = root.Parse("subcommand one two three subcommand four");

            result.CommandResult.Command.Should().BeSameAs(subcommand);

            result.GetValueForArgument(argument)
                  .Should()
                  .BeEquivalentSequenceTo("one", "two", "three", "subcommand", "four");
        }

        [Theory]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Option_arguments_can_match_the_aliases_of_sibling_options_when_non_space_argument_delimiter_is_used(string input)
        {
            var optionX = new Option<string>("-x");

            var command = new Command("command")
            {
                optionX,
                new Option<string>("-y")
            };

            var result = command.Parse(input);

            var valueForOption = result.GetValueForOption(optionX);

            result.Errors.Should().BeEmpty();
            valueForOption.Should().Be("-y");
        }

        [Fact]
        public void Single_option_arguments_that_match_option_aliases_are_parsed_correctly()
        {
            var optionX = new Option<string>("-x");

            var command = new RootCommand
            {
                optionX
            };

            var result = command.Parse("-x -x");

            result.GetValueForOption(optionX).Should().Be("-x");
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
            var optX = new Option<bool>("-x");
            var optY = new Option<bool>("-y");

            var root = new RootCommand("parent")
            {
                optX,
                optY,
            };

            var result = root.Parse(commandLine);

            result.Errors.Should().BeEmpty();

            result.GetValueForOption(optX).Should().BeTrue();
            result.GetValueForOption(optY).Should().BeTrue();
        }

        [Fact]
        public void Multiple_option_arguments_that_match_multiple_arity_option_aliases_are_parsed_correctly()
        {
            var optionX = new Option<string[]>("-x");
            var optionY = new Option<string[]>("-y");

            var command = new RootCommand
            {
                optionX,
                optionY
            };

            var result = command.Parse("-x -x -x -y -y -x -y -y -y -x -x -y");

            _output.WriteLine(result.Diagram());

            result.GetValueForOption(optionX).Should().BeEquivalentTo(new[] { "-x", "-y", "-y" });
            result.GetValueForOption(optionY).Should().BeEquivalentTo(new[] { "-x", "-y", "-x" });
        }

        [Fact]
        public void Bundled_option_arguments_that_match_option_aliases_are_parsed_correctly()
        {
            var optionX = new Option<string>("-x");
            var optionY = new Option<bool>("-y");

            var command = new RootCommand
            {
                optionX,
                optionY
            };

            var result = command.Parse("-yxx");

            result.GetValueForOption(optionX).Should().Be("x");
        }

        [Fact]
        public void Argument_name_is_not_matched_as_a_token()
        {
            var nameArg = new Argument<string>("name");
            var columnsArg = new Argument<IEnumerable<string>>("columns");

            var command = new Command("add", "Adds a new series")
            {
                nameArg,
                columnsArg
            };

            var result = command.Parse("name one two three");

            result.GetValueForArgument(nameArg).Should().Be("name");
            result.GetValueForArgument(columnsArg).Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Option_aliases_do_not_need_to_be_prefixed()
        {
            var option = new Option<bool>("noprefix");

            var result = new RootCommand { option }.Parse("noprefix");

            result.HasOption(option).Should().BeTrue();
        }

        [Fact]
        public void Boolean_options_with_no_argument_specified_do_not_match_subsequent_arguments()
        {
            var option = new Option<bool>("-v");

            var command = new Command("command")
            {
                option
            };

            var result = command.Parse("-v an-argument");

            result.GetValueForOption(option).Should().BeTrue();
        }

        [Fact]
        public void When_a_command_line_has_unmatched_tokens_they_are_not_applied_to_subsequent_options()
        {
            var command = new Command("command")
            {
                TreatUnmatchedTokensAsErrors = false
            };
            var optionX = new Option<string>("-x");
            command.AddOption(optionX);
            var optionY = new Option<string>("-y");
            command.AddOption(optionY);

            var result = command.Parse("-x 23 unmatched-token -y 42");

            result.GetValueForOption(optionX).Should().Be("23");
            result.GetValueForOption(optionY).Should().Be("42");
            result.UnmatchedTokens.Should().BeEquivalentTo("unmatched-token");
        }

        [Fact]
        public void Parse_can_be_called_with_null_args()
        {
            var parser = new Parser(new RootCommand());

            var result = parser.Parse(null);

            result.CommandResult.Command.Name.Should().Be(RootCommand.ExecutableName);
        }

        [Fact]
        public void Command_argument_arity_can_be_a_fixed_value_greater_than_1()
        {
            var argument = new Argument<string[]>
            {
                Arity = new ArgumentArity(3, 3)
            };
            var command = new Command("the-command")
            {
                argument
            };

            command.Parse("1 2 3")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument, argument),
                       new Token("2", TokenType.Argument, argument),
                       new Token("3", TokenType.Argument, argument));
        }

        [Fact]
        public void Command_argument_arity_can_be_a_range_with_a_lower_bound_greater_than_1()
        {
            var argument = new Argument<string[]>
            {
                Arity = new ArgumentArity(3, 5)
            };
            var command = new Command("the-command")
            {
                argument
            };

            command.Parse("1 2 3")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument, argument),
                       new Token("2", TokenType.Argument, argument),
                       new Token("3", TokenType.Argument, argument));
            command.Parse("1 2 3 4 5")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument, argument),
                       new Token("2", TokenType.Argument, argument),
                       new Token("3", TokenType.Argument, argument),
                       new Token("4", TokenType.Argument, argument),
                       new Token("5", TokenType.Argument, argument));
        }

        [Fact]
        public void When_command_arguments_are_fewer_than_minimum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                new Argument<string[]>
                {
                    Arity = new ArgumentArity(2, 3)
                }
            };

            var result = command.Parse("1");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain(LocalizationResources.Instance.RequiredArgumentMissing(result.CommandResult));
        }

        [Fact]
        public void When_command_arguments_are_greater_than_maximum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                new Argument<string[]>
                {
                    Arity = new ArgumentArity(2, 3)
                }
            };

            ParseResult parseResult = command.Parse("1 2 3 4");

            parseResult
                   .Errors
                   .Select(e => e.Message)
                   .Should()
                   .Contain(LocalizationResources.Instance.UnrecognizedCommandOrArgument("4"));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_fixed_value_greater_than_1()
        {
            var option = new Option<int[]>("-x") { Arity = new ArgumentArity(3, 3)};

            var command = new Command("the-command")
            {
                option
            };

            command.Parse("-x 1 -x 2 -x 3")
                   .FindResultFor(option)
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument, default),
                       new Token("2", TokenType.Argument, default),
                       new Token("3", TokenType.Argument, default));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_range_with_a_lower_bound_greater_than_1()
        {
            var option = new Option<string[]>("-x") { Arity = new ArgumentArity(3, 5) };

            var command = new Command("the-command")
            {
                option
            };

            command.Parse("-x 1 -x 2 -x 3")
                   .FindResultFor(option)
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument, default),
                       new Token("2", TokenType.Argument, default),
                       new Token("3", TokenType.Argument, default));
            command.Parse("-x 1 -x 2 -x 3 -x 4 -x 5")
                   .FindResultFor(option)
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument, default),
                       new Token("2", TokenType.Argument, default),
                       new Token("3", TokenType.Argument, default),
                       new Token("4", TokenType.Argument, default),
                       new Token("5", TokenType.Argument, default));
        }

        [Fact]
        public void When_option_arguments_are_fewer_than_minimum_arity_then_an_error_is_returned()
        {
            var option = new Option<int[]>("-x") { Arity = new ArgumentArity(2, 3) };

            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("-x 1");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain(LocalizationResources.Instance.RequiredArgumentMissing(result.CommandResult.FindResultFor(option)));
        }

        [Fact]
        public void When_option_arguments_are_greater_than_maximum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                new Option<int[]>("-x") { Arity = new ArgumentArity(2, 3)}
            };

            command.Parse("-x 1 2 3 4")
                   .Errors
                   .Select(e => e.Message)
                   .Should()
                   .Contain(LocalizationResources.Instance.UnrecognizedCommandOrArgument("4"));
        }
        
        [Fact]
        public void Tokens_are_not_split_if_the_part_before_the_delimiter_is_not_an_option()
        {
            var rootCommand = new RootCommand
            {
                Name = "jdbc"
            };
            rootCommand.Add(new Option<string>("url"));
            var result = rootCommand.Parse("jdbc url \"jdbc:sqlserver://10.0.0.2;databaseName=main\"");

            result.Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("url",
                                  "jdbc:sqlserver://10.0.0.2;databaseName=main");
        }

        [Fact]
        public void A_subcommand_wont_overflow_when_checking_maximum_argument_capacity()
        {
            // Tests bug identified in https://github.com/dotnet/command-line-api/issues/997

            var argument1 = new Argument<string>("arg1");

            var argument2 = new Argument<string[]>("arg2");

            var command = new Command("subcommand")
            {
                argument1,
                argument2
            };

            var rootCommand = new RootCommand
            {
                command
            };

            var parseResult = rootCommand.Parse("subcommand arg1 arg2");

            Action act = () => parseResult.GetCompletions();
            act.Should().NotThrow();
        }

        [Theory] // https://github.com/dotnet/command-line-api/issues/1551, https://github.com/dotnet/command-line-api/issues/1533
        [InlineData("--exec-prefix", "")]
        [InlineData("--exec-prefix:", "")]
        [InlineData("--exec-prefix=", "")]
        public void Parsed_value_of_empty_string_arg_is_an_empty_string(string arg1, string arg2)
        {
            var option = new Option<string>("--exec-prefix", getDefaultValue: () => "/usr/local");
            var rootCommand = new RootCommand
            {
                option
            };

            var result = rootCommand.Parse(new[] { arg1, arg2 });

            result.GetValueForOption(option).Should().BeEmpty();
        }
    }
}
