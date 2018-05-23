// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class SymbolTests
    {
        [Fact]
        public void ParsedOption_with_exactly_one_argument_accepts_single_argument()
        {
            var builder = new ArgumentDefinitionBuilder();
            var optionDefinition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: builder.ExactlyOne());

            var applied = new Option(optionDefinition, "-x");

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
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: builder.ExactlyOne());

            var applied = new Option(definition, "-x");

            applied.TryTakeToken(new Token("argument1", TokenType.Argument));

            applied.Arguments
                   .Should()
                   .BeEquivalentTo("argument1");
        }

        [Fact]
        public void ParsedOption_with_specific_arguments_does_not_accept_argument_that_does_not_match()
        {
            var builder = new ArgumentDefinitionBuilder();
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: builder.FromAmong("one", "two", "three").ExactlyOne());

            var option = new Option(definition, "-x");

            option.TryTakeToken(new Token("t", TokenType.Argument));

            option.Arguments.Should().BeEmpty();
        }

        [Fact]
        public void ParsedOption_with_no_arguments_does_not_accept_arguments()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: ArgumentDefinition.None);

            var option = new Option(definition, "-x");

            option.TryTakeToken(new Token("argument1", TokenType.Argument));

            option.Arguments.Should().HaveCount(0);
        }

        [Fact]
        public void ParsedCommand_can_have_nested_option_with_args()
        {
            var builder = new ArgumentDefinitionBuilder();
            var definition = new CommandDefinition("outer", "", new[] {
                new OptionDefinition(
                    "inner",
                    "",
                    argumentDefinition: builder.ExactlyOne())
            });

            var command = new Command(definition);

            command.TryTakeToken(new Token("inner", TokenType.Option));
            command.TryTakeToken(new Token("argument1", TokenType.Argument));

            command.Children.Should().ContainSingle(o => o.Name == "inner"
                                                      && o.Arguments.Single() == "argument1");
        }

        [Fact]
        public void Command_can_have_multiple_nested_options_with_args()
        {
            var definition = new CommandDefinition("outer", "", new[] {
                new OptionDefinition(
                    "inner1",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne()), new OptionDefinition(
                    "inner2",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())
            });

            var command = new Command(definition);

            command.TryTakeToken(new Token("inner1", TokenType.Option));
            command.TryTakeToken(new Token("argument1", TokenType.Argument));
            command.TryTakeToken(new Token("inner2", TokenType.Option));
            command.TryTakeToken(new Token("argument2", TokenType.Argument));

            command.Children.Should().ContainSingle(o => o.Name == "inner1"
                                                      && o.Arguments.Single() == "argument1");

            command.Children.Should().ContainSingle(o => o.Name == "inner2"
                                                      && o.Arguments.Single() == "argument2");
        }

        [Fact]
        public void An_option_with_a_default_argument_value_is_valid_without_having_the_argument_supplied()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder()
                                          .FromAmong("one", "two", "default")
                                          .WithDefaultValue(() => "default")
                                          .ExactlyOne());

            var option = new Option(definition, "-x");

            option.Arguments.Should().BeEquivalentTo("default");
        }

        [Fact]
        public void An_option_with_a_default_argument_value_will_accept_a_different_value()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().FromAmong("one", "two", "default")
                                          .WithDefaultValue(defaultValue: () => "default")
                                          .ExactlyOne());

            var option = new Option(definition, "-x");

            option.TryTakeToken(new Token("two", TokenType.Argument));

            option.Arguments.Should().BeEquivalentTo("two");
        }

        [Fact]
        public void Default_values_are_reevaluated_and_not_cached_between_parses()
        {
            var i = 0;
            var definition =
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder()
                                              .WithDefaultValue(() => (++i).ToString())
                                              .ExactlyOne());

            var result1 = definition.Parse("-x");
            var result2 = definition.Parse("-x");

            result1["x"].GetValueOrDefault().Should().Be("1");
            result2["x"].GetValueOrDefault().Should().Be("2");
        }

        [Fact]
        public void ExactlyOne_error_message_can_be_customized()
        {
            var builder = new ArgumentDefinitionBuilder();
            var definition =
                new CommandDefinition("the-command", "", symbolDefinitions: null, argumentDefinition: builder.ExactlyOne(o => $"Expected 1 arg for option `{o.Name}`, found none"));

            var result = definition.Parse("the-command");

            result.Errors.Select(e => e.Message)
                  .Should().BeEquivalentTo("Expected 1 arg for option `the-command`, found none");
        }

        [Fact]
        public void HasOption_can_be_used_to_check_the_presence_of_an_option()
        {
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    new[] {"-h", "--help"},
                    "",
                    argumentDefinition: null)
            });

            var result = definition.Parse("the-command -h");

            result.HasOption("help").Should().BeTrue();
        }

        [Fact]
        public void Command_TryTakeToken_will_accept_the_next_command_in_the_tree()
        {
            var definition = new CommandDefinition("one", "", new[] { new CommandDefinition("two", "", new[] { new CommandDefinition("three", "", ArgumentDefinition.None) }) });

            var command = new Command(definition);

            command.TryTakeToken(new Token("two", TokenType.Command)).Name
                .Should().Be("two");

            command.TryTakeToken(new Token("three", TokenType.Command)).Name
                .Should().Be("three");
        }

        [Fact]
        public void Command_TryTakeToken_is_accepts_long_form_option()
        {
            var definition = new CommandDefinition("command", "", new[] {
                new OptionDefinition(
                    new[] {"-o", "--one"},
                    "",
                    argumentDefinition: ArgumentDefinition.None)
            });

            var command = new Command(definition);

            command.TryTakeToken(new Token("--one", TokenType.Option)).Name
                .Should().Be("one");
        }

        [Fact]
        public void Command_TryTakeToken_is_accepts_short_form_option()
        {
            var definition = new CommandDefinition("command", "", new[] {
                new OptionDefinition(
                    new[] {"-o", "--one"},
                    "",
                    argumentDefinition: ArgumentDefinition.None)
            });

            var command = new Command(definition);

            command.TryTakeToken(new Token("-o", TokenType.Option)).Name
                .Should().Be("one");
        }

        [Fact]
        public void TryTakeToken_does_not_accept_incorrectly_prefixed_options()
        {
            var definition = new CommandDefinition("command", "", new[] {
                new OptionDefinition(
                    new[] {"-o", "--one"},
                    "",
                    argumentDefinition: ArgumentDefinition.None)
            });

            var command = new Command(definition);

            command.TryTakeToken(new Token("--o", TokenType.Option))
                .Should().BeNull();

            command.TryTakeToken(new Token("-one", TokenType.Option))
                .Should().BeNull();
        }

        [Fact]
        public void TakeToken_will_not_skip_a_level()
        {
            var definition = new CommandDefinition("one", "", new[] {
                new CommandDefinition("two", "", new[] {
                    new CommandDefinition("three", "", ArgumentDefinition.None)
                })
            });

            var command = new Command(definition);

            command.TryTakeToken(new Token("three", TokenType.Command))
                .Should().BeNull();
        }

        [Fact]
        public void TakeToken_will_not_accept_a_command_if_a_sibling_command_has_already_been_accepted()
        {
            var definition = new CommandDefinition("outer", "", new[] { new CommandDefinition("inner-one", "", ArgumentDefinition.None), new CommandDefinition("inner-two", "", ArgumentDefinition.None) });

            var command = new Command(definition);

            command.TryTakeToken(new Token("inner-one", TokenType.Command)).Name
                .Should().Be("inner-one");

            command.TryTakeToken(new Token("inner-two", TokenType.Command))
                .Should().BeNull();
        }

        [Fact]
        public void TryTakeToken_will_not_accept_an_argument_if_it_is_invalid()
        {
            var definition = new OptionDefinition(
                "--one",
                "",
                argumentDefinition: ArgumentDefinition.None);

            var option = new Option(definition);

            option.TryTakeToken(new Token("arg", TokenType.Argument))
                .Should().BeNull();
        }

        [Fact]
        public void Result_returns_single_string_default_value_when_no_argument_is_provided()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder()
                                          .WithDefaultValue(() => "default")
                                          .ExactlyOne());

            var option = new Option(definition);

            option.Result.Should().BeOfType<SuccessfulArgumentParseResult<string>>()
                .Which.Value.Should().Be("default");
        }

        [Fact]
        public void Result_returns_IEnumerable_containing_string_default_value_when_no_argument_is_provided()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder()
                                          .WithDefaultValue(() => "default")
                                          .OneOrMore());

            var option = new Option(definition);

            option.Result.Should().BeOfType<SuccessfulArgumentParseResult<IReadOnlyCollection<string>>>()
                .Which.Value.Should().BeEquivalentTo("default");
        }
    }
}
