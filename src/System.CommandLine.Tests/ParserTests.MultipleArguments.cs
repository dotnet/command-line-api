// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Tests.Utility;
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

                result.ValueForArgument(multipleArityArg)
                    .Should()
                    .BeEquivalentSequenceTo("1", "2", "3");
                result.ValueForArgument(singleArityArg)
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

                result.ValueForArgument(stringArg).Should().Be("1");
                result.ValueForArgument(intArg).Should().Be(2);
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
                    .ValueForArgument(first)
                    .Should()
                    .Be("one");

                parseResult
                    .ValueForArgument(second)
                    .Should()
                    .Be("two");

                parseResult
                    .ValueForArgument(third)
                    .Should()
                    .BeEquivalentSequenceTo("three", "four", "five");

                parseResult
                    .ValueForOption(verbose)
                    .Should()
                    .BeTrue();
            }

            [Fact]
            public void Multiple_arguments_of_unspecified_type_are_parsed_correctly()
            {
                var sourceArg = new Argument("source")
                {
                    Arity = ArgumentArity.ExactlyOne
                };
                var destinationArg = new Argument("destination")
                {
                    Arity = ArgumentArity.ExactlyOne
                };
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
                var command = new Command("the-command") { option };

                command.AddArgument(new Argument<string>
                {
                    Name = "arg1",
                });

                command.AddArgument(new Argument<string>
                {
                    Name = "arg2",
                });

                var result = command.Parse("-e foo");

                var optionResult = result.ValueForOption(option);

                optionResult.Should().Be("foo");
            }


            [Fact]
            public void tokens_that_cannot_be_converted_by_multiple_arity_argument_flow_to_next_multiple_arity_argument()
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

                result.ValueForArgument(ints)
                      .Should()
                      .BeEquivalentTo(new[] { 1, 2, 3 },
                                      options => options.WithStrictOrdering());

                result.ValueForArgument(strings)
                      .Should()
                      .BeEquivalentTo(new[] { "one", "two" },
                                      options => options.WithStrictOrdering());
            }

            [Fact]
            public void tokens_that_cannot_be_converted_by_multiple_arity_argument_flow_to_next_single_arity_argument()
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

                result.ValueForArgument(ints)
                      .Should()
                      .BeEquivalentTo(new[] { 1, 2, 3 },
                                      options => options.WithStrictOrdering());

                result.ValueForArgument(strings)
                      .Should()
                      .Be("four");

                result.UnparsedTokens.Should()
                      .ContainSingle()
                      .Which
                      .Should()
                      .Be("five");
            }

            [Fact(Skip = "https://github.com/dotnet/command-line-api/issues/1143")]
            public void tokens_that_cannot_be_converted_by_multiple_arity_option_flow_to_next_single_arity_argument()
            {
                var option = new Option<int[]>("-i");
                var argument = new Argument<string>("arg");

                var command = new RootCommand
                {
                    option,
                    argument
                };

                var result = command.Parse("-i 1 2 3 four");

                result.FindResultFor(option)
                      .GetValueOrDefault()
                      .Should()
                      .BeEquivalentTo(new[] { 1, 2, 3 }, options => options.WithStrictOrdering());

                result.FindResultFor(argument)
                      .Should()
                      .Be("four");
            }
        }
    }
}
