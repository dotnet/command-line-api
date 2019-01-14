// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class SymbolResultTests
    {
        [Fact]
        public void An_option_with_a_default_value_and_no_explicitly_provided_argument_has_an_empty_arguments_property()
        {
            var option = new Option(
                "-x",
                "",
                new Argument<string>("default"));

            var result = new OptionResult(option, "-x");

            result.Arguments.Should().BeEmpty();
        }

        [Fact]
        public void Default_values_are_reevaluated_and_not_cached_between_parses()
        {
            var option =
                new Option(
                    "-x",
                    "",
                    new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    });

            var i = 0;
            option.Argument.SetDefaultValue(() => (++i).ToString());

            var result1 = option.Parse("");
            var result2 = option.Parse("");

            result1["x"].GetValueOrDefault().Should().Be("1");
            result2["x"].GetValueOrDefault().Should().Be("2");
        }

        [Fact]
        public void HasOption_can_be_used_to_check_the_presence_of_an_option()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    new[] { "-h", "--help" }, "")
            });

            var result = command.Parse("the-command -h");

            result.HasOption("help").Should().BeTrue();
        }

        [Fact]
        public void HasOption_can_be_used_to_check_the_presence_of_an_implicit_option()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    new[] { "-c", "--count" }, "",
                    new Argument<int>(5) { Name = "count" })
            });

            var result = command.Parse("the-command");

            result.HasOption("count").Should().BeTrue();
        }

        [Fact]
        public void Command_will_not_accept_a_command_if_a_sibling_command_has_already_been_accepted()
        {
            var command = new Command(
                "outer", "",
                new[]
                {
                    new Command("inner-one", "",
                                argument: new Argument
                                {
                                    Arity = ArgumentArity.Zero
                                }),
                    new Command("inner-two", "",
                                argument: new Argument
                                {
                                    Arity = ArgumentArity.Zero
                                })
                });

            var result = new Parser(command).Parse("outer inner-one inner-two");

            result.CommandResult.Name.Should().Be("inner-one");
            result.Errors.Count.Should().Be(1);

            var result2 = new Parser(command).Parse("outer inner-two inner-one");

            result2.CommandResult.Name.Should().Be("inner-two");
            result2.Errors.Count.Should().Be(1);
        }

        [Fact]
        public void ValueForOption_throws_with_empty_alias()
        {
            var command = new Command("one", "");

            var result = new CommandResult(command);

            Action action = () => {
                result.ValueForOption<string>(string.Empty);
            };

            action.Should().Throw<ArgumentException>("Value cannot be null or whitespace.");
        }

        [Fact]
        public void HasAlias_returns_true_if_option_has_an_alias()
        {
            var result = new OptionResult(new Option(new[] { "o", "one" }, ""));

            result.HasAlias("one").Should().BeTrue();
        }
    }
}
