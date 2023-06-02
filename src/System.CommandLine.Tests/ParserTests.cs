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
        private readonly ITestOutputHelper _output;

        public ParserTests(ITestOutputHelper output)
        {
            _output = output;
        }

        protected virtual T GetValue<T>(ParseResult parseResult, CliOption<T> option)
            => parseResult.GetValue(option);

        protected virtual T GetValue<T>(ParseResult parseResult, CliArgument<T> argument)
            => parseResult.GetValue(argument);

        [Fact]
        public void An_option_can_be_checked_by_object_instance()
        {
            var option = new CliOption<bool>("--flag");
            var option2 = new CliOption<bool>("--flag2");
            var result = new CliRootCommand { option, option2 }
                .Parse("--flag");

            result.GetResult(option).Should().NotBeNull();
            result.GetResult(option2).Should().BeNull();
        }

        [Fact]
        public void Two_options_are_parsed_correctly()
        {
            var optionOne = new CliOption<bool>("-o", "--one");

            var optionTwo = new CliOption<bool>("-t", "--two");

            var result = new CliRootCommand { optionOne, optionTwo }.Parse("-o -t");

            result.GetResult(optionOne).Should().NotBeNull();
            result.GetResult(optionTwo).Should().NotBeNull();
        }

        [Theory]
        [InlineData("-")]
        [InlineData("/")]
        public void When_a_token_is_just_a_prefix_then_an_error_is_returned(string prefix)
        {
            var result = new CliRootCommand().Parse(prefix);

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain(LocalizationResources.UnrecognizedCommandOrArgument(prefix));
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_equals_delimiter()
        {
            var option = new CliOption<string>("-x");

            var result = new CliRootCommand { option }.Parse("-x=some-value");

            result.Errors.Should().BeEmpty();

            result.GetResult(option).Tokens.Should().ContainSingle(a => a.Value == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_equals_delimiter()
        {
            var option = new CliOption<string>("--hello");

            var result = new CliRootCommand { option }.Parse("--hello=there");

            result.Errors.Should().BeEmpty();

            result.GetResult(option).Tokens.Should().ContainSingle(a => a.Value == "there");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_colon_delimiter()
        {
            var option = new CliOption<string>("-x");

            var result = new CliRootCommand { option }.Parse("-x:some-value");

            result.Errors.Should().BeEmpty();

            result.GetResult(option).Tokens.Should().ContainSingle(a => a.Value == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_colon_delimiter()
        {
            var option = new CliOption<string>("--hello");

            var result = new CliRootCommand { option }.Parse("--hello:there");

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

            var result = command.Parse("the-command -xyz");

            result.CommandResult
                  .Children
                  .Select(o => ((OptionResult)o).Option.Name)
                  .Should()
                  .BeEquivalentTo("-x", "-y", "-z");
        }

        [Fact]
        public void Options_short_forms_do_not_get_unbundled_if_unbundling_is_turned_off()
        {
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

            var result = command.Parse("the-command --xyz");

            result.CommandResult
                  .Children
                  .Select(o => ((OptionResult)o).Option.Name)
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

            ParseResult result = outer.Parse("outer inner -abc");

            result.CommandResult
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

            var result = command.Parse("-a -bc");

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

            var result = command.Parse("-abcvalue");
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

            var result = command.Parse("-abc=value");
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

            var result = command.Parse("-abc:value");
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

            var result = command.Parse("-abvcalue");
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
            var parser = new CliRootCommand
            {
                animalsOption,
                vegetablesOption
            };

            var result = parser.Parse("-a cat -v carrot -a dog");

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

        [Fact]
        public void Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
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

            var result = command.Parse("the-command -a cat some-arg -v carrot");

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

            result.CommandResult
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

            var result = command.Parse("outer --inner1 argument1 --inner2 argument2");

            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o =>
                                     ((OptionResult)o).Option.Name == "--inner1" &&
                                     o.Tokens.Single().Value == "argument1");
            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o =>
                                     ((OptionResult)o).Option.Name == "--inner2" &&
                                     o.Tokens.Single().Value == "argument2");
        }

        [Fact]
        public void Relative_order_of_arguments_and_options_within_a_command_does_not_matter()
        {
            var command = new CliCommand("move")
            {
                new CliArgument<string[]>("arg"),
                new CliOption<string>("-X")
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
                             .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal))
                             .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.PrivateProtected)));
            result1.Should()
                   .BeEquivalentTo(
                       result3,
                       x => x.IgnoringCyclicReferences()
                             .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal))
                             .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.PrivateProtected)));
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
            var rawSplit = CliParser.SplitCommandLine(commandLine);

            var command = new CliCommand("the-command")
                          {
                              new CliArgument<string[]>("arg"),
                              new CliOption<string>("--one"),
                              new CliOption<string[]>("--many")
                          };

            var result = command.Parse(commandLine);

            result.Tokens.Select(t => t.Value).Should().Equal(rawSplit);
        }

        [Fact]
        public void An_outer_command_with_the_same_name_does_not_capture()
        {
            var command = new CliCommand("one")
                          {
                              new CliCommand("two")
                              {
                                  new CliCommand("three")
                              },
                              new CliCommand("three")
                          };

            ParseResult result = command.Parse("one two three");

            result.Diagram().Should().Be("[ one [ two [ three ] ] ]");
        }

        [Fact]
        public void An_inner_command_with_the_same_name_does_not_capture()
        {
            var command = new CliCommand("one")
                          {
                              new CliCommand("two")
                              {
                                  new CliCommand("three")
                              },
                              new CliCommand("three")
                          };

            ParseResult result = command.Parse("one three");

            result.Diagram().Should().Be("[ one [ three ] ]");
        }

        [Fact]
        public void When_nested_commands_all_accept_arguments_then_the_nearest_captures_the_arguments()
        {
            var command = new CliCommand(
                "outer")
            {
                new CliArgument<string[]>("arg1"),
                new CliCommand("inner")
                {
                    new CliArgument<string[]>("arg2")
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

        [Fact]
        public void When_child_option_will_not_accept_arg_then_parent_can()
        {
            var option = new CliOption<bool>("-x");
            var command = new CliCommand("the-command")
                         {
                             option,
                             new CliArgument<string>("arg")
                         };

            var result = command.Parse("the-command -x the-argument");

            var optionResult = result.GetResult(option);
            optionResult.Tokens.Should().BeEmpty();
            result.CommandResult.Tokens.Select(t => t.Value).Should().BeEquivalentTo("the-argument");
        }

        [Fact]
        public void When_parent_option_will_not_accept_arg_then_child_can()
        {
            var option = new CliOption<string>("-x");
            var command = new CliCommand("the-command")
            {
                option
            };

            var result = command.Parse("the-command -x the-argument");

            result.GetResult(option).Tokens.Select(t => t.Value).Should().BeEquivalentTo("the-argument");
            result.CommandResult.Tokens.Should().BeEmpty();
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

            var result = parent.Parse("child");

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

            var result = grandparent.Parse("parent grandchild");

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

            ParseResult result = outer.Parse("outer inner -x");

            result.CommandResult
                  .Parent
                  .Should()
                  .BeOfType<CommandResult>()
                  .Which
                  .Children
                  .Should()
                  .AllBeAssignableTo<CommandResult>();
            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o => ((OptionResult)o).Option.Name == "-x");
        }

        [Fact]
        public void When_options_with_the_same_name_are_defined_on_parent_and_child_commands_and_specified_in_between_then_it_attaches_to_the_outer_command()
        {
            var outer = new CliCommand("outer");
            outer.Options.Add(new CliOption<bool>("-x"));
            var inner = new CliCommand("inner");
            inner.Options.Add(new CliOption<bool>("-x"));
            outer.Subcommands.Add(inner);

            var result = outer.Parse("outer -x inner");

            result.CommandResult
                  .Children
                  .Should()
                  .BeEmpty();
            result.CommandResult
                  .Parent
                  .Should()
                  .BeOfType<CommandResult>()
                  .Which
                  .Children
                  .Should()
                  .ContainSingle(o => o is OptionResult && ((OptionResult)o).Option.Name == "-x");
        }

        [Fact]
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

            var result = outer.Parse("outer inner -x one -x two");

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

            ParseResult result = command.Parse(new[] { "the-command",
                                               "complete",
                                               "--position",
                                               "7",
                                               "the-command" });

            CommandResult completeResult = result.CommandResult;

            completeResult.Tokens.Select(t => t.Value).Should().BeEquivalentTo("the-command");
        }

        [Fact]
        public void Absolute_unix_style_paths_are_lexed_correctly()
        {
            const string commandText =
                @"rm ""/temp/the file.txt""";

            CliCommand command = new ("rm")
            {
                new CliArgument<string[]>("arg")
            };

            var result = command.Parse(commandText);

            result.CommandResult
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

            ParseResult result = command.Parse(commandText);

            result.CommandResult
                  .Tokens
                  .Should()
                  .OnlyContain(a => a.Value == @"c:\temp\the file.txt\");
        }

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

            ParseResult result = command.Parse("command");

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

            ParseResult result = command.Parse("command");

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

            var result = command.Parse("command");

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

            var result = command.Parse("command");

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
            var result = command.Parse("command");

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

            var result = command.Parse("the-directory");

            GetValue(result, argument)
                  .Name
                  .Should()
                  .Be("the-directory");
        }

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

            outerCommand.Parse("outer inner")
                  .CommandResult
                  .Command
                  .Should()
                  .BeSameAs(innerCommand);

            outerCommand.Parse("outer --inner")
                  .CommandResult
                  .Command
                  .Should()
                  .BeSameAs(outerCommand);

            outerCommand.Parse("outer --inner inner")
                  .CommandResult
                  .Command
                  .Should()
                  .BeSameAs(innerCommand);

            outerCommand.Parse("outer --inner inner")
                  .CommandResult
                  .Parent
                  .Should()
                  .BeOfType<CommandResult>()
                  .Which
                  .Children
                  .Should()
                  .Contain(o => ((OptionResult)o).Option == option);
        }

        [Fact]
        public void Options_can_have_the_same_alias_differentiated_only_by_prefix()
        {
            var option1 = new CliOption<bool>("-a");
            var option2 = new CliOption<bool>("--a");

            var parser = new CliRootCommand
            {
                option1, 
                option2
            };

            parser.Parse("-a").CommandResult
                  .Children
                  .Select(s => ((OptionResult)s).Option)
                  .Should()
                  .BeEquivalentTo(option1);
            parser.Parse("--a").CommandResult
                  .Children
                  .Select(s => ((OptionResult)s).Option)
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

            var parseResult = new CliRootCommand { option }.Parse(new[] { arg1, arg2 });

            parseResult
                .GetResult(option)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo(new[] { arg2 });
        }

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
            var optionX = new CliOption<string>("-x");

            var command = new CliCommand("command")
            {
                optionX,
                new CliOption<string>("-z")
            };

            var result = command.Parse(input);

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

            var result = command.Parse("-a -bc");

            GetValue(result, optionA).Should().Be("-bc");
            GetValue(result, optionB).Should().BeFalse();
            GetValue(result, optionC).Should().BeFalse();
        }

        [Fact]
        public void Option_arguments_can_match_subcommands()
        {
            var optionA = new CliOption<string>("-a");
            var root = new CliRootCommand
            {
                new CliCommand("subcommand"),
                optionA
            };

            var result = root.Parse("-a subcommand");

            _output.WriteLine(result.ToString());

            GetValue(result, optionA).Should().Be("subcommand");
            result.CommandResult.Command.Should().BeSameAs(root);
        }

        [Fact]
        public void Arguments_can_match_subcommands()
        {
            var argument = new CliArgument<string[]>("arg");
            var subcommand = new CliCommand("subcommand")
            {
                argument
            };
            var root = new CliRootCommand
            {
                subcommand
            };

            var result = root.Parse("subcommand one two three subcommand four");

            result.CommandResult.Command.Should().BeSameAs(subcommand);

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

            var result = command.Parse(input);

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

            var result = command.Parse("-x -x");

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

            var root = new CliRootCommand("parent")
            {
                optX,
                optY,
            };

            var result = root.Parse(commandLine);

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

            var result = command.Parse("-x -x -x -y -y -x -y -y -y -x -x -y");

            _output.WriteLine(result.Diagram());

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

            var result = command.Parse("-yxx");

            GetValue(result, optionX).Should().Be("x");
        }

        [Fact]
        public void Argument_name_is_not_matched_as_a_token()
        {
            var nameArg = new CliArgument<string>("name");
            var columnsArg = new CliArgument<IEnumerable<string>>("columns");

            var command = new CliCommand("add", "Adds a new series")
            {
                nameArg,
                columnsArg
            };

            var result = command.Parse("name one two three");

            GetValue(result, nameArg).Should().Be("name");
            GetValue(result, columnsArg).Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Option_aliases_do_not_need_to_be_prefixed()
        {
            var option = new CliOption<bool>("noprefix");

            var result = new CliRootCommand { option }.Parse("noprefix");

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

            var result = command.Parse("-v an-argument");

            GetValue(result, option).Should().BeTrue();
        }

        [Fact]
        public void When_a_command_line_has_unmatched_tokens_they_are_not_applied_to_subsequent_options()
        {
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
                result.Action.Should().NotBeSameAs(result.CommandResult.Command.Action);
            }
            else
            {
                result.Errors.Should().BeEmpty();
                result.Action.Should().BeSameAs(result.CommandResult.Command.Action);
            }
        }

        [Fact]
        public void RootCommand_TreatUnmatchedTokensAsErrors_set_to_false_has_precedence_over_subcommands()
        {
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
            result.Action.Should().BeSameAs(result.CommandResult.Command.Action);
        }

        [Fact]
        public void Parse_can_not_be_called_with_null_args()
        {
            Action passNull = () => new CliRootCommand().Parse(args: null);

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

            command.Parse("1 2 3")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new CliToken("1", CliTokenType.Argument, argument),
                       new CliToken("2", CliTokenType.Argument, argument),
                       new CliToken("3", CliTokenType.Argument, argument));
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

            command.Parse("1 2 3")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new CliToken("1", CliTokenType.Argument, argument),
                       new CliToken("2", CliTokenType.Argument, argument),
                       new CliToken("3", CliTokenType.Argument, argument));
            command.Parse("1 2 3 4 5")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new CliToken("1", CliTokenType.Argument, argument),
                       new CliToken("2", CliTokenType.Argument, argument),
                       new CliToken("3", CliTokenType.Argument, argument),
                       new CliToken("4", CliTokenType.Argument, argument),
                       new CliToken("5", CliTokenType.Argument, argument));
        }

        [Fact]
        public void When_command_arguments_are_fewer_than_minimum_arity_then_an_error_is_returned()
        {
            var command = new CliCommand("the-command")
            {
                new CliArgument<string[]>("arg")
                {
                    Arity = new ArgumentArity(2, 3)
                }
            };

            var result = command.Parse("1");

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

            ParseResult parseResult = command.Parse("1 2 3 4");

            parseResult
                   .Errors
                   .Select(e => e.Message)
                   .Should()
                   .Contain(LocalizationResources.UnrecognizedCommandOrArgument("4"));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_fixed_value_greater_than_1()
        {
            var option = new CliOption<int[]>("-x") { Arity = new ArgumentArity(3, 3)};

            var command = new CliCommand("the-command")
            {
                option
            };

            command.Parse("-x 1 -x 2 -x 3")
                   .GetResult(option)
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new CliToken("1", CliTokenType.Argument, default),
                       new CliToken("2", CliTokenType.Argument, default),
                       new CliToken("3", CliTokenType.Argument, default));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_range_with_a_lower_bound_greater_than_1()
        {
            var option = new CliOption<string[]>("-x") { Arity = new ArgumentArity(3, 5) };

            var command = new CliCommand("the-command")
            {
                option
            };

            command.Parse("-x 1 -x 2 -x 3")
                   .GetResult(option)
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new CliToken("1", CliTokenType.Argument, default),
                       new CliToken("2", CliTokenType.Argument, default),
                       new CliToken("3", CliTokenType.Argument, default));
            command.Parse("-x 1 -x 2 -x 3 -x 4 -x 5")
                   .GetResult(option)
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new CliToken("1", CliTokenType.Argument, default),
                       new CliToken("2", CliTokenType.Argument, default),
                       new CliToken("3", CliTokenType.Argument, default),
                       new CliToken("4", CliTokenType.Argument, default),
                       new CliToken("5", CliTokenType.Argument, default));
        }

        [Fact]
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

            var result = command.Parse("-x 1");

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

            command.Parse("-x 1 2 3 4")
                   .Errors
                   .Select(e => e.Message)
                   .Should()
                   .Contain(LocalizationResources.UnrecognizedCommandOrArgument("4"));
        }
        
        [Fact]
        public void Tokens_are_not_split_if_the_part_before_the_delimiter_is_not_an_option()
        {
            var rootCommand = new CliCommand("jdbc");
            rootCommand.Add(new CliOption<string>("url"));
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

        [Theory] // https://github.com/dotnet/command-line-api/issues/1551, https://github.com/dotnet/command-line-api/issues/1533
        [InlineData("--exec-prefix", "")]
        [InlineData("--exec-prefix:", "")]
        [InlineData("--exec-prefix=", "")]
        public void Parsed_value_of_empty_string_arg_is_an_empty_string(string arg1, string arg2)
        {
            var option = new CliOption<string>("--exec-prefix")
            {
                DefaultValueFactory = (_) => "/usr/local"
            };

            var rootCommand = new CliRootCommand
            {
                option
            };

            var result = rootCommand.Parse(new[] { arg1, arg2 });

            GetValue(result, option).Should().BeEmpty();
        }
    }
}
