// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace System.CommandLine.Tests
{
    public partial class ParserTests
    {
        public class MultipleArguments
        {
            private readonly ITestOutputHelper _output;

            public MultipleArguments(ITestOutputHelper output)
            {
                _output = output;
            }

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

                var several = result.CommandResult
                                    .GetArgumentValueOrDefault<IEnumerable<string>>("several");

                var one = result.CommandResult
                                .GetArgumentValueOrDefault<IEnumerable<string>>("one");

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

                var theString = result.CommandResult
                                    .GetArgumentValueOrDefault<string>("the-string");

                var theInt = result.CommandResult
                                .GetArgumentValueOrDefault<int>("the-int");

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
                    new Option("--verbose")
                    {
                        Argument = new Argument<bool>()
                    }  
                };

                var parseResult = command.Parse(commandLine);

                var commandResult = parseResult.CommandResult;

                commandResult
                    .GetArgumentValueOrDefault<string>("first")
                    .Should()
                    .Be("one");

                commandResult
                    .GetArgumentValueOrDefault<string>("second")
                    .Should()
                    .Be("two");

                commandResult
                    .GetArgumentValueOrDefault<string[]>("third")
                    .Should()
                    .BeEquivalentSequenceTo("three", "four", "five");

                commandResult.ValueForOption<bool>("--verbose")
                             .Should()
                             .BeTrue();
            }
        }
    }
}
