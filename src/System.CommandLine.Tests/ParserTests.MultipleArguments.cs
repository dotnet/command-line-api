// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
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
                var command = new Command("the-command")
                {
                    new Argument<string>
                    {
                        Arity = new ArgumentArity(3, 3),
                        Name = "several"
                    },
                    new Argument<string>
                    {
                        Arity = ArgumentArity.ZeroOrMore,
                        Name = "one"
                    }
                };

                var result = command.Parse("1 2 3 4");

                var several = result.ValueForArgument<IEnumerable<string>>("several");

                var one = result.ValueForArgument<IEnumerable<string>>("one");

                several.Should()
                       .BeEquivalentSequenceTo("1", "2", "3");
                one.Should()
                       .BeEquivalentSequenceTo("4");
            }

            [Fact]
            public void Multiple_arguments_can_differ_by_type()
            {
                var command = new Command("the-command")
                {
                    new Argument<string>
                    {
                        Name = "the-string"
                    },
                    new Argument<int>
                    {
                        Name = "the-int"
                    }
                };

                var result = command.Parse("1 2");

                var theString = result.ValueForArgument<string>("the-string");

                var theInt = result.ValueForArgument<int>("the-int");

                theString.Should().Be("1");
                theInt.Should().Be(2);
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
                var command = new Command("the-command")
                {
                    new Argument<string> { Name = "first" },
                    new Argument<string> { Name = "second" },
                    new Argument<string[]> { Name = "third" },
                    new Option<bool>("--verbose")
                };

                var parseResult = command.Parse(commandLine);

                parseResult
                    .ValueForArgument("first")
                    .Should()
                    .Be("one");

                parseResult
                    .ValueForArgument<string>("second")
                    .Should()
                    .Be("two");

                parseResult
                    .ValueForArgument<string[]>("third")
                    .Should()
                    .BeEquivalentSequenceTo("three", "four", "five");

                parseResult
                    .ValueForOption<bool>("--verbose")
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
