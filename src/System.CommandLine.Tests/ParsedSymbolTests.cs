// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using FluentAssertions;
using System.Linq;
using Xunit;
using static System.CommandLine.Create;

namespace System.CommandLine.Tests
{
    public class ParsedSymbolTests
    {
        [Fact]
        public void ParsedOption_with_exactly_one_argument_accepts_single_argument()
        {
            var builder = new ArgumentDefinitionBuilder();
            var option = Option("-x", "", builder.ExactlyOne());

            var applied = new Option(option, "-x");

            applied.TryTakeToken(new Token("some argument", TokenType.Argument))
                   .Should()
                   .NotBeNull();

            applied.Arguments
                   .Should()
                   .HaveCount(1);
        }

        [Fact]
        public void ParsedOption_with_exactly_one_argument_does_not_accept_two_arguments()
        {
            var builder = new ArgumentDefinitionBuilder();
            var option = Option("-x", "", builder.ExactlyOne());

            var applied = new Option(option, "-x");

            applied.TryTakeToken(new Token("argument1", TokenType.Argument));

            applied.Arguments
                   .Should()
                   .BeEquivalentTo("argument1");
        }

        [Fact]
        public void ParsedOption_with_specific_arguments_does_not_accept_argument_that_does_not_match()
        {
            var builder = new ArgumentDefinitionBuilder();
            var option = Option("-x", "", builder.FromAmong("one", "two", "three").ExactlyOne());

            var parsedOption = new Option(option, "-x");

            parsedOption.TryTakeToken(new Token("t", TokenType.Argument));

            parsedOption.Arguments.Should().BeEmpty();
        }

        [Fact]
        public void ParsedOption_with_no_arguments_does_not_accept_arguments()
        {
            var option = Option("-x", "", ArgumentDefinition.None);

            var parsedOption = new Option(option, "-x");

            parsedOption.TryTakeToken(new Token("argument1", TokenType.Argument));

            parsedOption.Arguments
                        .Should()
                        .HaveCount(0);
        }

        [Fact]
        public void ParsedCommand_can_have_nested_option_with_args()
        {
            var builder = new ArgumentDefinitionBuilder();
            var option = Command("outer", "",
                                 Option("inner", "",
                                        builder.ExactlyOne()));

            var parsedCommand = new Command(option);

            parsedCommand.TryTakeToken(new Token("inner", TokenType.Option));
            parsedCommand.TryTakeToken(new Token("argument1", TokenType.Argument));

            parsedCommand.Children
                         .Should()
                         .ContainSingle(o =>
                                            o.Name == "inner" &&
                                            o.Arguments.Single() == "argument1");
        }

        [Fact]
        public void ParsedCommand_can_have_multiple_nested_options_with_args()
        {
            var option = Command("outer", "",
                                 Option("inner1", "", new ArgumentDefinitionBuilder().ExactlyOne()),
                                 Option("inner2", "", new ArgumentDefinitionBuilder().ExactlyOne()));

            var parsedCommand = new Command(option);

            parsedCommand.TryTakeToken(new Token("inner1", TokenType.Option));
            parsedCommand.TryTakeToken(new Token("argument1", TokenType.Argument));
            parsedCommand.TryTakeToken(new Token("inner2", TokenType.Option));
            parsedCommand.TryTakeToken(new Token("argument2", TokenType.Argument));

            parsedCommand.Children
                         .Should()
                         .ContainSingle(o =>
                                            o.Name == "inner1" &&
                                            o.Arguments.Single() == "argument1");
            parsedCommand.Children
                         .Should()
                         .ContainSingle(o =>
                                            o.Name == "inner2" &&
                                            o.Arguments.Single() == "argument2");
        }

        [Fact]
        public void An_option_with_a_default_argument_value_is_valid_without_having_the_argument_supplied()
        {
            var option = Option("-x",
                                "",
                                Define.Arguments()
                                      .FromAmong("one", "two", "default")
                                      .WithDefaultValue(() => "default")
                                      .ExactlyOne());

            var parsedOption = new Option(option, "-x");

            parsedOption.Arguments.Should().BeEquivalentTo("default");
        }

        [Fact]
        public void An_option_with_a_default_argument_value_will_accept_a_different_value()
        {
            var option = Option("-x",
                                "",
                                Define.Arguments().FromAmong("one", "two", "default")
                                      .WithDefaultValue(defaultValue: () => "default")
                                      .ExactlyOne());

            var parsedOption = new Option(option, "-x");

            parsedOption.TryTakeToken(new Token("two", TokenType.Argument));

            parsedOption.Arguments.Should().BeEquivalentTo("two");
        }

        [Fact]
        public void Default_values_are_reevaluated_and_not_cached_between_parses()
        {
            var i = 0;
            var option =
                Option("-x",
                       "",
                       Define.Arguments()
                             .WithDefaultValue(() => (++i).ToString())
                             .ExactlyOne());

            var result1 = option.Parse("-x");
            var result2 = option.Parse("-x");

            result1["x"]
                .GetValueOrDefault()
                .Should()
                .Be("1");
            result2["x"]
                .GetValueOrDefault()
                .Should()
                .Be("2");
        }

