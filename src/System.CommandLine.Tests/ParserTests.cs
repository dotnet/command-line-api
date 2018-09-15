// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.IO;
using FluentAssertions;
using FluentAssertions.Equivalency;
using System.Linq;
using System.Reflection;
using FluentAssertions.Common;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ParserTests
    {
        private readonly ITestOutputHelper _output;

        public ParserTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void An_option_without_a_long_form_can_be_checked_for_using_a_prefix()
        {
            var result = new Parser(
                    new Option("--flag", ""))
                .Parse("--flag");

            result.HasOption("--flag").Should().BeTrue();
        }

        [Fact]
        public void An_option_can_be_checked_by_object_instance()
        {
            var option = new Option("--flag", "");
            var option2 = new Option("--flag2", "");
            var result = new Parser(option, option2)
                .Parse("--flag");

            result.HasOption(option).Should().BeTrue();
            result.HasOption(option2).Should().BeFalse();
        }

        [Fact]
        public void An_option_without_a_long_form_can_be_checked_for_without_using_a_prefix()
        {
            var result = new Parser(
                    new Option("--flag", ""))
                .Parse("--flag");

            result.HasOption("flag").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_short_form_an_option_with_an_alias_can_be_checked_for_by_its_short_form()
        {
            var result = new Parser(
                    new Option(new[] { "-o", "--one" }, ""))
                .Parse("-o");

            result.HasOption("o").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_long_form_an_option_with_an_alias_can_be_checked_for_by_its_short_form()
        {
            var result = new Parser(
                    new Option(new[] { "-o", "--one" }, ""))
                .Parse("--one");

            result.HasOption("o").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_short_form_an_option_with_an_alias_can_be_checked_for_by_its_long_form()
        {
            var result = new Parser(
                    new Option(new[] { "-o", "--one" }, ""))
                .Parse("-o");

            result.HasOption("one").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_long_form_an_option_with_an_alias_can_be_checked_for_by_its_long_form()
        {
            var result = new Parser(
                    new Option(new[] { "-o", "--one" }, ""))
                .Parse("--one");

            result.HasOption("one").Should().BeTrue();
        }

        [Fact]
        public void Two_options_are_parsed_correctly()
        {
            ParseResult result = new Parser(
                    new Option(
                        new[] { "-o", "--one" }, ""),
                    new Option(
                        new[] { "-t", "--two" }, "")
                )
                .Parse("-o -t");

            result.HasOption("o").Should().BeTrue();
            result.HasOption("one").Should().BeTrue();
            result.HasOption("t").Should().BeTrue();
            result.HasOption("two").Should().BeTrue();
        }

        [Fact]
        public void Parse_result_contains_arguments_to_options()
        {
            var result = new CommandLineBuilder()
                         .AddOption(
                             new[] { "-o", "--one" },
                             arguments: args => args.ExactlyOne())
                         .AddOption(
                             new[] { "-t", "--two" },
                             arguments: args => args.ExactlyOne())
                         .Build()
                         .Parse("-o args_for_one -t args_for_two");

            result["one"].Arguments.Single().Should().Be("args_for_one");
            result["two"].Arguments.Single().Should().Be("args_for_two");
        }

        [Fact]
        public void When_no_options_are_specified_then_an_error_is_returned()
        {
            Action create = () => new Parser();

            create.Should()
                  .Throw<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("You must specify at least one option.");
        }

        [Fact]
        public void Two_options_cannot_have_conflicting_aliases()
        {
            Action create = () =>
                new Parser(new Option(
                               new[] { "-o", "--one" }, ""),
                           new Option(
                               new[] { "-t", "--one" }, ""));

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
            var result = new Parser(new Option(new[] { "-o", "--one" }, ""))
                .Parse("-o \"some stuff\" -- -x -y -z -o:foo");

            result.HasOption("o")
                  .Should()
                  .BeTrue();

            result.UnparsedTokens
                  .Should()
                  .HaveCount(4);
        }

        [Fact]
        public void The_portion_of_the_command_line_following_a_double_slash_is_accessible_as_UnparsedTokens()
        {
            var result = new Parser(new Option("-o", ""))
                .Parse("-o \"some stuff\" -- x y z");

            result.UnparsedTokens
                  .Should()
                  .ContainInOrder("x", "y", "z");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_equals_delimiter()
        {
            var parser = new Parser(new Option(
                                        "-x",
                                        "",
                                        argument: new ArgumentBuilder().ExactlyOne()));

            var result = parser.Parse("-x=some-value");

            result.Errors.Should().BeEmpty();

            result["x"].Arguments.Should().ContainSingle(a => a == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_equals_delimiter()
        {
            var parser = new Parser(
                new Option(
                    "--hello",
                    "",
                    argument: new ArgumentBuilder().ExactlyOne()));

            var result = parser.Parse("--hello=there");

            result.Errors.Should().BeEmpty();

            result["hello"].Arguments.Should().ContainSingle(a => a == "there");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_colon_delimiter()
        {
            var parser = new Parser(
                new Option(
                    "-x",
                    "",
                    argument: new ArgumentBuilder().ExactlyOne()));

            var result = parser.Parse("-x:some-value");

            result.Errors.Should().BeEmpty();

            result["x"].Arguments.Should().ContainSingle(a => a == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_colon_delimiter()
        {
            var parser = new CommandLineBuilder()
                         .AddOption("--hello", "",
                                    args => args.ExactlyOne())
                         .Build();

            var result = parser.Parse("--hello:there");

            result.Errors.Should().BeEmpty();

            result["hello"].Arguments.Should().ContainSingle(a => a == "there");
        }

        [Fact]
        public void Option_short_forms_can_be_bundled()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("the-command", "",
                                     c => c.AddOption("-x")
                                           .AddOption("-y")
                                           .AddOption("-z"))
                         .Build();

            var result = parser.Parse("the-command -xyz");

            result.CommandResult
                  .Children
                  .Select(o => o.Name)
                  .Should()
                  .BeEquivalentTo("x", "y", "z");
        }

        [Fact]
        public void Options_short_forms_do_not_get_unbundled_if_unbundling_is_turned_off()
        {
            var parser = new CommandLineBuilder()
                         .EnablePosixBundling(false)
                         .AddCommand("the-command", "", c =>
                                         c.AddOption("-x", "")
                                          .AddOption("-y", "")
                                          .AddOption("-z", ""))
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
                new Command("the-command", "", new[] {
                    new Option("--xyz", ""),
                    new Option("-x", ""),
                    new Option("-y", ""),
                    new Option("-z", "")
                }));

            var result = parser.Parse("the-command --xyz");

            result.CommandResult
                  .Children
                  .Select(o => o.Name)
                  .Should()
                  .BeEquivalentTo("xyz");
        }

        [Fact]
        public void Options_do_not_get_unbundled_unless_all_resulting_options_would_be_valid_for_the_current_command()
        {
            var parser = new Parser(
                new Command("outer", "",
                                      new Symbol[] {
                                          new Option("-a", ""),
                                          new Command("inner", "",
                                                                new[] {
                                                                    new Option("-b", ""),
                                                                    new Option("-c", "")
                                                                },
                                                                new ArgumentBuilder().ZeroOrMore())
                                      }));

            ParseResult result = parser.Parse("outer inner -abc");

            result.CommandResult
                  .Children
                  .Should()
                  .BeEmpty();

            result.CommandResult
                  .Arguments
                  .Should()
                  .BeEquivalentTo("-abc");
        }

        [Fact]
        public void Parser_root_Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
            var parser = new Parser(
                new Option(
                    new[] { "-a", "--animals" }, "",
                    argument: new ArgumentBuilder().ZeroOrMore()),
                new Option(
                    new[] { "-v", "--vegetables" }, "",
                    argument: new ArgumentBuilder().ZeroOrMore()));

            var result = parser.Parse("-a cat -v carrot -a dog");

            result["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat", "dog");

            result["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");
        }

        [Fact]
        public void Command_Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
            var builder = new ArgumentBuilder();
            Argument rule = builder.FromAmong("dog", "cat", "sheep").ZeroOrMore();

            var parser = new Parser(
                new Command("the-command", "", new[] {
                    new Option(
                        new[] { "-a", "--animals" },
                        "",
                        argument: rule),
                    new Option(
                        new[] { "-v", "--vegetables" },
                        "",
                        argument: new ArgumentBuilder().ZeroOrMore())
                }));

            var result = parser.Parse("the-command -a cat -v carrot -a dog");

            var command = result.CommandResult;

            command["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat", "dog");

            command["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");
        }

        [Fact]
        public void When_a_Parser_root_option_is_not_respecified_then_the_following_token_is_unmatched()
        {
            var parser = new Parser(
                new Option(
                    new[] { "-a", "--animals" },
                    "",
                    argument: new ArgumentBuilder().ZeroOrMore()),
                new Option(
                    new[] { "-v", "--vegetables" },
                    "",
                    argument: new ArgumentBuilder().ZeroOrMore()));

            ParseResult result = parser.Parse("-a cat some-arg -v carrot");

            result["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat");

            result["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");

            result
                .UnmatchedTokens
                .Should()
                .BeEquivalentTo("some-arg");
        }

        [Fact]
        public void When_a_Command_option_is_not_respecified_then_the_following_token_is_considered_an_argument_to_the_outer_command()
        {
            var parser = new Parser(
                new Command("the-command", "", new[] {
                                          new Option(
                                              new[] { "-a", "--animals" }, "",
                                              argument: new ArgumentBuilder().ZeroOrMore()),
                                          new Option(
                                              new[] { "-v", "--vegetables" }, "",
                                              argument: new ArgumentBuilder().ZeroOrMore())
                                      },
                                      new ArgumentBuilder().ZeroOrMore()));

            ParseResult result = parser.Parse("the-command -a cat some-arg -v carrot");

            var command = result.CommandResult;

            command["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat");

            command["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");

            command
                .Arguments
                .Should()
                .BeEquivalentTo("some-arg");
        }

        [Fact]
        public void Option_with_multiple_nested_options_allowed_is_parsed_correctly()
        {
            var option = new Command("outer", "",
                                               new[] {
                                                   new Option(
                                                       "--inner1", "",
                                                       argument: new ArgumentBuilder().ExactlyOne()),
                                                   new Option(
                                                       "--inner2", "",
                                                       argument: new ArgumentBuilder().ExactlyOne())
                                               });

            var parser = new Parser(option);

            var result = parser.Parse("outer --inner1 argument1 --inner2 argument2");

            _output.WriteLine(result.Diagram());

            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o =>
                                     o.Name == "inner1" &&
                                     o.Arguments.Single() == "argument1");
            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o =>
                                     o.Name == "inner2" &&
                                     o.Arguments.Single() == "argument2");
        }

        [Fact]
        public void Relative_order_of_arguments_and_options_does_not_matter()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("move", "",
                                     move => move.AddOption("-X", "",
                                                            xArgs => xArgs.ExactlyOne()),
                                     moveArgs => moveArgs.OneOrMore())
                         .Build();

            // option before args
            ParseResult result1 = parser.Parse(
                "move -X the-arg-for-option-x ARG1 ARG2");

            // option between two args
            ParseResult result2 = parser.Parse(
                "move ARG1 -X the-arg-for-option-x ARG2");

            // option after args
            ParseResult result3 = parser.Parse(
                "move ARG1 ARG2 -X the-arg-for-option-x");

            // arg order reversed
            ParseResult result4 = parser.Parse(
                "move ARG2 ARG1 -X the-arg-for-option-x");

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
            result1.Should()
                   .BeEquivalentTo(
                       result4,
                       x => x.IgnoringCyclicReferences()
                             .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal)));
        }

        [Fact]
        public void An_outer_command_with_the_same_name_does_not_capture()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("one", "",
                                     one => {
                                         one.AddCommand("two", "",
                                                        two => two.AddCommand("three", ""));
                                         one.AddCommand("three", "");
                                     })
                         .Build();

            ParseResult result = parser.Parse("one two three");

            result.Diagram().Should().Be($"[ {CommandLineBuilder.ExeName} [ one [ two [ three ] ] ] ]");
        }

        [Fact]
        public void An_inner_command_with_the_same_name_does_not_capture()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand("one", "",
                                     one => {
                                         one.AddCommand("two", "",
                                                        two => two.AddCommand("three", ""));
                                         one.AddCommand("three", "");
                                     })
                         .Build();

            ParseResult result = parser.Parse("one three");

            result.Diagram().Should().Be($"[ {CommandLineBuilder.ExeName} [ one [ three ] ] ]");
        }

        [Fact]
        public void When_nested_commands_all_acccept_arguments_then_the_nearest_captures_the_arguments()
        {
            var command = new CommandLineBuilder()
                          .AddCommand("outer", "",
                                      arguments: outerArgs => outerArgs.ZeroOrMore(),
                                      symbols: outer => outer.AddCommand("inner", "", arguments: innerArgs => innerArgs.ZeroOrMore()))
                          .BuildCommand();

            ParseResult result = command.Parse("outer arg1 inner arg2");

            result.CommandResult.Parent.Arguments.Should().BeEquivalentTo("arg1");

            result.CommandResult.Arguments.Should().BeEquivalentTo("arg2");
        }

        [Fact]
        public void Nested_commands_with_colliding_names_cannot_both_be_applied()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(
                             "outer", "",
                             arguments: outerArgs => outerArgs.ExactlyOne(),
                             symbols: outer =>
                                 outer.AddCommand(
                                          "non-unique", "",
                                          arguments: args => args.ExactlyOne())
                                      .AddCommand(
                                          "inner", "",
                                          arguments: args => args.ExactlyOne(),
                                          symbols: inner => inner.AddCommand(
                                              "non-unique", "",
                                              arguments: innerArgs => innerArgs.ExactlyOne())))
                         .Build();

            ParseResult result = parser.Parse("outer arg1 inner arg2 non-unique arg3 ");

            _output.WriteLine(result.Diagram());

            result.Diagram().Should().Be($"[ {CommandLineBuilder.ExeName} [ outer [ inner [ non-unique <arg3> ] <arg2> ] <arg1> ] ]");
        }

        [Fact]
        public void When_child_option_will_not_accept_arg_then_parent_can()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(
                             "the-command", "",
                             arguments: commandArgs => commandArgs.ZeroOrMore(),
                             symbols: cmd => cmd.AddOption("-x", "", optionArgs => optionArgs.None()))
                         .Build();

            ParseResult result = parser.Parse("the-command -x two");

            var command = result.CommandResult;
            command["x"].Arguments.Should().BeEmpty();
            command.Arguments.Should().BeEquivalentTo("two");
        }

        [Fact]
        public void When_parent_option_will_not_accept_arg_then_child_can()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(
                             "the-command", "",
                             arguments: commandArgs => commandArgs.None(),
                             symbols: cmd => cmd.AddOption("-x", "", optionArgs => optionArgs.ExactlyOne()))
                         .Build();

            ParseResult result = parser.Parse("the-command -x two");

            result.CommandResult["x"].Arguments.Should().BeEquivalentTo("two");
            result.CommandResult.Arguments.Should().BeEmpty();
        }

        [Fact]
        public void When_the_same_option_is_defined_on_both_outer_and_inner_command_and_specified_at_the_end_then_it_attaches_to_the_inner_command()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(
                             "outer", "",
                             outer => {
                                 outer.AddCommand(
                                     "inner", "",
                                     inner => inner.AddOption("-x", ""));
                                 outer.AddOption("-x", "");
                             })
                         .Build();

            ParseResult result = parser.Parse("outer inner -x");

            result.CommandResult
                  .Parent
                  .Children
                  .Should()
                  .NotContain(o => o.Name == "x");
            result.CommandResult
                  .Children
                  .Should()
                  .ContainSingle(o => o.Name == "x");
        }

        [Fact]
        public void When_the_same_option_is_defined_on_both_outer_and_inner_command_and_specified_in_between_then_it_attaches_to_the_outer_command()
        {
            var parser = new CommandLineBuilder()
                         .AddCommand(
                             "outer", "",
                             outer => {
                                 outer.AddCommand(
                                     "inner", "",
                                     inner => inner.AddOption("-x", ""));
                                 outer.AddOption("-x", "");
                             })
                         .Build();

            var result = parser.Parse("outer -x inner");

            result.CommandResult
                  .Children
                  .Should()
                  .BeEmpty();
            result.CommandResult
                  .Parent
                  .Children
                  .Should()
                  .ContainSingle(o => o.Name == "x");
        }

        [Fact]
        public void Arguments_only_apply_to_the_nearest_command()
        {
            var command = new CommandLineBuilder()
                          .AddCommand("outer", "",
                                      outer => outer.AddCommand("inner", "",
                                                                arguments: innerArgs => innerArgs.ExactlyOne()),
                                      outerArgs => outerArgs.ExactlyOne())
                          .BuildCommand();

            ParseResult result = command.Parse("outer inner arg1 arg2");

            result.CommandResult
                  .Parent
                  .Arguments
                  .Should()
                  .BeEmpty();
            result.CommandResult
                  .Arguments
                  .Should()
                  .BeEquivalentTo("arg1");
            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo("arg2");
        }

        [Fact]
        public void Subsequent_occurrences_of_tokens_matching_command_names_are_parsed_as_arguments()
        {
            var command = new CommandBuilder("the-command")
                          .AddCommand("complete", "",
                                      completeCmd => completeCmd.AddOption("--position", "",
                                                                           positionArgs => positionArgs.ExactlyOne()),
                                      completeArgs => completeArgs.ExactlyOne())
                          .BuildCommand();

            ParseResult result = command.Parse("the-command",
                                               "complete",
                                               "--position",
                                               "7",
                                               "the-command");

            CommandResult complete = result.CommandResult;

            _output.WriteLine(result.Diagram());

            complete.Arguments.Should().BeEquivalentTo("the-command");
        }

        [Fact]
        public void A_root_command_can_be_omitted_from_the_parsed_args()
        {
            var command = new Command("outer", "", new[] {
                new Command("inner", "", new[] {
                    new Option(
                        "-x",
                        "",
                        argument: new ArgumentBuilder().ExactlyOne())
                })
            });

            var result1 = command.Parse("inner -x hello");
            var result2 = command.Parse("outer inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void A_root_command_can_match_a_full_path_to_an_executable()
        {
            var command = new Command("outer", "", new[] {
                new Command("inner", "", new[] {
                    new Option(
                        "-x",
                        "",
                        argument: new ArgumentBuilder().ExactlyOne())
                })
            });

            ParseResult result1 = command.Parse("inner -x hello");

            var exePath = Path.Combine("dev", "outer.exe");
            ParseResult result2 = command.Parse($"{exePath} inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void When_no_commands_are_added_then_ParseResult_Command_identifies_executable()
        {
            var parser = new CommandLineBuilder()
                         .AddOption("-x", "")
                         .AddOption("-y", "")
                         .Build();

            var result = parser.Parse("-x -y");

            var command = result.CommandResult.Command;

            command.Should().NotBeNull();

            command.Name.Should().Be(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
        }

        [Fact]
        public void An_implicit_root_command_is_returned_by_Command()
        {
            var result = new CommandLineBuilder()
                         .AddOption("-x")
                         .AddOption("-y")
                         .Build()
                         .Parse("-x -y");

            var command = result.CommandResult;

            command.Should().NotBeNull();

            command.Name.Should().Be(Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
        }

        [Fact]
        public void Absolute_unix_style_paths_are_lexed_correctly()
        {
            var command =
                @"rm ""/temp/the file.txt""";

            var parser = new Parser(new Command("rm", "", new ArgumentBuilder().ZeroOrMore()));

            var result = parser.Parse(command);

            result.CommandResult
                  .Arguments
                  .Should()
                  .OnlyContain(a => a == @"/temp/the file.txt");
        }

        [Fact]
        public void Absolute_Windows_style_paths_are_lexed_correctly()
        {
            var command =
                @"rm ""c:\temp\the file.txt\""";

            var parser = new Parser(new Command("rm", "", new ArgumentBuilder().ZeroOrMore()));

            ParseResult result = parser.Parse(command);

            result.CommandResult
                  .Arguments
                  .Should()
                  .OnlyContain(a => a == @"c:\temp\the file.txt\");
        }

        [Fact]
        public void Commands_can_have_default_argument_values()
        {
            var command = new CommandLineBuilder()
                          .AddCommand("command", "",
                                      arguments:
                                      args => args.WithDefaultValue(() => "default")
                                                  .ExactlyOne())
                          .BuildCommand();

            ParseResult result = command.Parse("command");

            result.CommandResult.GetValueOrDefault().Should().Be("default");
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_the_option_can_still_be_accessed_as_though_it_had_been_applied()
        {
            var command = new Command("command", "", new[] {
                new Option(
                    new[] { "-o", "--option" },
                    "",
                    argument: new ArgumentBuilder()
                                        .WithDefaultValue(() => "the-default")
                                        .ExactlyOne())
            });

            ParseResult result = command.Parse("command");

            result.HasOption("o").Should().BeTrue();
            result.HasOption("option").Should().BeTrue();
            result["o"].GetValueOrDefault<string>().Should().Be("the-default");
            result.CommandResult.ValueForOption("o").Should().Be("the-default");
        }

        [Fact]
        public void Unmatched_options_are_not_split_into_smaller_tokens()
        {
            var command = new CommandLineBuilder()
                          .AddCommand("outer", "",
                                      outer => outer.AddOption("-p", "")
                                                    .AddCommand("inner", "",
                                                                inner => inner.AddOption("-o", ""), args => args.OneOrMore()))
                          .BuildCommand();

            ParseResult result = command.Parse("outer inner -p:RandomThing=random");

            _output.WriteLine(result.Diagram());

            result.CommandResult
                  .Arguments
                  .Should()
                  .BeEquivalentTo("-p:RandomThing=random");
        }

        [Fact]
        public void The_default_behavior_of_unmatched_tokens_resulting_in_errors_can_be_turned_off()
        {
            var parser = new CommandLineBuilder()
                         .TreatUnmatchedTokensAsErrors(false)
                         .AddCommand("the-command", "", arguments: args => args.ExactlyOne())
                         .Build();

            ParseResult result = parser.Parse("the-command arg1 arg2");

            result.Errors.Should().BeEmpty();

            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo("arg2");
        }

        [Fact]
        public void Argument_names_can_collide_with_option_names()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    "--one",
                    "",
                    argument: new ArgumentBuilder().ExactlyOne())
            });

            ParseResult result = command.Parse("the-command --one one");

            result.CommandResult["one"]
                  .Arguments
                  .Should()
                  .BeEquivalentTo("one");
        }

        [Fact]
        public void Option_and_Command_can_have_the_same_alias()
        {
            var innerCommand = new Command(
                "inner", "",
                symbols: null,
                argument: new ArgumentBuilder().ZeroOrMore());

            var option = new Option("--inner", "");

            var outerCommand = new Command(
                "outer", "",
                new Symbol[] {
                    innerCommand,
                    option
                },
                new ArgumentBuilder().ZeroOrMore());

            var parser = new Parser(outerCommand);

            parser.Parse("outer inner").CommandResult
                  .Command
                  .Should()
                  .Be(innerCommand);

            parser.Parse("outer --inner").CommandResult
                  .Command
                  .Should()
                  .Be(outerCommand);

            parser.Parse("outer --inner inner").CommandResult
                  .Command
                  .Should()
                  .Be(innerCommand);

            parser.Parse("outer --inner inner").CommandResult
                  .Parent
                  .Children
                  .Should()
                  .Contain(c => c.Symbol == option);
        }

        [Fact]
        public void Options_can_have_the_same_alias_differentiated_only_by_prefix()
        {
            var option1 = new Option(new[] { "-a" }, "");
            var option2 = new Option(new[] { "--a" }, "");

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
            ParseResult parseResult = new Parser(
                    new Option(
                        "-x",
                        "",
                        new ArgumentBuilder().ZeroOrMore()))
                .Parse(input);

            parseResult["x"].Arguments
                            .Should()
                            .BeEquivalentTo(new[] { expected });
        }

        [Theory]
        [InlineData("-x -y")]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Arguments_can_start_with_prefixes_that_make_them_look_like_options(string input)
        {
            var parser = new CommandLineBuilder()
                         .AddOption("-x", "", args => args.ZeroOrOne())
                         .AddOption("-z", "", args => args.ZeroOrOne())
                         .Build();

            var result = parser.Parse(input);

            var valueForOption = result.ValueForOption("-x");

            valueForOption.Should().Be("-y");
        }

        [Theory]
        [InlineData("-x=-y")]
        [InlineData("-x:-y")]
        public void Arguments_can_match_the_aliases_of_sibling_options(string input)
        {
            var parser = new CommandLineBuilder()
                         .AddOption("-x", "", args => args.ZeroOrOne())
                         .AddOption("-y", "", args => args.ZeroOrOne())
                         .Build();

            var result = parser.Parse(input);

            var valueForOption = result.ValueForOption("-x");

            valueForOption.Should().Be("-y");
        }

        [Theory]
        [InlineData("/o")]
        [InlineData("-o")]
        [InlineData("--o")]
        [InlineData("/output")]
        [InlineData("-output")]
        [InlineData("--output")]
        public void Option_aliases_can_be_specified_and_are_prefixed_with_defaults(string input)
        {
            var option = new Option(new[] { "output", "o" }, "");
            var configuration = new CommandLineConfiguration(
                new[] { option },
                prefixes: new[] { "-", "--", "/" });
            var parser = new Parser(configuration);

            ParseResult parseResult = parser.Parse(input);
            parseResult["output"].Should().NotBeNull();
            parseResult["o"].Should().NotBeNull();
        }

        [Theory]
        [InlineData("/o")]
        [InlineData("-o")]
        [InlineData("--output")]
        [InlineData("--out")]
        [InlineData("-out")]
        [InlineData("/out")]
        public void Option_aliases_can_be_specified_for_particular_prefixes(string input)
        {
            var option = new Option(new[] { "--output", "-o", "/o", "out" }, "");
            var configuration = new CommandLineConfiguration(
                new[] { option },
                prefixes: new[] { "-", "--", "/" });
            var parser = new Parser(configuration);

            ParseResult parseResult = parser.Parse(input);
            parseResult["o"].Should().NotBeNull();
            parseResult["out"].Should().NotBeNull();
            parseResult["output"].Should().NotBeNull();
        }

        [Fact]
        public void Boolean_options_with_no_argument_specified_do_not_match_subsequent_arguments()
        {
            var parser = new CommandLineBuilder()
                         .AddOption("-v", "", builder => builder.ParseArgumentsAs<bool>())
                         .AddArguments(builder => builder.OneOrMore())
                         .Build();

            var result = parser.Parse("-v an-argument");

            _output.WriteLine(result.ToString());

            result.ValueForOption("v").Should().Be(true);
        }
    }
}
