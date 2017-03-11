// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using System.Linq;
using Xunit;
using static Microsoft.DotNet.Cli.CommandLine.Accept;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class AppliedOptionTests
    {
        [Fact]
        public void Applied_option_with_exactly_one_argument_accepts_single_argument()
        {
            var option = Option("-x", "", ExactlyOneArgument());

            var applied = new AppliedOption(option, "-x");

            var remainder = applied.TryTakeTokens("some argument");

            remainder.Should()
                     .BeEmpty();

            applied.Arguments
                   .Should()
                   .HaveCount(1);
        }

        [Fact]
        public void Applied_option_with_exactly_one_argument_does_not_accept_two_arguments()
        {
            var option = Option("-x", "", ExactlyOneArgument());

            var applied = new AppliedOption(option, "-x");

            var remainder = applied.TryTakeTokens("argument1", "argument2");

            remainder.Should()
                     .BeEquivalentTo("argument2");

            applied.Arguments
                   .Should()
                   .HaveCount(1);
        }

        [Fact]
        public void Applied_option_with_specific_arguments_does_not_accept_argument_that_does_not_match()
        {
            var option = Option("-x", "", AnyOneOf("one", "two", "three"));

            var applied = new AppliedOption(option, "-x");

            var remainder = applied.TryTakeTokens("t");

            remainder
                .Should()
                .BeEquivalentTo("t");
        }

        [Fact]
        public void Applied_option_with_no_arguments_does_not_accept_arguments()
        {
            var option = Option("-x", "", NoArguments());

            var applied = new AppliedOption(option, "-x");

            applied.TryTakeTokens("argument1");

            applied.Arguments
                   .Should()
                   .HaveCount(0);
        }

        [Fact]
        public void Applied_option_returns_empty_remainder_when_TryTakeTokens_is_called_with_empty_array()
        {
            var option = Option("-x", "", ZeroOrMoreArguments());

            var applied = new AppliedOption(option, "-x");

            var remainder = applied.TryTakeTokens(Array.Empty<string>());

            remainder.Should().BeEmpty();
        }

        [Fact]
        public void Applied_option_can_have_nested_option_with_args()
        {
            var option = Command("outer", "",
                                 Option("inner", "",
                                        ExactlyOneArgument()));

            var applied = new AppliedOption(option, "outer");

            applied.TryTakeTokens("inner", "argument1");

            applied.AppliedOptions
                   .Should()
                   .ContainSingle(o =>
                                      o.Name == "inner" &&
                                      o.Arguments.Single() == "argument1");
        }

        [Fact]
        public void Applied_option_can_have_multiple_nested_options_with_args()
        {
            var option = Command("outer", "",
                                 Option("inner1", "", ExactlyOneArgument()),
                                 Option("inner2", "", ExactlyOneArgument()));

            var applied = new AppliedOption(option, "outer");

            applied.TryTakeTokens("inner1", "argument1");
            applied.TryTakeTokens("inner2", "argument2");

            System.Console.WriteLine(applied.Diagram());

            applied.AppliedOptions
                   .Should()
                   .ContainSingle(o =>
                                      o.Name == "inner1" &&
                                      o.Arguments.Single() == "argument1");
            applied.AppliedOptions
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
                                AnyOneOf("one", "two", "default")
                                    .With(defaultValue: () => "default"));

            var applied = new AppliedOption(option, "-x");

            applied.Arguments.Should().BeEquivalentTo("default");
        }

        [Fact]
        public void An_option_with_a_default_argument_value_will_accept_a_different_value()
        {
            var option = Option("-x",
                                "",
                                AnyOneOf("one", "two", "default")
                                    .With(defaultValue: () => "default"));

            var applied = new AppliedOption(option, "-x");

            var remainder = applied.TryTakeTokens("two");

            remainder.Should().BeEmpty();
            applied.Arguments.Should().BeEquivalentTo("two");
        }

        [Fact]
        public void ExactlyOneArgument_error_message_can_be_customized()
        {
            var option =
                Command("the-command", "",
                        ExactlyOneArgument(
                            o => $"Expected 1 arg for option `{o.Name}`, found none"));

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

            result.AppliedOptions
                  .Single()
                  .HasOption("help")
                  .Should()
                  .BeTrue();
        }
    }
}