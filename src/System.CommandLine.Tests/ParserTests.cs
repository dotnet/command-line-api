// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using FluentAssertions.Common;
using FluentAssertions.Equivalency;
using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public class ParserTests
    {
        private readonly ITestOutputHelper output;

        public ParserTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void An_option_without_a_long_form_can_be_checked_for_using_a_prefix()
        {
            var result = new OptionParser(new OptionDefinition(
                                              "--flag",
                                              "",
                                              argumentDefinition: null))
                .Parse("--flag");

            result.HasOption("--flag").Should().BeTrue();
        }

        [Fact]
        public void An_option_without_a_long_form_can_be_checked_for_without_using_a_prefix()
        {
            var result = new OptionParser(new OptionDefinition(
                                              "--flag",
                                              "",
                                              argumentDefinition: null))
                .Parse("--flag");

            result.HasOption("flag").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_short_form_an_option_with_an_alias_can_be_checked_for_by_its_short_form()
        {
            var result = new OptionParser(new OptionDefinition(
                                              new[] {"-o", "--one"},
                                              "",
                                              argumentDefinition: null))
                .Parse("-o");

            result.HasOption("o").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_long_form_an_option_with_an_alias_can_be_checked_for_by_its_short_form()
        {
            var result = new OptionParser(new OptionDefinition(
                                              new[] {"-o", "--one"},
                                              "",
                                              argumentDefinition: null))
                .Parse("--one");

            result.HasOption("o").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_short_form_an_option_with_an_alias_can_be_checked_for_by_its_long_form()
        {
            var result = new OptionParser(new OptionDefinition(
                                              new[] {"-o", "--one"},
                                              "",
                                              argumentDefinition: null))
                .Parse("-o");

            result.HasOption("one").Should().BeTrue();
        }

        [Fact]
        public void When_invoked_by_its_long_form_an_option_with_an_alias_can_be_checked_for_by_its_long_form()
        {
            var result = new OptionParser(new OptionDefinition(
                                              new[] {"-o", "--one"},
                                              "",
                                              argumentDefinition: null))
                .Parse("--one");

            result.HasOption("one").Should().BeTrue();
        }

        [Fact]
        public void Two_options_are_parsed_correctly()
        {
            OptionParseResult result = new OptionParser(
                    new OptionDefinition(
                        new[] {"-o", "--one"},
                        "",
                        argumentDefinition: null),
                    new OptionDefinition(
                        new[] {"-t", "--two"},
                        "",
                        argumentDefinition: null)
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
            OptionParseResult result = new OptionParser(
                    new OptionDefinition(
                        new[] {"-o", "--one"},
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()),
                    new OptionDefinition(
                        new[] {"-t", "--two"},
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()))
                .Parse("-o args_for_one -t args_for_two");

            result["one"].Arguments.Single().Should().Be("args_for_one");
            result["two"].Arguments.Single().Should().Be("args_for_two");
        }

        [Fact]
        public void When_no_options_are_specified_then_an_error_is_returned()
        {
            Action create = () => new OptionParser();

            create.ShouldThrow<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("You must specify at least one option.");
        }

        [Fact]
        public void Two_options_cannot_have_conflicting_aliases()
        {
            Action create = () =>
                new OptionParser(new OptionDefinition(
                                     new[] {"-o", "--one"},
                                     "",
                                     argumentDefinition: null), new OptionDefinition(
                                     new[] {"-t", "--one"},
                                     "",
                                     argumentDefinition: null));

            create.ShouldThrow<ArgumentException>()
                  .Which
                  .Message
                  .Should()
                  .Be("Alias '--one' is already in use.");
        }

        [Fact]
        public void A_double_dash_delimiter_specifies_that_no_further_command_line_args_will_be_treated_as_options()
        {
            var result = new OptionParser(new OptionDefinition(
                                              new[] {"-o", "--one"},
                                              "",
                                              argumentDefinition: null))
                .Parse("-o \"some stuff\" -- -x -y -z -o:foo");

            result.HasOption("o")
                  .Should()
                  .BeTrue();

            result.Symbols
                  .Should()
                  .HaveCount(1);

            result.UnparsedTokens
                  .Should()
                  .HaveCount(4);
        }

        [Fact]
        public void The_portion_of_the_command_line_following_a_double_slash_is_accessible_as_UnparsedTokens()
        {
            var result = new OptionParser(new OptionDefinition(
                                              "-o",
                                              "",
                                              argumentDefinition: null))
                .Parse("-o \"some stuff\" -- x y z");

            result.UnparsedTokens
                  .Should()
                  .ContainInOrder("x", "y", "z");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_equals_delimiter()
        {
            var parser = new OptionParser(new OptionDefinition(
                                              "-x",
                                              "",
                                              argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()));

            var result = parser.Parse("-x=some-value");

            result.Errors.Should().BeEmpty();

            result["x"].Arguments.Should().ContainSingle(a => a == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_equals_delimiter()
        {
            var parser = new OptionParser(new OptionDefinition(
                                              "--hello",
                                              "",
                                              argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()));

            var result = parser.Parse("--hello=there");

            result.Errors.Should().BeEmpty();

            result["hello"].Arguments.Should().ContainSingle(a => a == "there");
        }

        [Fact]
        public void Short_form_options_can_be_specified_using_colon_delimiter()
        {
            var parser = new OptionParser(new OptionDefinition(
                                              "-x",
                                              "",
                                              argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()));

            var result = parser.Parse("-x:some-value");

            result.Errors.Should().BeEmpty();

            result["x"].Arguments.Should().ContainSingle(a => a == "some-value");
        }

        [Fact]
        public void Long_form_options_can_be_specified_using_colon_delimiter()
        {
            var parser = new OptionParser(new OptionDefinition(
                                              "--hello",
                                              "",
                                              argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()));

            var result = parser.Parse("--hello:there");

            result.Errors.Should().BeEmpty();

            result["hello"].Arguments.Should().ContainSingle(a => a == "there");
        }

        [Fact]
        public void Option_short_forms_can_be_bundled()
        {
            var parser = new CommandParser(
                Create.Command("the-command", "",
                    new OptionDefinition(
                        "-x",
                        "",
                        argumentDefinition: ArgumentDefinition.None),
                    new OptionDefinition(
                        "-y",
                        "",
                        argumentDefinition: ArgumentDefinition.None),
                    new OptionDefinition(
                        "-z",
                        "",
                        argumentDefinition: ArgumentDefinition.None)));

            var result = parser.Parse("the-command -xyz");

            ParseResultExtensions.Command(result)
                .Children
                .Select(o => o.Name)
                .Should()
                .BeEquivalentTo("x", "y", "z");
        }

        [Fact]
        public void Options_short_forms_do_not_get_unbundled_if_unbundling_is_turned_off()
        {
            CommandDefinition commandDefinition = Create.Command("the-command", "",
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: ArgumentDefinition.None),
                new OptionDefinition(
                    "-y",
                    "",
                    argumentDefinition: ArgumentDefinition.None),
                new OptionDefinition(
                    "-z",
                    "",
                    argumentDefinition: ArgumentDefinition.None),
                new OptionDefinition(
                    "-xyz",
                    "",
                    argumentDefinition: ArgumentDefinition.None));
            var parseConfig = new ParserConfiguration(new[] { commandDefinition }, allowUnbundling: false);
            var parser = new CommandParser(parseConfig);
            var result = parser.Parse("the-command -xyz");

            ParseResultExtensions.Command(result)
                .Children
                .Select(o => o.Name)
                .Should()
                .BeEquivalentTo("xyz");
        }

        [Fact]
        public void Option_long_forms_do_not_get_unbundled()
        {
            var parser = new CommandParser(
                Create.Command("the-command", "",
                    new OptionDefinition(
                        "--xyz",
                        "",
                        argumentDefinition: ArgumentDefinition.None),
                    new OptionDefinition(
                        "-x",
                        "",
                        argumentDefinition: ArgumentDefinition.None),
                    new OptionDefinition(
                        "-y",
                        "",
                        argumentDefinition: ArgumentDefinition.None),
                    new OptionDefinition(
                        "-z",
                        "",
                        argumentDefinition: ArgumentDefinition.None)));

            var result = parser.Parse("the-command --xyz");

            ParseResultExtensions.Command(result)
                .Children
                .Select(o => o.Name)
                .Should()
                .BeEquivalentTo("xyz");
        }

        [Fact]
        public void Options_do_not_get_unbundled_unless_all_resulting_options_would_be_valid_for_the_current_command()
        {
            var parser = new CommandParser(
                Create.Command("outer", "",
                    new OptionDefinition(
                        "-a",
                        "",
                        argumentDefinition: null),
                    Create.Command("inner", "", new ArgumentDefinitionBuilder().ZeroOrMore(),
                        new OptionDefinition(
                            "-b",
                            "",
                            argumentDefinition: null),
                        new OptionDefinition(
                            "-c",
                            "",
                            argumentDefinition: null))));

            CommandParseResult result = parser.Parse("outer inner -abc");

            ParseResultExtensions.Command(result)
                  .Children
                  .Should()
                  .BeEmpty();

            ParseResultExtensions.Command(result)
                  .Arguments
                  .Should()
                  .BeEquivalentTo("-abc");
        }

        [Fact]
        public void Parser_root_Options_can_be_specified_multiple_times_and_their_arguments_are_collated()
        {
            var parser = new OptionParser(
                new OptionDefinition(
                    new[] {"-a", "--animals"},
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore()),
                new OptionDefinition(
                    new[] {"-v", "--vegetables"},
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore()));

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
            var builder = new ArgumentDefinitionBuilder();
            ArgumentDefinition rule = builder.FromAmong("dog", "cat", "sheep").ZeroOrMore();

            var parser = new CommandParser(
                Create.Command("the-command", "",
                    new OptionDefinition(
                        new[] {"-a", "--animals"},
                        "",
                        argumentDefinition: rule),
                    new OptionDefinition(
                        new[] {"-v", "--vegetables"},
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore())));

            CommandParseResult result = parser.Parse("the-command -a cat -v carrot -a dog");

            var parsedCommand = ParseResultExtensions.Command(result);

            parsedCommand["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat", "dog");

            parsedCommand["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");
        }

        [Fact]
        public void When_a_Parser_root_option_is_not_respecified_then_the_following_token_is_unmatched()
        {
            var parser = new OptionParser(
                new OptionDefinition(
                    new[] {"-a", "--animals"},
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore()),
                new OptionDefinition(
                    new[] {"-v", "--vegetables"},
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore()));

            OptionParseResult result = parser.Parse("-a cat some-arg -v carrot");

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
            var parser = new CommandParser(
                Create.Command("the-command", "", new ArgumentDefinitionBuilder().ZeroOrMore(),
                    new OptionDefinition(
                        new[] {"-a", "--animals"},
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore()),
                    new OptionDefinition(
                        new[] {"-v", "--vegetables"},
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore())));

            CommandParseResult result = parser.Parse("the-command -a cat some-arg -v carrot");

            var parsedCommand = ParseResultExtensions.Command(result);

            parsedCommand["animals"]
                .Arguments
                .Should()
                .BeEquivalentTo("cat");

            parsedCommand["vegetables"]
                .Arguments
                .Should()
                .BeEquivalentTo("carrot");

            parsedCommand
                .Arguments
                .Should()
                .BeEquivalentTo("some-arg");
        }

        [Fact]
        public void Option_with_multiple_nested_options_allowed_is_parsed_correctly()
        {
            var option = Create.Command("outer", "",
                new OptionDefinition(
                    "--inner1",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()),
                new OptionDefinition(
                    "--inner2",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()));

            var parser = new CommandParser(option);

            var result = parser.Parse("outer --inner1 argument1 --inner2 argument2");

            output.WriteLine(result.Diagram());

            var applied = result.Symbols.Single();

            applied.Children
                   .Should()
                   .ContainSingle(o =>
                                      o.Name == "inner1" &&
                                      o.Arguments.Single() == "argument1");
            applied.Children
                   .Should()
                   .ContainSingle(o =>
                                      o.Name == "inner2" &&
                                      o.Arguments.Single() == "argument2");
        }

        [Fact]
        public void Relative_order_of_arguments_and_options_does_not_matter()
        {
            var parser = new CommandParser(
                Create.Command("move", "", new ArgumentDefinitionBuilder().OneOrMore(),
                    new OptionDefinition(
                        "-X",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())));

            // option before args
            CommandParseResult result1 = parser.Parse(
                "move -X the-arg-for-option-x ARG1 ARG2");

            // option between two args
            CommandParseResult result2 = parser.Parse(
                "move ARG1 -X the-arg-for-option-x ARG2");

            // option after args
            CommandParseResult result3 = parser.Parse(
                "move ARG1 ARG2 -X the-arg-for-option-x");

            // arg order reversed
            CommandParseResult result4 = parser.Parse(
                "move ARG2 ARG1 -X the-arg-for-option-x");

            // all should be equivalent
            result1.ShouldBeEquivalentTo(
                result2,
                x => x.IgnoringCyclicReferences()
                      .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal)));
            result1.ShouldBeEquivalentTo(
                result3,
                x => x.IgnoringCyclicReferences()
                      .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal)));
            result1.ShouldBeEquivalentTo(
                result4,
                x => x.IgnoringCyclicReferences()
                      .Excluding(y => y.WhichGetterHas(CSharpAccessModifier.Internal)));
        }

        [Fact]
        public void An_outer_command_with_the_same_name_does_not_capture()
        {
            var command = Create.Command("one", "",
                Create.Command("two", "",
                    Create.Command("three", "")),
                Create.Command("three", ""));

            CommandParseResult result = command.Parse("one two three");

            result.Diagram().Should().Be("[ one [ two [ three ] ] ]");
        }

        [Fact]
        public void An_inner_command_with_the_same_name_does_not_capture()
        {
            var command = Create.Command("one", "",
                Create.Command("two", "",
                    Create.Command("three", "")),
                Create.Command("three", ""));

            CommandParseResult result = command.Parse("one three");

            result.Diagram().Should().Be("[ one [ three ] ]");
        }

        [Fact]
        public void When_nested_commands_all_acccept_arguments_then_the_nearest_captures_the_arguments()
        {
            var command = Create.Command("outer", "", new ArgumentDefinitionBuilder().ZeroOrMore(),
                Create.Command("inner", "", new ArgumentDefinitionBuilder().ZeroOrMore()));

            CommandParseResult result = command.Parse("outer arg1 inner arg2");

            ParseResultExtensions.Command(result).Parent.Arguments.Should().BeEquivalentTo("arg1");

            ParseResultExtensions.Command(result).Arguments.Should().BeEquivalentTo("arg2");
        }

        [Fact]
        public void Nested_commands_with_colliding_names_cannot_both_be_applied()
        {
            var command = Create.Command("outer", "", new ArgumentDefinitionBuilder().ExactlyOne(),
                Create.Command("non-unique", "", new ArgumentDefinitionBuilder().ExactlyOne()),
                Create.Command("inner", "", new ArgumentDefinitionBuilder().ExactlyOne(),
                    Create.Command("non-unique", "", new ArgumentDefinitionBuilder().ExactlyOne())));

            CommandParseResult result = command.Parse("outer arg1 inner arg2 non-unique arg3 ");

            output.WriteLine(result.Diagram());

            result.Diagram().Should().Be("[ outer [ inner [ non-unique <arg3> ] <arg2> ] <arg1> ]");
        }

        [Fact]
        public void When_child_option_will_not_accept_arg_then_parent_can()
        {
            var parser = new CommandParser(Create.Command("the-command", "", new ArgumentDefinitionBuilder().ZeroOrMore(),
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: ArgumentDefinition.None)));

            CommandParseResult result = parser.Parse("the-command -x two");

            var theCommand = ParseResultExtensions.Command(result);
            theCommand["x"].Arguments.Should().BeEmpty();
            theCommand.Arguments.Should().BeEquivalentTo("two");
        }

        [Fact]
        public void When_parent_option_will_not_accept_arg_then_child_can()
        {
            var parser = new CommandParser(Create.Command("the-command", "",
                        ArgumentDefinition.None, new OptionDefinition(
                                                              "-x",
                                                              "",
                                                              argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())));

            CommandParseResult result = parser.Parse("the-command -x two");

            ParseResultExtensions.Command(result)["x"].Arguments.Should().BeEquivalentTo("two");
            ParseResultExtensions.Command(result).Arguments.Should().BeEmpty();
        }

        [Fact]
        public void When_the_same_option_is_defined_on_both_outer_and_inner_command_and_specified_at_the_end_then_it_attaches_to_the_inner_command()
        {
            var parser = new CommandParser(
                Create.Command("outer", "", ArgumentDefinition.None,
                    Create.Command("inner", "",
                        new OptionDefinition(
                            "-x",
                            "",
                            argumentDefinition: null)),
                    new OptionDefinition(
                        "-x",
                        "",
                        argumentDefinition: null)));

            CommandParseResult result = parser.Parse("outer inner -x");

            ParseResultExtensions.Command(result)
                .Parent
                .Children
                .Should()
                .NotContain(o => o.Name == "x");
            ParseResultExtensions.Command(result)
                .Children
                .Should()
                .ContainSingle(o => o.Name == "x");
        }

        [Fact]
        public void When_the_same_option_is_defined_on_both_outer_and_inner_command_and_specified_in_between_then_it_attaches_to_the_outer_command()
        {
            var parser = new CommandParser(
                Create.Command("outer", "",
                    Create.Command("inner", "",
                        new OptionDefinition(
                            "-x",
                            "",
                            argumentDefinition: null)),
                    new OptionDefinition(
                        "-x",
                        "",
                        argumentDefinition: null)));

            var result = parser.Parse("outer -x inner");

            ParseResultExtensions.Command(result)
                .Children
                .Should()
                .BeEmpty();
            ParseResultExtensions.Command(result)
                .Parent
                .Children
                .Should()
                .ContainSingle(o => o.Name == "x");
        }

        [Fact]
        public void Arguments_only_apply_to_the_nearest_command()
        {
            var command = Create.Command("outer", "", new ArgumentDefinitionBuilder().ExactlyOne(),
                Create.Command("inner", "", new ArgumentDefinitionBuilder().ExactlyOne()));

            CommandParseResult result = command.Parse("outer inner arg1 arg2");

            ParseResultExtensions.Command(result)
                  .Parent
                  .Arguments
                  .Should()
                  .BeEmpty();
            ParseResultExtensions.Command(result)
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
            var command = Create.Command("the-command", "",
                Create.Command("complete", "", new ArgumentDefinitionBuilder().ExactlyOne(),
                    new OptionDefinition(
                        "--position",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())));

            CommandParseResult result = command.Parse("the-command",
                                       "complete",
                                       "--position",
                                       "7",
                                       "the-command");

            Command complete = ParseResultExtensions.Command(result);

            output.WriteLine(result.Diagram());

            complete.Arguments.Should().BeEquivalentTo("the-command");
        }

        [Fact]
        public void A_root_command_can_be_omitted_from_the_parsed_args()
        {
            var command = Create.Command("outer", "",
                Create.Command("inner", "",
                    new OptionDefinition(
                        "-x",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())));

            var result1 = command.Parse("inner -x hello");
            var result2 = command.Parse("outer inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void A_root_command_can_match_a_full_path_to_an_executable()
        {
            var command = Create.Command("outer", "",
                Create.Command("inner", "",
                    new OptionDefinition(
                        "-x",
                        "",
                        argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())));

            CommandParseResult result1 = command.Parse("inner -x hello");

            var exePath = Path.Combine("dev", "outer.exe");
            CommandParseResult result2 = command.Parse($"{exePath} inner -x hello");

            result1.Diagram().Should().Be(result2.Diagram());
        }

        [Fact]
        public void Absolute_unix_style_paths_are_lexed_correctly()
        {
            var command =
                @"rm ""/temp/the file.txt""";

            var parser = new CommandParser(Create.Command("rm", "", new ArgumentDefinitionBuilder().ZeroOrMore()));

            var result = parser.Parse(command);

            result.Symbols["rm"]
                  .Arguments
                  .Should()
                  .OnlyContain(a => a == @"/temp/the file.txt");
        }

        [Fact]
        public void Absolute_Windows_style_paths_are_lexed_correctly()
        {
            var command =
                @"rm ""c:\temp\the file.txt\""";

            var parser = new CommandParser(Create.Command("rm", "", new ArgumentDefinitionBuilder().ZeroOrMore()));

            CommandParseResult result = parser.Parse(command);

            Console.WriteLine(result);

            result.Symbols["rm"]
                  .Arguments
                  .Should()
                  .OnlyContain(a => a == @"c:\temp\the file.txt\");
        }

        [Fact]
        public void When_a_default_argument_value_is_not_provided_then_the_default_value_can_be_accessed_from_the_parse_result()
        {
            var option = Create.Command("command", "", Define.Arguments()
                                     .WithDefaultValue(() => "default")
                                     .ExactlyOne(), Create.Command("subcommand", "",
                                         new ArgumentDefinitionBuilder().ExactlyOne()));

            CommandParseResult result = option.Parse("command subcommand subcommand-arg");

            output.WriteLine(result.Diagram());

            ParseResultExtensions.Command(result).Parent.Arguments.Should().BeEquivalentTo("default");
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_the_option_can_still_be_accessed_as_though_it_had_been_applied()
        {
            var command = Create.Command("command", "", new OptionDefinition(
                                             new[] {"-o", "--option"},
                                             "",
                                             argumentDefinition: Define.Arguments()
                                                                       .WithDefaultValue(() => "the-default")
                                                                       .ExactlyOne()));

            CommandParseResult result = command.Parse("command");

            result.HasOption("o").Should().BeTrue();
            result.HasOption("option").Should().BeTrue();
            ParseResultExtensions.Command(result).ValueForOption("o").Should().Be("the-default");
        }

        [Fact]
        public void When_an_option_with_a_default_value_is_not_matched_then_the_option_can_still_be_accessed_from_the_parse_result_as_though_it_had_been_applied()
        {
            var option = new OptionDefinition(
                new[] {"-o", "--option"},
                "",
                argumentDefinition: Define.Arguments().WithDefaultValue(() => "the-default").ExactlyOne());

            OptionParseResult result = option.Parse("");

            result.HasOption("o").Should().BeTrue();
            result.HasOption("option").Should().BeTrue();
            result["o"].GetValueOrDefault<string>().Should().Be("the-default");
        }

        [Fact]
        public void Unmatched_options_are_not_split_into_smaller_tokens()
        {
            var command = Create.Command("outer", "", ArgumentDefinition.None,
                new OptionDefinition(
                    "-p",
                    "",
                    argumentDefinition: null),
                Create.Command("inner", "", new ArgumentDefinitionBuilder().OneOrMore(),
                    new OptionDefinition(
                        "-o",
                        "",
                        argumentDefinition: ArgumentDefinition.None)));

            CommandParseResult result = command.Parse("outer inner -p:RandomThing=random");

            output.WriteLine(result.Diagram());

            ParseResultExtensions.Command(result)
                  .Arguments
                  .Should()
                  .BeEquivalentTo("-p:RandomThing=random");
        }

        [Fact]
        public void The_default_behavior_of_unmatched_tokens_resulting_in_errors_can_be_turned_off()
        {
            var command = Create.Command("the-command",
                                  "",
                                  treatUnmatchedTokensAsErrors: false,
                                  arguments: new ArgumentDefinitionBuilder().ExactlyOne());

            var parser = new OptionParser(
                new ParserConfiguration(
                    symbolDefinitions: new[] { command }));

            OptionParseResult result = parser.Parse("the-command arg1 arg2");

            result.Errors.Should().BeEmpty();

            result.UnmatchedTokens
                  .Should()
                  .BeEquivalentTo("arg2");
        }

        [Fact]
        public void Argument_names_can_collide_with_option_names()
        {
            var command = Create.Command("the-command", "",
                new OptionDefinition(
                    "--one",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()));

            CommandParseResult result = command.Parse("the-command --one one");

            ParseResultExtensions.Command(result)["one"]
                  .Arguments
                  .Should()
                  .BeEquivalentTo("one");
        }

        [Fact]
        public void Option_and_Command_can_have_the_same_alias()
        {
            var innerCommand = Create.Command("inner", "",
                                            new ArgumentDefinitionBuilder().ZeroOrMore());

            var option = new OptionDefinition(
                "--inner",
                "",
                argumentDefinition: null);

            var outerCommand = Create.Command("outer", "",
                                               new ArgumentDefinitionBuilder().ZeroOrMore(),
                                               innerCommand,
                                              option);

            var parser = new CommandParser(outerCommand);

            ParseResultExtensions.Command(parser.Parse("outer inner"))
                  .Definition
                  .Should()
                  .Be(innerCommand);

            ParseResultExtensions.Command(parser.Parse("outer --inner"))
                  .Definition
                  .Should()
                  .Be(outerCommand);

            ParseResultExtensions.Command(parser.Parse("outer --inner inner"))
                  .Definition
                  .Should()
                  .Be(innerCommand);

            ParseResultExtensions.Command(parser.Parse("outer --inner inner"))
                  .Parent
                  .Children
                  .Should()
                  .Contain(c => c.SymbolDefinition == option);
        }

        [Fact]
        public void Options_can_have_the_same_alias_differentiated_only_by_prefix()
        {
            var option1 = new OptionDefinition(new[] { "-a" }, "");
            var option2 = new OptionDefinition(new[] { "--a" }, "");

            var parser = new OptionParser(option1, option2);

            parser.Parse("-a")
                  .Symbols
                  .Select(s => s.SymbolDefinition)
                  .Should()
                  .BeEquivalentTo(option1);
            parser.Parse("--a")
                  .Symbols
                  .Select(s => s.SymbolDefinition)
                  .Should()
                  .BeEquivalentTo(option2);
        }

        [Fact]
        public void Empty_string_can_be_accepted_as_a_command_line_argument_when_enclosed_in_double_quotes()
        {
            OptionParseResult parseResult = new OptionParser(
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore()))
                .Parse("-x \"\"");

            parseResult["x"].Arguments
                            .ShouldBeEquivalentTo(new[] { "" });
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
            var option = new OptionDefinition(new[] { "output", "o" }, "");
            var configuration = new ParserConfiguration(
                new[] { option },
                prefixes: new[] { "-", "--", "/" });
            var parser = new OptionParser(configuration);

            OptionParseResult parseResult = parser.Parse(input);
            parseResult.Symbols["output"].Should().NotBeNull();
            parseResult.Symbols["o"].Should().NotBeNull();
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
            var option = new OptionDefinition(new[] { "--output", "-o", "/o", "out" }, "");
            var configuration = new ParserConfiguration(
                new[] { option },
                prefixes: new[] { "-", "--", "/" });
            var parser = new OptionParser(configuration);

            OptionParseResult parseResult = parser.Parse(input);
            parseResult.Symbols["output"].Should().NotBeNull();
            parseResult.Symbols["o"].Should().NotBeNull();
            parseResult.Symbols["out"].Should().NotBeNull();
        }
    }
}