        [Fact]
        public void ExactlyOne_error_message_can_be_customized()
        {
            var builder = new ArgumentDefinitionBuilder();
            var option =
                Command("the-command", "",
                        builder.ExactlyOne(o => $"Expected 1 arg for option `{o.Name}`, found none"));

            var result = option.Parse("the-command");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .BeEquivalentTo("Expected 1 arg for option `the-command`, found none");
        }

        [Fact]
        public void HasOption_can_be_used_to_check_the_presence_of_an_option()
        {
            var command = Command("the-command", "",
                                  Option("-h|--help", ""));

            var result = command.Parse("the-command -h");

            result.HasOption("help")
                  .Should()
                  .BeTrue();
        }

        [Fact]
        public void ParsedCommand_TryTakeToken_will_accept_the_next_command_in_the_tree()
        {
            var command = Command("one", "",
                                  Command("two", "",
                                          Command("three", "")));

            var parsedCommand = new Command(command);

            parsedCommand.TryTakeToken(new Token("two", TokenType.Command))
                         .Name
                         .Should()
                         .Be("two");
            parsedCommand.TryTakeToken(new Token("three", TokenType.Command))
                         .Name
                         .Should()
                         .Be("three");
        }

        [Fact]
        public void ParsedCommand_TryTakeToken_is_accepts_long_form_option()
        {
            var command = Command("command", "", Option("-o|--one", "", ArgumentDefinition.None));

            var parsedCommand = new Command(command);

            parsedCommand.TryTakeToken(new Token("--one", TokenType.Option))
                         .Name
                         .Should()
                         .Be("one");
        }

        [Fact]
        public void ParsedCommand_TryTakeToken_is_accepts_short_form_option()
        {
            var command = Command("command", "", Option("-o|--one", "", ArgumentDefinition.None));

            var parsedCommand = new Command(command);

            parsedCommand.TryTakeToken(new Token("-o", TokenType.Option))
                         .Name
                         .Should()
                         .Be("one");
        }

        [Fact]
        public void TryTakeToken_does_not_accept_incorrectly_prefixed_options()
        {
            var command = Command("command", "", Option("-o|--one", "", ArgumentDefinition.None));

            var parsedCommand = new Command(command);

            parsedCommand.TryTakeToken(new Token("--o", TokenType.Option))
                         .Should()
                         .BeNull();

            parsedCommand.TryTakeToken(new Token("-one", TokenType.Option))
                         .Should()
                         .BeNull();
        }

        [Fact]
        public void TakeToken_will_not_skip_a_level()
        {
            var command = Command("one", "",
                                  Command("two", "",
                                          Command("three", "")));

            var parsedCommand = new Command(command);

            parsedCommand.TryTakeToken(new Token("three", TokenType.Command))
                         .Should()
                         .BeNull();
        }

        [Fact]
        public void TakeToken_will_not_accept_a_command_if_a_sibling_command_has_already_been_accepted()
        {
            var command = Command("outer", "",
                                  Command("inner-one", ""),
                                  Command("inner-two", ""));

            var parsedCommand = new Command(command);

            parsedCommand.TryTakeToken(new Token("inner-one", TokenType.Command))
                         .Name
                         .Should()
                         .Be("inner-one");

            parsedCommand.TryTakeToken(new Token("inner-two", TokenType.Command))
                         .Should()
                         .BeNull();
        }

        [Fact]
        public void TryTakeToken_will_not_accept_an_argument_if_it_is_invalid()
        {
            var option = Option("--one", "", ArgumentDefinition.None);

            var parsedOption = new Option(option);

            parsedOption.TryTakeToken(new Token("arg", TokenType.Argument))
                        .Should()
                        .BeNull();
        }

        [Fact]
        public void Result_returns_single_string_default_value_when_no_argument_is_provided()
        {
            var option = Option("-x", "",
                                Define.Arguments()
                                      .WithDefaultValue(() => "default")
                                      .ExactlyOne());

            var parsed = new Option(option);

            parsed.Result
                  .Should()
                  .BeOfType<SuccessfulArgumentParseResult<string>>()
                  .Which
                  .Value
                  .Should()
                  .Be("default");
        }

        [Fact]
        public void Result_returns_IEnumerable_containing_string_default_value_when_no_argument_is_provided()
        {
            var option = Option("-x", "",
                                Define.Arguments()
                                      .WithDefaultValue(() => "default")
                                      .OneOrMore());

            var parsed = new Option(option);

            parsed.Result
                  .Should()
                  .BeOfType<SuccessfulArgumentParseResult<IReadOnlyCollection<string>>>()
                  .Which
                  .Value
                  .Should()
                  .BeEquivalentTo("default");
        }

    }
}
