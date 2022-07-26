// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.CommandLine.Tests.Utility;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        public class MultipleArguments
        {
            [Fact]
            public void Multiple_arguments_can_differ_by_arity()
            {
                var multipleArityArg = new Argument<IEnumerable<string>>
                {
                    Arity = new ArgumentArity(3, 3),
                    Name = "several"
                };

                var singleArityArg = new Argument<IEnumerable<string>>
                {
                    Arity = ArgumentArity.ZeroOrMore,
                    Name = "one"
                };

                var command = new Command("the-command")
                {
                    multipleArityArg,
                    singleArityArg
                };

                var result = command.Parse("1 2 3 4");

                result.GetValueForArgument(multipleArityArg)
                      .Should()
                      .BeEquivalentSequenceTo("1", "2", "3");
                result.GetValueForArgument(singleArityArg)
                      .Should()
                      .BeEquivalentSequenceTo("4");
            }

            [Fact]
            public void Multiple_arguments_can_differ_by_type()
            {
                var stringArg = new Argument<string>
                {
                    Name = "the-string"
                };
                var intArg = new Argument<int>
                {
                    Name = "the-int"
                };

                var command = new Command("the-command")
                {
                    stringArg,
                    intArg
                };

                var result = command.Parse("1 2");

                result.GetValueForArgument(stringArg).Should().Be("1");
                result.GetValueForArgument(intArg).Should().Be(2);
            }

            [Theory]
            [InlineData("--verbose one two three four five")]
            [InlineData("one --verbose two three four five")]
            [InlineData("one two --verbose three four five")]
            [InlineData("one two three --verbose four five")]
            [InlineData("one two three four --verbose five")]
            [InlineData("one two three four five --verbose")]
            [InlineData("--verbose true one two three four five")]
            [InlineData("one --verbose true two three four five")]
            [InlineData("one two --verbose true three four five")]
            [InlineData("one two three --verbose true four five")]
            [InlineData("one two three four --verbose true five")]
            [InlineData("one two three four five --verbose true")]
            public void When_multiple_arguments_are_present_then_their_order_relative_to_sibling_options_is_not_significant(string commandLine)
            {
                var first = new Argument<string> { Name = "first" };
                var second = new Argument<string> { Name = "second" };
                var third = new Argument<string[]> { Name = "third" };
                var verbose = new Option<bool>("--verbose");

                var command = new Command("the-command")
                {
                    first,
                    second,
                    third,
                    verbose
                };

                var parseResult = command.Parse(commandLine);

                parseResult
                    .GetValueForArgument(first)
                    .Should()
                    .Be("one");

                parseResult
                    .GetValueForArgument(second)
                    .Should()
                    .Be("two");

                parseResult
                    .GetValueForArgument(third)
                    .Should()
                    .BeEquivalentSequenceTo("three", "four", "five");

                parseResult
                    .GetValueForOption(verbose)
                    .Should()
                    .BeTrue();
            }

            [Fact]
            public void Multiple_arguments_of_unspecified_type_are_parsed_correctly()
            {
                var sourceArg = new Argument<string>("source");
                var destinationArg = new Argument<string>("destination");
                var root = new RootCommand
                {
                    sourceArg,
                    destinationArg
                };

                var result = root.Parse("src.txt dest.txt");

                result.FindResultFor(sourceArg)
                      .GetValueOrDefault()
                      .Should()
                      .Be("src.txt");
                
                result.FindResultFor(destinationArg)
                      .GetValueOrDefault()
                      .Should()
                      .Be("dest.txt");
            }


            [Fact]
            public void When_multiple_arguments_are_defined_but_not_provided_then_option_parses_correctly()
            {
                var option = new Option<string>("-e");
                var command = new Command("the-command")
                {
                    option,
                    new Argument<string>(),
                    new Argument<string>()
                };

                var result = command.Parse("-e foo");

                var optionResult = result.GetValueForOption(option);

                optionResult.Should().Be("foo");
            }

            [Fact]
            public void Tokens_that_cannot_be_converted_by_multiple_arity_argument_flow_to_next_multiple_arity_argument()
            {
                var ints = new Argument<int[]>();
                var strings = new Argument<string[]>();

                var root = new RootCommand
                {
                    ints,
                    strings
                };

                var result = root.Parse("1 2 3 one two");

                var _ = new AssertionScope();

                result.GetValueForArgument(ints)
                      .Should()
                      .BeEquivalentTo(new[] { 1, 2, 3 },
                                      options => options.WithStrictOrdering());

                result.GetValueForArgument(strings)
                      .Should()
                      .BeEquivalentTo(new[] { "one", "two" },
                                      options => options.WithStrictOrdering());
            }

            [Fact]
            public void Tokens_that_cannot_be_converted_by_multiple_arity_argument_flow_to_next_single_arity_argument()
            {
                var ints = new Argument<int[]>();
                var strings = new Argument<string>();

                var root = new RootCommand
                {
                    ints,
                    strings
                };

                var result = root.Parse("1 2 3 four five");

                var _ = new AssertionScope();

                result.GetValueForArgument(ints)
                      .Should()
                      .BeEquivalentTo(new[] { 1, 2, 3 },
                                      options => options.WithStrictOrdering());

                result.GetValueForArgument(strings)
                      .Should()
                      .Be("four");

                result.UnmatchedTokens
                      .Should()
                      .ContainSingle()
                      .Which
                      .Should()
                      .Be("five");
            }

            [Fact]
            public void Unsatisfied_subsequent_argument_with_min_arity_0_parses_as_default_value()
            {
                var arg1 = new Argument<string>
                {
                    Arity = ArgumentArity.ExactlyOne
                };
                var arg2 = new Argument<string>
                {
                    Arity = ArgumentArity.ZeroOrOne,
                };
                arg2.SetDefaultValue("the-default");
                var rootCommand = new RootCommand
                {
                    arg1,
                    arg2,
                };

                var result = rootCommand.Parse("value-1");

                result.GetValueForArgument(arg1).Should().Be("value-1");
                result.GetValueForArgument(arg2).Should().Be("the-default");
            }

            [Fact] // https://github.com/dotnet/command-line-api/issues/1403
            public void Unsatisfied_subsequent_argument_with_min_arity_1_parses_as_default_value()
            {
                Argument<string> arg1 = new(name: "arg1");
                Argument<string> arg2 = new(name: "arg2", getDefaultValue: () => "the-default");

                var rootCommand = new RootCommand
                {
                    arg1,
                    arg2,
                };

                var result = rootCommand.Parse("");

                result.FindResultFor(arg1).Should().BeNull();
                result.GetValueForArgument(arg2).Should().Be("the-default");
            }

            [Fact] // https://github.com/dotnet/command-line-api/issues/1395
            public void When_subsequent_argument_with_ZeroOrOne_arity_is_not_provided_then_parse_is_correct()
            {
                var argument1 = new Argument<string>();
                var rootCommand = new RootCommand
                {
                    argument1,
                    new Argument<string>
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    },
                };

                var result = rootCommand.Parse("one");

                result.Errors.Should().BeEmpty();

                result.GetValueForArgument(argument1).Should().Be("one");
            }

            [Theory] // https://github.com/dotnet/command-line-api/issues/1711
            [InlineData("")]
            [InlineData("a")]
            [InlineData("a b")]
            [InlineData("a b c")]
            public void When_there_are_not_enough_tokens_for_all_arguments_then_the_correct_number_of_errors_is_reported(
                string providedArgs)
            {
                var command = new Command("command")
                {
                    new Argument<string>(),
                    new Argument<string>(),
                    new Argument<string>(),
                    new Argument<string>()
                };

                var result = new Parser(command).Parse(providedArgs);

                var numberOfMissingArgs =
                    result
                        .Errors
                        .Count(e => e.Message == LocalizationResources.Instance.RequiredArgumentMissing(result.CommandResult));

                numberOfMissingArgs
                    .Should()
                    .Be(4 - providedArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length);
            }
        }
    }
}
