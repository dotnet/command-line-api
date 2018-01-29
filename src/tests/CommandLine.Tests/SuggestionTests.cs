// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using FluentAssertions;
using Xunit;
using static Microsoft.DotNet.Cli.CommandLine.Accept;
using static Microsoft.DotNet.Cli.CommandLine.Create;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class SuggestionTests
    {
        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_command()
        {
            var parser = new Parser(Command("outer", "",
                                            Command("one", "Command one"),
                                            Command("two", "Command two"),
                                            Command("three", "Command three")));

            var result = parser.Parse("outer ");

            result.Suggestions().Should().BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(Command("outer", "",
                                            Option("--one", "Option one"),
                                            Option("--two", "Option two"),
                                            Option("--three", "Option three")));

            var result = parser.Parse("outer ");

            result.Suggestions().Should().BeEquivalentTo("--one", "--two", "--three");
        }

        [Fact]
        public void Argument_suggestions_can_be_based_on_the_proximate_option()
        {
            var parser = new Parser(
                Command("outer", "",
                        Option("--one", "", AnyOneOf("one-a", "one-b")),
                        Option("--two", "", AnyOneOf("two-a", "two-b"))));

            var result = parser.Parse("outer --two ");

            result.Suggestions().Should().BeEquivalentTo("two-a", "two-b");
        }

        [Fact]
        public void Option_suggestions_can_be_based_on_the_proximate_option_and_partial_input()
        {
            var parser = new Parser(Command("outer", "",
                                            Command("one", "Command one"),
                                            Command("two", "Command two"),
                                            Command("three", "Command three")));

            var result = parser.Parse("outer o");

            result.Suggestions().Should().BeEquivalentTo("one", "two");
        }

        [Fact]
        public void Suggestions_can_be_provided_in_the_absence_of_validation()
        {
            var command = Command("the-command", "",
                                  Option("-t", "",
                                         ExactlyOneArgument()
                                             .WithSuggestionsFrom(
                                                 "vegetable",
                                                 "mineral",
                                                 "animal")));

            command.Parse("the-command -t m")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");

            command.Parse("the-command -t something-else").Errors.Should().BeEmpty();
        }

        [Fact]
        public void Suggestions_can_be_provided_using_a_delegate()
        {
            var command = Command("the-command", "",
                                  Command("one", "",
                                          WithSuggestionsFrom(s => new[]
                                          {
                                              "vegetable",
                                              "mineral",
                                              "animal"
                                          })));

            command.Parse("the-command one m")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");
        }

        [Fact]
        public void Validations_and_suggestions_can_be_provided_using_a_delegate()
        {
            var command = Command("the-command", "",
                                  Command("one", "",
                                          AnyOneOf(() => new[]
                                          {
                                              "vegetable",
                                              "mineral",
                                              "animal"
                                          })));

            command.Parse("the-command one m")
                   .Suggestions()
                   .Should()
                   .BeEquivalentTo("animal",
                                   "mineral");

            command
                .Parse("the-command one fungus")
                .Errors
                .Select(e => e.Message)
                .Should()
                .BeEquivalentTo(
                    "Unrecognized command or argument 'fungus'",
                    "Required argument missing for command: one");
        }

        [Fact]
        public void When_we_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var parser = new Parser(
                Command("outer", "",
                        NoArguments(),
                        Option("one", "", arguments: AnyOneOf("one-a", "one-b", "one-c")),
                        Option("two", "", arguments: AnyOneOf("two-a", "two-b", "two-c")),
                        Option("three", "", arguments: AnyOneOf("three-a", "three-b", "three-c"))));

            var result = parser.Parse(new[] { "outer", "two", "b" });

            System.Console.WriteLine(result.Diagram());

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_option()
        {
            var parser = new Parser(
                Command("outer", "",
                        NoArguments(),
                        Option("one", "", arguments: AnyOneOf("one-a", "one-b", "one-c")),
                        Option("two", "", arguments: AnyOneOf("two-a", "two-b", "two-c")),
                        Option("three", "", arguments: AnyOneOf("three-a", "three-b", "three-c"))));

            var result = parser.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_we_do_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var parser = new Parser(
                Command("outer", "",
                        NoArguments(),
                        Command("one", "", arguments: AnyOneOf("one-a", "one-b", "one-c")),
                        Command("two", "", arguments: AnyOneOf("two-a", "two-b", "two-c")),
                        Command("three", "", arguments: AnyOneOf("three-a", "three-b", "three-c"))));

            var result = parser.Parse(new[] { "outer", "two", "b" });

            System.Console.WriteLine(result.Diagram());

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }

        [Fact]
        public void When_caller_does_the_tokenizing_then_argument_suggestions_are_based_on_the_proximate_command()
        {
            var parser = new Parser(
                Command("outer", "",
                        Command("one", "", arguments: AnyOneOf("one-a", "one-b", "one-c")),
                        Command("two", "", arguments: AnyOneOf("two-a", "two-b", "two-c")),
                        Command("three", "", arguments: AnyOneOf("three-a", "three-b", "three-c")))
            );

            var result = parser.Parse("outer two b");

            result.Suggestions()
                  .Should()
                  .BeEquivalentTo("two-b");
        }
    }
}