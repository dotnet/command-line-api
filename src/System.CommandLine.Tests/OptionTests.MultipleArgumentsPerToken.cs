// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public partial class OptionTests
    {
        public class MultipleArgumentsPerToken
        {
            public class Allowed
            {
                private readonly ITestOutputHelper _output;

                public Allowed(ITestOutputHelper output)
                {
                    _output = output;
                }

                [Fact]
                public void When_option_is_not_respecified_but_limit_is_not_reached_then_the_following_token_is_used_as_value()
                {
                    var animalsOption = new CliOption<string[]>("-a", "--animals")
                    {
                        AllowMultipleArgumentsPerToken = true,
                    };
                    var vegetablesOption = new CliOption<string>("-v", "--vegetables");

                    var command = new CliRootCommand
                    {
                        animalsOption,
                        vegetablesOption
                    };

                    var result = command.Parse("-a cat dog -v carrot");

                    result
                        .GetResult(animalsOption)
                        .Tokens
                        .Select(t => t.Value)
                        .Should()
                        .BeEquivalentTo(new[] { "cat", "dog" });

                    result
                        .GetResult(vegetablesOption)
                        .Tokens
                        .Select(t => t.Value)
                        .Should()
                        .BeEquivalentTo("carrot");

                    result
                        .UnmatchedTokens
                        .Should()
                        .BeNullOrEmpty();
                }

                [Fact]
                public void When_option_is_not_respecified_and_limit_is_reached_then_the_following_token_is_unmatched()
                {
                    var animalsOption = new CliOption<string>("-a", "--animals")
                    {
                        AllowMultipleArgumentsPerToken = true
                    };
                    var vegetablesOption = new CliOption<string[]>("-v", "--vegetables");

                    var command = new CliRootCommand
                    {
                        animalsOption,
                        vegetablesOption
                    };

                    var result = command.Parse("-a cat some-arg -v carrot");

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

                    result
                        .UnmatchedTokens
                        .Should()
                        .BeEquivalentTo("some-arg");
                }

                [Theory]
                [InlineData("--option 1 --option 2")]
                [InlineData("xyz --option 1 --option 2")]
                [InlineData("--option 1 xyz --option 2")]
                [InlineData("--option 1 --option 2 xyz")]
                public void When_max_arity_is_1_then_subsequent_option_args_overwrite_previous_ones(string commandLine)
                {
                    var option = new CliOption<string>("--option")
                    {
                        AllowMultipleArgumentsPerToken = true
                    };
                    var command = new CliCommand("the-command")
                    {
                        option,
                        new CliArgument<string>("arg")
                    };

                    var result = command.Parse(commandLine);

                    var value = result.GetValue(option);

                    value.Should().Be("2");
                }

                [Fact]
                public void All_consumed_tokens_are_present_in_option_result()
                {
                    var option = new CliOption<int>("-x")
                    {
                        AllowMultipleArgumentsPerToken = true
                    };

                    var result = new CliRootCommand { option }.Parse("-x 1 -x 2 -x 3 -x 4");

                    _output.WriteLine(result.Diagram());

                    var optionResult = result.GetResult(option);

                    optionResult
                        .Tokens
                        .Select(t => t.Value)
                        .Should().BeEquivalentSequenceTo("1", "2", "3", "4");
                }

                [Fact]
                public void Multiple_option_arguments_that_match_single_arity_option_aliases_are_parsed_correctly()
                {
                    var optionX = new CliOption<string>("-x")
                    {
                        AllowMultipleArgumentsPerToken = true
                    };
                    var optionY = new CliOption<string>("-y")
                    {
                        AllowMultipleArgumentsPerToken = true
                    };

                    var command = new CliRootCommand
                    {
                        optionX,
                        optionY
                    };

                    var result = command.Parse("-x -x -x -y -y -x -y -y -y -x -x -y");

                    _output.WriteLine(result.Diagram());

                    result.Errors.Should().BeEmpty();
                    result.GetValue(optionY).Should().Be("-x");
                    result.GetValue(optionX).Should().Be("-y");
                }
            }

            public class Disallowed
            {
                [Fact]
                public void Single_option_arg_is_matched()
                {
                    var option = new CliOption<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
                    var command = new CliCommand("the-command") { option };

                    var result = command.Parse("--option 1 2");

                    var value = result.GetValue(option);

                    value.Should().BeEquivalentTo(new[] { "1" });
                }

                [Fact]
                public void Subsequent_matched_arguments_result_in_errors()
                {
                    var option = new CliOption<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
                    var command = new CliCommand("the-command") { option };

                    var result = command.Parse("--option 1 2");

                    result.UnmatchedTokens.Should().BeEquivalentTo(new[] { "2" });
                    result.Errors.Should().Contain(e => e.Message == LocalizationResources.UnrecognizedCommandOrArgument("2"));
                }

                [Fact]
                public void When_max_arity_is_greater_than_1_then_multiple_option_args_are_matched()
                {
                    var option = new CliOption<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
                    var command = new CliCommand("the-command") { option };

                    var result = command.Parse("--option 1 --option 2");

                    var value = result.GetValue(option);

                    value.Should().BeEquivalentTo(new[] { "1", "2" });
                    result.Errors.Should().BeEmpty();
                }
            }
        }
    }
}