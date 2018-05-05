// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.DotNet.Cli.CommandLine.Create;
using static Microsoft.DotNet.Cli.CommandLine.Define;

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class TypeConversionTests
    {
        private readonly ITestOutputHelper output;

        public TypeConversionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void ParseAs_can_specify_custom_types()
        {
            var parser = new CommandParser(
                Command("move", "",
                        Arguments()
                            .ParseAs<FileInfo[]>(parsed =>
                                                     ArgumentParseResult.Success(parsed.Arguments.Select(f => new FileInfo(f)).ToArray())
                            ),
                        Option("-d|--destination", "",
                               Arguments()
                                   .ParseAs<FileInfo>(parsed => ArgumentParseResult.Success(new FileInfo(parsed.Arguments.Single()))))));

            var folder = new DirectoryInfo(Path.Combine("temp"));
            var file1 = new FileInfo(Path.Combine(folder.FullName, "the file.txt"));
            var file2 = new FileInfo(Path.Combine(folder.FullName, "the other file.txt"));

            var result = parser.Parse($@"move -d ""{folder}"" ""{file1}"" ""{file2}""");

            var destination = result.ParsedCommand().ValueForOption<FileInfo>("d");
            var files = result.ParsedCommand().GetValueOrDefault<FileInfo[]>();

            destination
                .FullName
                .Should()
                .Be(folder.FullName);
            files
                .Select(f => f.FullName)
                .Should()
                .BeEquivalentTo(file2.FullName,
                                file1.FullName);
        }

        [Fact]
        public void When_argument_cannot_be_parsed_as_the_specified_type_then_ValueForOption_returns_null()
        {
            var command = Command("the-command", "",
                                  Option("-o|--one", "",
                                         Arguments()
                                             .ParseAs<int>(parsedSymbol =>
                                             {
                                                 if (int.TryParse(parsedSymbol.Arguments.Single(), out int intValue))
                                                 {
                                                     return ArgumentParseResult.Success(intValue);
                                                 }

                                                 return ArgumentParseResult.Failure($"'{parsedSymbol.Token}' is not an integer");
                                             })));

            var result = command.Parse("the-command -o not-an-int");

             result.ParsedCommand().ValueForOption("o").Should().BeNull();
        }

        [Fact]
        public void By_default_an_option_with_zero_or_one_argument_parses_as_the_argument_string_value_by_default()
        {
            var command = Command("the-command", "",
                                  Option("-x", "",
                                         new ArgumentRuleBuilder().ZeroOrOne()));

            var result = command.Parse("the-command -x the-argument");

            result.ParsedCommand()
                  .ValueForOption("x")
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void By_default_an_option_with_exactly_one_argument_parses_as_the_argument_string_value_by_default()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", new ArgumentRuleBuilder().ExactlyOne()));

            var result = command.Parse("the-command -x the-argument");

            result.ParsedCommand()
                  .ValueForOption("x")
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void When_exactly_one_argument_is_expected_and_none_are_provided_then_Value_returns_null()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", new ArgumentRuleBuilder().ExactlyOne()));

            var result = command.Parse("the-command -x");

            result.ParsedCommand()
                  .ValueForOption("x")
                  .Should()
                  .BeNull();
        }

        [Fact]
        public void When_zero_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_Value_returns_an_empty_sequence_of_strings()
        {
            var command = Command("the-command", "",
                                  Option("-x", "",
                                         new ArgumentRuleBuilder().ZeroOrMore()));

            var result = command.Parse("the-command -x");

            var value = result.ParsedCommand().ValueForOption("x");

            value.Should().BeAssignableTo<IReadOnlyCollection<string>>();

            var values = (IReadOnlyCollection<string>) value;

            values
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void When_one_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_Value_returns_null()
        {
            var command = Command("the-command", "",
                                  Option("-x", "",
                                         new ArgumentRuleBuilder().OneOrMore()));

            var result = command.Parse("the-command -x");

            result.ParsedCommand().ValueForOption("x").Should().BeNull();
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_multiple_arguments_parses_as_a_sequence_of_strings()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", new ArgumentRuleBuilder().ZeroOrMore()));

            command.Parse("the-command -x arg1 -x arg2").ParsedCommand()
                   .ValueForOption("x")
                   .ShouldBeEquivalentTo(new[] { "arg1", "arg2" });
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_one_argument_parses_as_a_sequence_of_strings()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", new ArgumentRuleBuilder().ZeroOrMore()));

            command.Parse("the-command -x arg1")
                   .ParsedCommand()
                   .ValueForOption("x")
                   .ShouldBeEquivalentTo(new[] { "arg1" });
        }

        [Fact]
        public void By_default_an_option_without_arguments_parses_as_true_when_it_is_applied()
        {
            var command = Command("something", "", ArgumentsRule.None,
                                  Option("-x", ""));

            var result = command.Parse("something -x");

            result.ParsedCommand()
                  .ValueForOption<bool>("x")
                  .Should()
                  .BeTrue();
        }

        [Fact]
        public void By_default_an_option_without_arguments_parses_as_false_when_it_is_not_applied()
        {
            var command = Command("something", "", Option("-x", ""));

            var result = command.Parse("something");

            result.ParsedCommand().ValueForOption<bool>("x").Should().BeFalse();
        }

        [Fact]
        public void An_option_with_a_default_value_parses_as_the_default_value_when_it_the_option_has_not_been_applied()
        {
            var command = Command(
                "something", "",
                Option("-x",
                       "",
                    Arguments().WithDefaultValue(() => "123").ExactlyOne()));

            var result = command.Parse("something");

            var parsedOption = result.ParsedCommand()["x"];

            parsedOption.GetValueOrDefault<string>().Should().Be("123");
        }

        [Fact(Skip = "not implemented yet")]
        public void When_OfType_is_used_and_an_argument_is_of_the_wrong_type_then_an_error_is_returned()
        {
            var command = Command("tally", "",
                                  Arguments()
                                        .ParseAs<int>(parsedSymbol =>
                                        {

                                            if (int.TryParse(parsedSymbol.Token, out var i))
                                            {
                                                return ArgumentParseResult.Success(i);
                                            }

                                            return ArgumentParseResult.Failure("Could not parse int");
                                        }));

            var result = command.Parse("tally one");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("oops wrong type");
        }

        [Fact(Skip = "not implemented yet")]
        public void OfType_can_be_used_to_parse_an_argument_as_int()
        {
            var command = Command("tally", "",
                                  Arguments()
                                        .ParseAs<int>(parsedSymbol =>
                                        {
                                            if (int.TryParse(parsedSymbol.Token, out var i))
                                            {
                                                return ArgumentParseResult.Success(i);
                                            }

                                            return ArgumentParseResult.Failure("Could not parse int");
                                        }));

            var result = command.Parse("tally 123");

            result.ParsedCommand()
                  .ValueForOption("tally")
                  .Should()
                  .BeOfType<int>();
        }

    }
}