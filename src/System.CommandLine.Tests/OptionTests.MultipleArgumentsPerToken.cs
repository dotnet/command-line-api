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
    public partial class OptionTests : SymbolTests
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
                    var animalsOption = new Option<string[]>(new[] { "-a", "--animals" })
                    {
                        AllowMultipleArgumentsPerToken = true,
                    };
                    var vegetablesOption = new Option<string>(new[] { "-v", "--vegetables" });

                    var command = new RootCommand
                    {
                        animalsOption,
                        vegetablesOption
                    };

                    var result = command.Parse("-a cat dog -v carrot");

                    result
                        .FindResultFor(animalsOption)
                        .Tokens
                        .Select(t => t.Value)
                        .Should()
                        .BeEquivalentTo(new[] { "cat", "dog" });

                    result
                        .FindResultFor(vegetablesOption)
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
                    var animalsOption = new Option<string>(new[] { "-a", "--animals" })
                    {
                        AllowMultipleArgumentsPerToken = true
                    };
                    var vegetablesOption = new Option<string[]>(new[] { "-v", "--vegetables" });

                    var command = new RootCommand
                    {
                        animalsOption,
                        vegetablesOption
                    };

                    var result = command.Parse("-a cat some-arg -v carrot");

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
                    var option = new Option<string>("--option")
                    {
                        AllowMultipleArgumentsPerToken = true
                    };
                    var command = new Command("the-command")
                    {
                        option,
                        new Argument<string>()
                    };

                    var result = command.Parse(commandLine);

                    var value = result.GetValueForOption(option);

                    value.Should().Be("2");
                }

                [Fact]
                public void All_consumed_tokens_are_present_in_option_result()
                {
                    var option = new Option<int>("-x")
                    {
                        AllowMultipleArgumentsPerToken = true
                    };

                    var result = option.Parse("-x 1 -x 2 -x 3 -x 4");

                    _output.WriteLine(result.Diagram());

                    var optionResult = result.FindResultFor(option);

                    optionResult
                        .Tokens
                        .Select(t => t.Value)
                        .Should().BeEquivalentSequenceTo("1", "2", "3", "4");
                }

                [Fact]
                public void Multiple_option_arguments_that_match_single_arity_option_aliases_are_parsed_correctly()
                {
                    var optionX = new Option<string>("-x")
                    {
                        AllowMultipleArgumentsPerToken = true
                    };
                    var optionY = new Option<string>("-y")
                    {
                        AllowMultipleArgumentsPerToken = true
                    };

                    var command = new RootCommand
                    {
                        optionX,
                        optionY
                    };

                    var result = command.Parse("-x -x -x -y -y -x -y -y -y -x -x -y");

                    _output.WriteLine(result.Diagram());

                    result.Errors.Should().BeEmpty();
                    result.GetValueForOption(optionY).Should().Be("-x");
                    result.GetValueForOption(optionX).Should().Be("-y");
                }
            }

            public class Disallowed
            {
                [Fact]
                public void Single_option_arg_is_matched()
                {
                    var option = new Option<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
                    var command = new Command("the-command") { option };

                    var result = command.Parse("--option 1 2");

                    var value = result.GetValueForOption(option);

                    value.Should().BeEquivalentTo(new[] { "1" });
                }

                [Fact]
                public void Subsequent_matched_arguments_result_in_errors()
                {
                    var option = new Option<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
                    var command = new Command("the-command") { option };

                    var result = command.Parse("--option 1 2");

                    result.UnmatchedTokens.Should().BeEquivalentTo(new[] { "2" });
                    result.Errors.Should().Contain(e => e.Message == LocalizationResources.Instance.UnrecognizedCommandOrArgument("2"));
                }

                [Fact]
                public void When_max_arity_is_greater_than_1_then_multiple_option_args_are_matched()
                {
                    var option = new Option<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
                    var command = new Command("the-command") { option };

                    var result = command.Parse("--option 1 --option 2");

                    var value = result.GetValueForOption(option);

                    value.Should().BeEquivalentTo(new[] { "1", "2" });
                    result.Errors.Should().BeEmpty();
                }
            }
        }
    }
}