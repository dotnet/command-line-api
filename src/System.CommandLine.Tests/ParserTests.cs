// Copyright (c) .NET Foundation and contributors. All rights reserved.
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
using System.ComponentModel;
using System.Globalization;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        [Fact]
        public void An_option_can_be_checked_by_object_instance()
        {
            var option = new Option("--flag");
            var option2 = new Option("--flag2");
            var result = new Parser(option, option2)
                .Parse("--flag");

            result.HasOption(option).Should().BeTrue();
            result.HasOption(option2).Should().BeFalse();
        }

        [Fact]
        public void Two_options_are_parsed_correctly()
        {
            var optionOne = new Option(new[] { "-o", "--one" });

            var optionTwo = new Option(new[] { "-t", "--two" });

            var result = new Parser(
                    optionOne,
                    optionTwo)
                .Parse("-o -t");

            result.HasOption(optionOne).Should().BeTrue();
            result.HasOption(optionTwo).Should().BeTrue();
        }

        [Fact]
        public void When_no_options_are_specified_then_an_error_is_returned()
        {
            Action create = () => new Parser(Array.Empty<Symbol>());

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("You must specify at least one option or command.");
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
                  .Contain(Resources.Instance.UnrecognizedCommandOrArgument(prefix));
        }

        [Fact]
        public void Two_options_cannot_have_conflicting_aliases()
        {
            Action create = () =>
                new Parser(new Option(
                               new[] { "-o", "--one" }),
                           new Option(
                               new[] { "-t", "--one" }));

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("Alias '--one' is already in use.");
        }

        [Fact]
        public void A_double_dash_delimiter_specifies_that_no_further_command_line_args_will_be_treated_as_options()
        {
            var option = new Option(new[] { "-o", "--one" });
            var result = new Parser(option)
                .Parse("-o \"some stuff\" -- -x -y -z -o:foo");

            result.HasOption(option)
                  .Should()
                  .BeTrue();

            result.UnparsedTokens
                  .Should()
                  .BeEquivalentSequenceTo("-x",
                                          "-y",
                                          "-z",
                                          "-o:foo");
        }

        [Fact]
        public void The_portion_of_the_command_line_following_a_double_dash_is_accessible_as_UnparsedTokens()
        {
            var result = new Parser(new Option("-o"))
                .Parse("-o \"some stuff\" -- x y z");

            result.UnparsedTokens
                  .Should()
                  .BeEquivalentSequenceTo("x", "y", "z");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_equals_delimiter()
        {
            var option = new Option<string>("-x") { Arity = ArgumentArity.ExactlyOne };

            var result = option.Parse("-x=some-value");

            result.Errors.Should().BeEmpty();

            result.FindResultFor(option).Tokens.Should().ContainSingle(a => a.Value == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_equals_delimiter()
        {
            var option = 
                new Option("--hello") { Arity = ArgumentArity.ExactlyOne };

            var result = option.Parse("--hello=there");

            result.Errors.Should().BeEmpty();

            result.FindResultFor(option).Tokens.Should().ContainSingle(a => a.Value == "there");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_colon_delimiter()
        {
            var option = new Option("-x") { Arity = ArgumentArity.ExactlyOne };

            var result = option.Parse("-x:some-value");

            result.Errors.Should().BeEmpty();

            result.FindResultFor(option).Tokens.Should().ContainSingle(a => a.Value == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_colon_delimiter()
        {
            var option = new Option("--hello") { Arity = ArgumentArity.ExactlyOne };

            var result = option.Parse("--hello:there");

            result.Errors.Should().BeEmpty();

            result.FindResultFor(option).Tokens.Should().ContainSingle(a => a.Value == "there");
        }

        [Fact]
        public void Option_short_forms_can_be_bundled()
        {
            var command = new Command("the-command");
            command.AddOption(new Option("-x"));
            command.AddOption(new Option("-y"));
            command.AddOption(new Option("-z"));

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
            var parser = new CommandLineBuilder()
                         .EnablePosixBundling(false)
                         .AddCommand(new Command("the-command")
                                     {
                                         new Option("-x"),
                                         new Option("-y"),
                                         new Option("-z")
                                     })
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
                    new Option("--xyz"),
                    new Option("-x"),
                    new Option("-y"),
                    new Option("-z")
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
            outer.AddOption(new Option("-a"));
            var inner = new Command("inner")
            {
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };
            inner.AddOption(new Option("-b"));
            inner.AddOption(new Option("-c"));
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
            var optionB = new Option("-b");
            var optionC = new Option("-c");


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
        public void Optional_option_arguments_are_unbundled()
        {
            var optionA = new Option<string>("-a") { Arity = ArgumentArity.ZeroOrOne };
            var optionB = new Option("-b");
            var optionC = new Option("-c");

            var command = new RootCommand
            {
                optionA,
                optionB,
                optionC
            };

            var result = command.Parse("-a -bc");

            result.Tokens
                  .Select(t => t.Value)
                  .Should()
                  .BeEquivalentTo("-a", "-b", "-c");
        }

        [Fact]
        public void Last_bundled_option_can_accept_argument_with_no_separator()
        {
            var optionA = new Option("-a");
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
            var optionA = new Option("-a");
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
            var optionA = new Option("-a");
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
            var optionA = new Option("-a");
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
            var animalsOption = new Option(new[] { "-a", "--animals" }) { Arity = ArgumentArity.ZeroOrMore };
            var vegetablesOption = new Option(new[] { "-v", "--vegetables" }) { Arity = ArgumentArity.ZeroOrMore };
            var parser = new Parser(
                animalsOption,
                vegetablesOption);

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
            var animalsOption = new Option(new[] { "-a", "--animals" }) { Arity = ArgumentArity.ZeroOrMore}
                .FromAmong("dog", "cat", "sheep");
            var vegetablesOption = new Option(new[] { "-v", "--vegetables" }) { Arity = ArgumentArity.ZeroOrMore };
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
            var animalsOption = new Option(new[] { "-a", "--animals" }) { Arity = ArgumentArity.ZeroOrOne };

            var vegetablesOption = new Option(new[] { "-v", "--vegetables" }) { Arity = ArgumentArity.ZeroOrOne };
            
            var parser = new Parser(
                new Command("the-command")
                {
                    animalsOption,
                    vegetablesOption,
                    new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }});

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
                new Option("--inner1") { Arity = ArgumentArity.ExactlyOne },
                new Option("--inner2") { Arity = ArgumentArity.ExactlyOne }
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
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                },
                new Command("inner")
                {
                    new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }
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
            var option = new Option("-x");
            var command = new Command("the-command")
                         {
                             option,
                             new Argument<string>()
                         };

            var result = command.Parse("the-command -x the-argument");

            result.FindResultFor(option).Tokens.Should().BeEmpty();
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
                                new Option("-x")
                            },
                            new Option("-x")
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
            outer.AddOption(new Option("-x"));
            var inner = new Command("inner");
            inner.AddOption(new Option("-x"));
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
        public void A_root_command_can_be_omitted_from_the_parsed_args()
        {
            var command = new Command("outer")
            {
                new Command("inner")
                {
                    new Option("-x") { Arity = ArgumentArity.ExactlyOne }
                }
            };

            var result1 = command.Parse("inner -x hello");
            var result2 = command.Parse("outer inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void A_root_command_can_match_a_full_path_to_an_executable()
        {
            var command = new RootCommand
            {
                new Command("inner")
                {
                    new Option("-x") { Arity = ArgumentArity.ExactlyOne }
                }
            };

            ParseResult result1 = command.Parse("inner -x hello");

            ParseResult result2 = command.Parse($"{RootCommand.ExecutablePath} inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void A_renamed_RootCommand_can_be_omitted_from_the_parsed_args()
        {
            var rootCommand = new RootCommand
                              {
                                  new Command("inner")
                                  {
                                      new Option("-x") { Arity = ArgumentArity.ExactlyOne }
                                  }
                              };
            rootCommand.Name = "outer";

            var result1 = rootCommand.Parse("inner -x hello");
            var result2 = rootCommand.Parse("outer inner -x hello");
            var result3 = rootCommand.Parse($"{RootCommand.ExecutableName} inner -x hello");

            result2.RootCommandResult.Command.Should().Be(result1.RootCommandResult.Command);
            result3.RootCommandResult.Command.Should().Be(result1.RootCommandResult.Command);
        }

        [Fact]
        public void Absolute_unix_style_paths_are_lexed_correctly()
        {
            var command =
                @"rm ""/temp/the file.txt""";

            var parser = new Parser(new Command("rm")
            {
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
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
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
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

            result.ValueForArgument(argument)
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
            result.ValueForOption(option).Should().Be("the-default");
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
                  .BeNull();
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

            result.ValueForArgument(argument)
                  ?.Name
                  .Should()
                  .Be("the-directory");
        }

        [Fact]
        public void Unmatched_options_are_not_split_into_smaller_tokens()
        {
            var outer = new Command("outer")
            {
                new Option("-p"),
                new Command("inner")
                {
                    new Argument
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
                new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
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
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };

            var option = new Option("--inner");

            var outerCommand = new Command("outer")
            {
                innerCommand,
                option,
                new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };

            var parser = new Parser(outerCommand);

            parser.Parse("outer inner")
                  .CommandResult
                  .Command
                  .Should()
                  .Be(innerCommand);

            parser.Parse("outer --inner")
                  .CommandResult
                  .Command
                  .Should()
                  .Be(outerCommand);

            parser.Parse("outer --inner inner")
                  .CommandResult
                  .Command
                  .Should()
                  .Be(innerCommand);

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
            var option1 = new Option(new[] { "-a" });
            var option2 = new Option(new[] { "--a" });

            var parser = new Parser(option1, option2);

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
        [InlineData("-x \"hello\"", "hello")]
        [InlineData("-x=\"hello\"", "hello")]
        [InlineData("-x:\"hello\"", "hello")]
        [InlineData("-x \"\"", "")]
        [InlineData("-x=\"\"", "")]
        [InlineData("-x:\"\"", "")]
        public void When_an_argument_is_enclosed_in_double_quotes_its_value_has_the_quotes_removed(string input, string expected)
        {
            var option = new Option("-x") { Arity = ArgumentArity.ZeroOrMore };

            var parseResult = option.Parse(input);

            parseResult
                .FindResultFor(option)
                .Tokens
                .Select(t => t.Value)
                .Should()
                .BeEquivalentTo(new[] { expected });
        }

        [Theory]
        [InlineData("-x -y")]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Arguments_can_start_with_prefixes_that_make_them_look_like_options(string input)
        {
            var optionX = new Option("-x") { Arity = ArgumentArity.ZeroOrOne};

            var command = new Command("command")
            {
                optionX,
                new Option("-z") { Arity = ArgumentArity.ZeroOrOne}
            };

            var result = command.Parse(input);

            var valueForOption = result.ValueForOption(optionX);

            valueForOption.Should().Be("-y");
        }

        [Theory]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Arguments_can_match_the_aliases_of_sibling_options(string input)
        {
            var optionX = new Option("-x") { Arity = ArgumentArity.ZeroOrOne};

            var command = new Command("command")
            {
                optionX,
                new Option("-y") { Arity = ArgumentArity.ZeroOrOne}
            };

            var result = command.Parse(input);

            var valueForOption = result.ValueForOption(optionX);

            valueForOption.Should().Be("-y");
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

            result.ValueForArgument(nameArg).Should().Be("name");
            result.ValueForArgument(columnsArg).Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Option_aliases_do_not_need_to_be_prefixed()
        {
            var option = new Option("noprefix");

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

            result.ValueForOption(option).Should().BeTrue();
        }

        [Fact]
        public void When_a_command_line_has_unmatched_tokens_they_are_not_applied_to_subsequent_options()
        {
            var command = new Command("command")
            {
                TreatUnmatchedTokensAsErrors = false
            };
            var optionX = new Option("-x") { Arity = ArgumentArity.ExactlyOne };
            command.AddOption(optionX);
            var optionY = new Option("-y") { Arity = ArgumentArity.ExactlyOne };
            command.AddOption(optionY);

            var result = command.Parse("-x 23 unmatched-token -y 42");

            result.ValueForOption(optionX).Should().Be("23");
            result.ValueForOption(optionY).Should().Be("42");
            result.UnmatchedTokens.Should().BeEquivalentTo("unmatched-token");
        }

        [Fact]
        public void Parse_can_be_called_with_null_args()
        {
            var parser = new Parser();

            var result = parser.Parse(null);

            result.CommandResult.Command.Name.Should().Be(RootCommand.ExecutableName);
        }

        [Fact]
        public void Command_argument_arity_can_be_a_fixed_value_greater_than_1()
        {
            var command = new Command("the-command")
            {
                new Argument
                {
                    Arity = new ArgumentArity(3, 3)
                }
            };

            command.Parse("1 2 3")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument));
        }

        [Fact]
        public void Command_argument_arity_can_be_a_range_with_a_lower_bound_greater_than_1()
        {
            var command = new Command("the-command")
            {
                new Argument
                {
                    Arity = new ArgumentArity(3, 5)
                }
            };

            command.Parse("1 2 3")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument));
            command.Parse("1 2 3 4 5")
                   .CommandResult
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument),
                       new Token("4", TokenType.Argument),
                       new Token("5", TokenType.Argument));
        }

        [Fact]
        public void When_command_arguments_are_fewer_than_minimum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                new Argument
                {
                    Arity = new ArgumentArity(2, 3)
                }
            };

            var result = command.Parse("1");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain(Resources.Instance.RequiredArgumentMissing(result.CommandResult));
        }

        [Fact]
        public void When_command_arguments_are_greater_than_maximum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                new Argument
                {
                    Arity = new ArgumentArity(2, 3)
                }
            };

            ParseResult parseResult = command.Parse("1 2 3 4");

            parseResult
                   .Errors
                   .Select(e => e.Message)
                   .Should()
                   .Contain(Resources.Instance.UnrecognizedCommandOrArgument("4"));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_fixed_value_greater_than_1()
        {
            var option = new Option("-x") { Arity = new ArgumentArity(3, 3)};

            var command = new Command("the-command")
            {
                option
            };

            command.Parse("-x 1 -x 2 -x 3")
                   .FindResultFor(option)
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument));
        }

        [Fact]
        public void Option_argument_arity_can_be_a_range_with_a_lower_bound_greater_than_1()
        {
            var option = new Option("-x") { Arity = new ArgumentArity(3, 5) };

            var command = new Command("the-command")
            {
                option
            };

            command.Parse("-x 1 -x 2 -x 3")
                   .FindResultFor(option)
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument));
            command.Parse("-x 1 -x 2 -x 3 -x 4 -x 5")
                   .FindResultFor(option)
                   .Tokens
                   .Should()
                   .BeEquivalentTo(
                       new Token("1", TokenType.Argument),
                       new Token("2", TokenType.Argument),
                       new Token("3", TokenType.Argument),
                       new Token("4", TokenType.Argument),
                       new Token("5", TokenType.Argument));
        }

        [Fact]
        public void When_option_arguments_are_fewer_than_minimum_arity_then_an_error_is_returned()
        {
            var option = new Option("-x") { Arity = new ArgumentArity(2, 3) };

            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("-x 1");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain(Resources.Instance.RequiredArgumentMissing(result.CommandResult.FindResultFor(option)));
        }

        [Fact]
        public void When_option_arguments_are_greater_than_maximum_arity_then_an_error_is_returned()
        {
            var command = new Command("the-command")
            {
                new Option("-x") { Arity = new ArgumentArity(2, 3)}
            };

            command.Parse("-x 1 2 3 4")
                   .Errors
                   .Select(e => e.Message)
                   .Should()
                   .Contain(Resources.Instance.UnrecognizedCommandOrArgument("4"));
        }


        [Fact]
        public void Argument_with_custom_type_converter_can_be_bound()
        {
            var option = new Option<ClassWithCustomTypeConverter>("--value");

            var parseResult = option.Parse("--value a;b;c");

            var instance = parseResult.ValueForOption(option);

            instance.Values.Should().BeEquivalentTo("a", "b", "c");
        }

        [Fact]
        public void Argument_with_custom_collection_type_converter_can_be_bound()
        {
            var option = new Option<CollectionWithCustomTypeConverter>("--value") { Arity = ArgumentArity.ExactlyOne };

            var parseResult = option.Parse("--value a;b;c");

            CollectionWithCustomTypeConverter instance = parseResult.ValueForOption(option);

            instance.Should().BeEquivalentTo("a", "b", "c");
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
        public void A_subcommand_wont_overflow_when_checking_maximum_argument_capcity()
        {
            // Tests bug identified in https://github.com/dotnet/command-line-api/issues/997

            var argument1 = new Argument("arg1")
            {
                Arity = ArgumentArity.ExactlyOne
            };

            var argument2 = new Argument<string[]>("arg2")
            {
                Arity = ArgumentArity.OneOrMore
            };

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

            Action act = () => parseResult.GetSuggestions();
            act.Should().NotThrow();
        }

        [TypeConverter(typeof(CustomTypeConverter))]
        public class ClassWithCustomTypeConverter
        {
            public string[] Values { get; set; }
        }

        public class CustomTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) ||
                    base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string stringValue)
                {
                    return new ClassWithCustomTypeConverter
                    {
                        Values = stringValue.Split(';')
                    };
                }
                return base.ConvertFrom(context, culture, value);
            }
        }

        [TypeConverter(typeof(CustomCollectionTypeConverter))]
        public class CollectionWithCustomTypeConverter : List<string>
        {
            public CollectionWithCustomTypeConverter(IEnumerable<string> values)
                : base(values)
            { }
        }
      
        public class CustomCollectionTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) ||
                    base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string stringValue)
                {
                    return new CollectionWithCustomTypeConverter(stringValue.Split(';'));
                }
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
