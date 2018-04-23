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

namespace Microsoft.DotNet.Cli.CommandLine.Tests
{
    public class MaterializerTests
    {
        private readonly ITestOutputHelper output;

        public MaterializerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void A_materializer_can_be_specified_using_options_and_arguments_to_create_an_object()
        {
            var parser = new CommandParser(
                Command("move", "",
                    Define.Arguments()
                        .OfType<FileMoveOperation>(parsedSymbol =>
                        {
                            output.WriteLine(parsedSymbol.Token);

                            var fileInfos = parsedSymbol.Arguments.Select(f => new FileInfo(f)).ToList();

                            var destination = new DirectoryInfo(parsedSymbol.Children["destination"].Arguments.Single());

                            return Result.Success(new FileMoveOperation
                            {
                                Files = fileInfos,
                                Destination = destination
                            });
                        })
                        .OneOrMore(),
                        options: new[]
                        {
                            Option("-d|--destination", "", Accept.ExactlyOneArgument())
                        }));

            var folder = new DirectoryInfo(Path.Combine("temp"));
            var file1 = new FileInfo(Path.Combine(folder.FullName, "the file.txt"));
            var file2 = new FileInfo(Path.Combine(folder.FullName, "the other file.txt"));

            var result = parser.Parse($@"move -d ""{folder}"" ""{file1}"" ""{file2}""");

            var fileMoveOperation = result.ParsedCommand()
                                          .GetValueOrDefault<FileMoveOperation>();

            fileMoveOperation.Destination
                             .FullName
                             .Should()
                             .Be(folder.FullName);

            fileMoveOperation.Files
                             .Select(f => f.FullName)
                             .Should()
                             .BeEquivalentTo(file2.FullName,
                                             file1.FullName);
        }

        [Fact]
        public void When_a_materializer_throws_then_an_informative_exception_message_is_given()
        {
            var command = Command("the-command", "",
                                  Option("-o|--one", "",
                                  Define.Arguments().OfType<int>(parsedSymbol =>
                                  {
                                      if (int.TryParse(parsedSymbol.Token, out int intValue))
                                      {
                                          return Result.Success(intValue);
                                      }
                                      return Result.Failure($"'{parsedSymbol.Token}' is not an integer");
                                  }).ExactlyOne()));

            var result = command.Parse("the-command -o not-an-int");

            Action getValue = () => result.ParsedCommand().ValueForOption("o");

            getValue.ShouldThrow<ParseException>()
                    .Which
                    .Message
                    .Should()
                    .Be("An exception occurred while getting the value for option 'one' based on argument(s): not-an-int.");
        }

        [Fact]
        public void By_default_an_option_with_zero_or_one_argument_materializes_as_the_argument_string_value_by_default()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", Accept.ZeroOrOneArgument()));

            var result = command.Parse("the-command -x the-argument");

            result.ParsedCommand()
                .ValueForOption("x")
                .Should()
                .Be("the-argument");
        }

        [Fact]
        public void By_default_an_option_with_exactly_one_argument_materializes_as_the_argument_string_value_by_default()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", Accept.ExactlyOneArgument()));

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
                                  Option("-x", "", Accept.ExactlyOneArgument()));

            var result = command.Parse("the-command -x");

            result.ParsedCommand()
                .ValueForOption("x")
                .Should()
                .BeNull();
        }

        [Fact]
        public void When_one_or_more_arguments_are_expected_and_none_are_provided_then_Value_returns_empty()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", Accept.OneOrMoreArguments()));

            var result = command.Parse("the-command -x");

            var value = result.ParsedCommand().ValueForOption("x");

            value.Should().BeAssignableTo<IReadOnlyCollection<string>>();

            var values = (IReadOnlyCollection<string>) value;

            values
                .Should()
                .BeEmpty();
        }

        [Fact]
        public void By_default_an_option_with_multiple_arguments_materializes_as_a_sequence_of_strings_by_default()
        {
            var command = Command("the-command", "",
                                  Option("-x", "", Accept.ZeroOrMoreArguments()));

            var result = command.Parse("the-command -x arg1 -x arg2");

            result.ParsedCommand()
                .ValueForOption("x")
                .ShouldBeEquivalentTo(new[] { "arg1", "arg2" });

            command.Parse("the-command -x arg1").ParsedCommand()
                   .ValueForOption("x")
                   .ShouldBeEquivalentTo(new[] { "arg1" });
        }

        [Fact]
        public void By_default_an_option_without_arguments_materializes_as_true_when_it_is_applied()
        {
            var command = Command("something", "",
                                  Accept.NoArguments(),
                                  Option("-x", ""));

            var result = command.Parse("something -x");

            result.ParsedCommand()
                  .ValueForOption<bool>("x")
                  .Should()
                  .BeTrue();
        }

        [Fact]
        public void By_default_an_option_without_arguments_materializes_as_false_when_it_is_not_applied()
        {
            var command = Command("something", "", Option("-x", ""));

            var result = command.Parse("something");

            result.ParsedCommand().ValueForOption<bool>("x").Should().BeFalse();
        }

        [Fact]
        public void An_option_with_a_default_value_materializes_as_the_default_value_when_it_the_option_has_not_been_applied()
        {
            var command = Command(
                "something", "",
                Option("-x",
                       "",
                    Define.Arguments().WithDefaultValue(() => "123").ExactlyOne()));

            var result = command.Parse("something");

            var parsedOption = result.ParsedCommand()["x"];

            parsedOption.GetValueOrDefault<string>().Should().Be("123");
        }

        [Fact(Skip = "not implemented yet")]
        public void When_OfType_is_used_and_an_argument_is_of_the_wrong_type_then_an_error_is_returned()
        {
            var command = Command("tally", "",
                                  Define.Arguments()
                                        .OfType<int>(parsedSymbol =>
                                        {
                                            
                                            if (int.TryParse(parsedSymbol.Token, out var i))
                                            {
                                                return Result.Success(i);
                                            }

                                            return Result.Failure("Could not parse int");
                                        })
                                        .ExactlyOne());

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
                                  Define.Arguments()
                                        .OfType<int>(parsedSymbol =>
                                        {
                                            if (int.TryParse(parsedSymbol.Token, out var i))
                                            {
                                                return Result.Success(i);
                                            }

                                            return Result.Failure("Could not parse int");
                                        })
                                        .ExactlyOne());

            var result = command.Parse("tally 123");

            result.ParsedCommand()
                  .ValueForOption("tally")
                  .Should()
                  .BeOfType<int>();
        }

        public class FileMoveOperation
        {
            public List<FileInfo> Files { get; set; }
            public DirectoryInfo Destination { get; set; }
        }
    }
}