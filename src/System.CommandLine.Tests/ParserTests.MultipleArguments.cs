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
                var multipleArityArg = new CliArgument<IEnumerable<string>>("several")
                {
                    Arity = new ArgumentArity(3, 3),
                };

                var singleArityArg = new CliArgument<IEnumerable<string>>("one")
                {
                    Arity = ArgumentArity.ZeroOrMore,
                };

                var command = new CliCommand("the-command")
                {
                    multipleArityArg,
                    singleArityArg
                };

                var result = command.Parse("1 2 3 4");

                result.GetValue(multipleArityArg)
                      .Should()
                      .BeEquivalentSequenceTo("1", "2", "3");
                result.GetValue(singleArityArg)
                      .Should()
                      .BeEquivalentSequenceTo("4");
            }

            [Fact]
            public void Multiple_arguments_can_differ_by_type()
            {
                var stringArg = new CliArgument<string>("the-string");
                var intArg = new CliArgument<int>("the-int");

                var command = new CliCommand("the-command")
                {
                    stringArg,
                    intArg
                };

                var result = command.Parse("1 2");

                result.GetValue(stringArg).Should().Be("1");
                result.GetValue(intArg).Should().Be(2);
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
                var first = new CliArgument<string>("first");
                var second = new CliArgument<string>("second");
                var third = new CliArgument<string[]>("third");
                var verbose = new CliOption<bool>("--verbose");

                var command = new CliCommand("the-command")
                {
                    first,
                    second,
                    third,
                    verbose
                };

                var parseResult = command.Parse(commandLine);

                parseResult
                    .GetValue(first)
                    .Should()
                    .Be("one");

                parseResult
                    .GetValue(second)
                    .Should()
                    .Be("two");

                parseResult
                    .GetValue(third)
                    .Should()
                    .BeEquivalentSequenceTo("three", "four", "five");

                parseResult
                    .GetValue(verbose)
                    .Should()
                    .BeTrue();
            }

            [Fact]
            public void Multiple_arguments_of_unspecified_type_are_parsed_correctly()
            {
                var sourceArg = new CliArgument<string>("source");
                var destinationArg = new CliArgument<string>("destination");
                var root = new CliRootCommand
                {
                    sourceArg,
                    destinationArg
                };

                var result = root.Parse("src.txt dest.txt");

                result.GetResult(sourceArg)
                      .GetValueOrDefault<string>()
                      .Should()
                      .Be("src.txt");
                
                result.GetResult(destinationArg)
                      .GetValueOrDefault<string>()
                      .Should()
                      .Be("dest.txt");
            }


            [Fact]
            public void When_multiple_arguments_are_defined_but_not_provided_then_option_parses_correctly()
            {
                var option = new CliOption<string>("-e");
                var command = new CliCommand("the-command")
                {
                    option,
                    new CliArgument<string>("arg1"),
                    new CliArgument<string>("arg2")
                };

                var result = command.Parse("-e foo");

                var optionResult = result.GetValue(option);

                optionResult.Should().Be("foo");
            }

            [Fact]
            public void Tokens_that_cannot_be_converted_by_multiple_arity_argument_flow_to_next_multiple_arity_argument()
            {
                var ints = new CliArgument<int[]>("ints");
                var strings = new CliArgument<string[]>("strings");

                var root = new CliRootCommand
                {
                    ints,
                    strings
                };

                var result = root.Parse("1 2 3 one two");

                var _ = new AssertionScope();

                result.GetValue(ints)
                      .Should()
                      .BeEquivalentTo(new[] { 1, 2, 3 },
                                      options => options.WithStrictOrdering());

                result.GetValue(strings)
                      .Should()
                      .BeEquivalentTo(new[] { "one", "two" },
                                      options => options.WithStrictOrdering());
            }

            [Fact]
            public void Tokens_that_cannot_be_converted_by_multiple_arity_argument_flow_to_next_single_arity_argument()
            {
                var ints = new CliArgument<int[]>("arg1");
                var strings = new CliArgument<string>("arg2");

                var root = new CliRootCommand
                {
                    ints,
                    strings
                };

                var result = root.Parse("1 2 3 four five");

                var _ = new AssertionScope();

                result.GetValue(ints)
                      .Should()
                      .BeEquivalentTo(new[] { 1, 2, 3 },
                                      options => options.WithStrictOrdering());

                result.GetValue(strings)
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
                var arg1 = new CliArgument<string>("arg1")
                {
                    Arity = ArgumentArity.ExactlyOne
                };
                var arg2 = new CliArgument<string>("arg2")
                {
                    Arity = ArgumentArity.ZeroOrOne,
                    DefaultValueFactory = (_) => "the-default"
                };
                var rootCommand = new CliRootCommand
                {
                    arg1,
                    arg2,
                };

                var result = rootCommand.Parse("value-1");

                result.GetValue(arg1).Should().Be("value-1");
                result.GetValue(arg2).Should().Be("the-default");
            }

            [Fact] // https://github.com/dotnet/command-line-api/issues/1403
            public void Unsatisfied_subsequent_argument_with_min_arity_1_parses_as_default_value()
            {
                CliArgument<string> arg1 = new(name: "arg1");
                CliArgument<string> arg2 = new(name: "arg2")
                {
                    DefaultValueFactory = (_) => "the-default"
                };

                var rootCommand = new CliRootCommand
                {
                    arg1,
                    arg2,
                };

                var result = rootCommand.Parse("");

                result.GetResult(arg1).Should().NotBeNull();
                result.GetValue(arg2).Should().Be("the-default");
            }

            [Fact] // https://github.com/dotnet/command-line-api/issues/1395
            public void When_subsequent_argument_with_ZeroOrOne_arity_is_not_provided_then_parse_is_correct()
            {
                var argument1 = new CliArgument<string>("arg1");
                var rootCommand = new CliRootCommand
                {
                    argument1,
                    new CliArgument<string>("arg2")
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    },
                };

                var result = rootCommand.Parse("one");

                result.Errors.Should().BeEmpty();

                result.GetValue(argument1).Should().Be("one");
            }

            [Theory] // https://github.com/dotnet/command-line-api/issues/1711
            [InlineData("")]
            [InlineData("a")]
            [InlineData("a b")]
            [InlineData("a b c")]
            public void When_there_are_not_enough_tokens_for_all_arguments_then_the_correct_number_of_errors_is_reported(
                string providedArgs)
            {
                var command = new CliCommand("command")
                {
                    new CliArgument<string>("arg1"),
                    new CliArgument<string>("arg2"),
                    new CliArgument<string>("arg3"),
                    new CliArgument<string>("arg4")
                };

                var result = CliParser.Parse(command, providedArgs);

                result
                    .Errors
                    .Count
                    .Should()
                    .Be(4 - providedArgs.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length);
            }
        }
    }
}
