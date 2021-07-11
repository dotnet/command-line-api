// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class OptionTests : SymbolTests
    {
        public class MultipleArgumentsPerToken
        {
            public class Allowed
            {
                [Fact]
                public void When_option_is_not_respecified_but_limit_is_not_reached_then_the_following_token_is_used_as_value()
                {
                    var animalsOption = new Option(new[] { "-a", "--animals" })
                    {
                        AllowMultipleArgumentsPerToken = true,
                        Arity = ArgumentArity.ZeroOrMore
                    };
                    var vegetablesOption = new Option(new[] { "-v", "--vegetables" }) { Arity = ArgumentArity.ZeroOrMore };

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
                    var animalsOption = new Option(new[] { "-a", "--animals" })
                    {
                        AllowMultipleArgumentsPerToken = true,
                        Arity = ArgumentArity.ZeroOrOne
                    };
                    var vegetablesOption = new Option(new[] { "-v", "--vegetables" }) { Arity = ArgumentArity.ZeroOrMore };

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
            }

            public class Disallowed
            {
                [Fact]
                public void Single_option_arg_is_matched()
                {
                    var option = new Option<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
                    var command = new Command("the-command") { option };

                    var result = command.Parse("--option 1 2");

                    var value = result.ValueForOption(option);

                    value.Should().BeEquivalentTo(new[] { "1" });
                    result.UnmatchedTokens.Should().BeEquivalentTo(new[] { "2" });
                }

                [Fact]
                public void When_max_arity_is_greater_than_1_then_multiple_option_args_are_matched()
                {
                    var option = new Option<string[]>("--option") { AllowMultipleArgumentsPerToken = false };
                    var command = new Command("the-command") { option };

                    var result = command.Parse("--option 1 --option 2");

                    var value = result.ValueForOption(option);

                    value.Should().BeEquivalentTo(new[] { "1", "2" });
                }

                [Theory]
                [InlineData("--option 1 --option 2")]
                [InlineData("xyz --option 1 --option 2")]
                [InlineData("--option 1 xyz --option 2")]
                public void When_max_arity_is_1_then_subsequent_option_args_overwrite_its_value(string commandLine)
                {
                    var option = new Option<string>("--option") { AllowMultipleArgumentsPerToken = false };
                    var command = new Command("the-command") { 
                        option, 
                        new Argument<string>() 
                    };

                    var result = command.Parse(commandLine);

                    var value = result.ValueForOption(option);

                    value.Should().Be("2");
                }
            }
        }
    }
}