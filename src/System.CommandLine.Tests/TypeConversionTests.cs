// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests
{
    public class TypeConversionTests
    {
        [Fact]
        public void ParseArgumentsAs_can_specify_custom_types_and_conversion_logic()
        {
            var parser = new Parser(
                new Command("custom", "",
                            new ArgumentBuilder()
                                .ParseArgumentsAs<MyCustomType>(parsed => {
                                    var custom = new MyCustomType();
                                    foreach (var argument in parsed.Arguments)
                                    {
                                        custom.Add(argument);
                                    }

                                    return ArgumentParseResult.Success(custom);
                                }, ArgumentArity.ZeroOrMore)));

            var result = parser.Parse("custom one two three");

            var customType = result.CommandResult.GetValueOrDefault<MyCustomType>();

            customType
                .Values
                .Should()
                .BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void ParseArgumentsAs_with_arity_of_one_can_be_called_without_custom_conversion_logic_if_the_type_has_a_constructor_thats_takes_a_single_string()
        {
            var option = new Option(
                "--file",
                "",
                argument: new ArgumentBuilder().ParseArgumentsAs<FileInfo>());

            var file = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "the-file.txt"));
            var result = option.Parse($"--file {file.FullName}");

            result.ValueForOption("--file")
                  .Should()
                  .BeOfType<FileInfo>()
                  .Which
                  .Name
                  .Should()
                  .Be("the-file.txt");
        }

        [Fact]
        public void ParseArgumentsAs_with_arity_of_many_can_be_called_without_custom_conversion_logic_if_the_item_type_has_a_constructor_thats_takes_a_single_string()
        {
            var option = new Option(
                "--file",
                "",
                argument: new ArgumentBuilder().ParseArgumentsAs<FileInfo[]>());

            var file1 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file1.txt"));
            var file2 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file2.txt"));
            var result = option.Parse($"--file {file1.FullName} --file {file2.FullName}");

            result.ValueForOption("--file")
                  .Should()
                  .BeOfType<FileInfo[]>()
                  .Which
                  .Select(fi => fi.Name)
                  .Should()
                  .BeEquivalentTo("file1.txt", "file2.txt");
        }

        [Fact]
        public void ParseArgumentsAs_defaults_arity_to_One_for_non_IEnumerable_types()
        {
            var argument = new ArgumentBuilder().ParseArgumentsAs<int>(s => ArgumentParseResult.Success(1));

            argument.ArgumentArity.Should().Be(ArgumentArity.ExactlyOne);
        }

        [Fact]
        public void ParseArgumentsAs_defaults_arity_to_ExactlyOne_for_string()
        {
            var argument = new ArgumentBuilder().ParseArgumentsAs<string>(s => ArgumentParseResult.Success(1));

            argument.ArgumentArity.Should().Be(ArgumentArity.ExactlyOne);
        }

        [Fact]
        public void ParseArgumentsAs_infers_arity_of_IEnumerable_types_as_OneOrMore()
        {
            var argument = new ArgumentBuilder().ParseArgumentsAs<int[]>(s => ArgumentParseResult.Success(1));

            argument.ArgumentArity.Should().Be(ArgumentArity.OneOrMore);
        }

        [Fact]
        public void ParseArgumentsAs_bool_will_default_to_true_when_no_argument_is_passed()
        {
            var parser = new CommandLineBuilder()
                         .AddOption("-x", "", args => args.ParseArgumentsAs<bool>())
                         .Build();

            var result = parser.Parse("-x");

            result.Errors
                  .Should()
                  .BeEmpty();
            result["x"].Result
                       .Should()
                       .BeOfType<SuccessfulArgumentParseResult<bool>>()
                       .Which
                       .Value
                       .Should()
                       .BeTrue();
            result.ValueForOption("x").Should().Be(true);
        }

        [Fact]
        public void ParseArgumentsAs_parses_as_the_default_value_when_the_option_has_not_been_applied()
        {
            var command = new Command("something", "", new[] {
                new Option("-x", "",
                           new ArgumentBuilder()
                               .WithDefaultValue(() => "123")
                               .ParseArgumentsAs<int>())
            });

            var result = command.Parse("something");

            var option = result.CommandResult["x"];

            option.GetValueOrDefault().Should().Be(123);
        }

        [Fact]
        public void ParseArgumentsAs_does_not_parse_as_the_default_value_when_the_option_has_been_applied()
        {
            var command = new Command("something", "", new[] {
                new Option("-x", "",
                           new ArgumentBuilder()
                               .WithDefaultValue(() => "123")
                               .ParseArgumentsAs<int>())
            });

            var result = command.Parse("something -x 456");

            var option = result.CommandResult["x"];

            option.GetValueOrDefault().Should().Be(456);
        }

        [Theory]
        [InlineData("the-command -x")]
        [InlineData("the-command -x true")]
        [InlineData("the-command -x:true")]
        [InlineData("the-command -x=true")]
        public void ParseArgumentsAs_bool_does_not_parse_as_the_default_value_when_the_option_has_been_applied(string commandLine)
        {
            var command = new Command("the-command", "", new[] {
                new Option("-x", "",
                           new ArgumentBuilder()
                               .WithDefaultValue(() => "false")
                               .ParseArgumentsAs<bool>())
            });

            command
                .Parse(commandLine)
                .CommandResult["x"]
                .GetValueOrDefault()
                .Should()
                .Be(true);
        }

        [Fact]
        public void When_argument_cannot_be_parsed_as_the_specified_type_then_getting_value_throws()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    new[] { "-o", "--one" },
                    "",
                    argument: new ArgumentBuilder()
                        .ParseArgumentsAs<int>(symbol => {
                            if (int.TryParse(symbol.Arguments.Single(), out int intValue))
                            {
                                return ArgumentParseResult.Success(intValue);
                            }

                            return ArgumentParseResult.Failure($"'{symbol.Token}' is not an integer");
                        }))
            });

            var result = command.Parse("the-command -o not-an-int");

            Action getValue = () =>
                result.CommandResult.ValueForOption("o");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("'-o' is not an integer");
        }

        [Fact]
        public void By_default_an_option_with_zero_or_one_argument_parses_as_the_argument_string_value_by_default()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    "-x",
                    "",
                    argument: new ArgumentBuilder().ZeroOrOne())
            });

            var result = command.Parse("the-command -x the-argument");

            result.CommandResult
                  .ValueForOption("x")
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void By_default_an_option_with_exactly_one_argument_parses_as_the_argument_string_value_by_default()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    "-x",
                    "",
                    argument: new ArgumentBuilder().ExactlyOne())
            });

            var result = command.Parse("the-command -x the-argument");

            result.CommandResult
                  .ValueForOption("x")
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void When_exactly_one_argument_is_expected_and_none_are_provided_then_getting_value_throws()
        {
            var option = new Option("-x", "",
                                    new ArgumentBuilder().ExactlyOne());

            var command = new Command("the-command", "", new[] {
                option
            });

            var result = command.Parse("the-command -x");

            Action getValue = () => result.CommandResult.ValueForOption("x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be(ValidationMessages.Instance.RequiredArgumentMissing(new OptionResult(option)));
        }

        [Fact]
        public void When_zero_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_getting_value_returns_an_empty_sequence_of_strings()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    "-x",
                    "",
                    argument: new ArgumentBuilder().ZeroOrMore())
            });

            var result = command.Parse("the-command -x");

            result.CommandResult
                  .ValueForOption("x")
                  .Should()
                  .BeAssignableTo<IReadOnlyCollection<string>>()
                  .Which
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void
            When_zero_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_and_there_is_a_default_then_getting_value_returns_default_in_an_empty_sequence_of_strings()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    "-x",
                    "",
                    argument: new ArgumentBuilder()
                              .WithDefaultValue(() => "the-default")
                              .ZeroOrMore())
            });

            var result = command.Parse("the-command");

            result.CommandResult
                  .ValueForOption("x")
                  .Should()
                  .BeAssignableTo<IReadOnlyCollection<string>>()
                  .Which
                  .Should()
                  .BeEquivalentTo("the-default");
        }

        [Fact]
        public void When_one_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_getting_value_throws()
        {
            var option = new Option(
                "-x",
                "",
                new ArgumentBuilder().OneOrMore());
            var command = new Command("the-command", "", new[] {
                option
            });

            var result = command.Parse("the-command -x");

            Action getValue = () => result.CommandResult.ValueForOption("x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be(ValidationMessages.Instance.RequiredArgumentMissing(new OptionResult(option)));
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_multiple_arguments_parses_as_a_sequence_of_strings()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    "-x",
                    "",
                    argument: new ArgumentBuilder().ZeroOrMore())
            });

            command.Parse("the-command -x arg1 -x arg2")
                   .CommandResult
                   .ValueForOption("x")
                   .Should()
                   .BeEquivalentTo(new[] { "arg1", "arg2" });
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_one_argument_parses_as_a_sequence_of_strings()
        {
            var command = new Command("the-command", "", new[] {
                new Option(
                    "-x", "",
                    new ArgumentBuilder().ZeroOrMore())
            });

            command.Parse("the-command -x arg1").CommandResult
                   .ValueForOption("x")
                   .Should()
                   .BeEquivalentTo(new[] { "arg1" });
        }

        [Fact]
        public void The_default_value_of_a_command_with_no_arguments_is_an_empty_collection()
        {
            var option = new CommandResult(new Command("-x", ""));

            option.GetValueOrDefault()
                  .Should()
                  .BeAssignableTo<IReadOnlyCollection<string>>()
                  .Which
                  .Count
                  .Should()
                  .Be(0);
        }

        [Fact]
        public void The_default_value_of_an_option_with_no_arguments_is_true()
        {
            var command = new OptionResult(new Option("-x", ""));

            command.GetValueOrDefault().Should().Be(null);
        }

        [Fact]
        public void By_default_an_option_without_arguments_parses_as_false_when_it_is_not_applied()
        {
            var command = new Command("something", "", new[] {
                new Option("-x", "")
            });

            var result = command.Parse("something");

            result.CommandResult
                  .ValueForOption<bool>("x")
                  .Should()
                  .BeFalse();
        }

        [Fact]
        public void An_option_with_a_default_value_parses_as_the_default_value_when_the_option_has_not_been_applied()
        {
            var command = new Command("something", "", new[] {
                new Option(
                    "-x", "",
                    new ArgumentBuilder()
                        .WithDefaultValue(() => "123")
                        .ExactlyOne())
            });

            var result = command.Parse("something");

            var option = result.CommandResult["x"];

            option.GetValueOrDefault<string>()
                  .Should()
                  .Be("123");
            option.GetValueOrDefault<int>()
                  .Should()
                  .Be(123);
        }

        [Fact]
        public void A_default_value_of_a_non_string_type_can_be_specified()
        {
            var command = new Command("something", "", new[] {
                new Option(
                    "-x", "",
                    new ArgumentBuilder()
                        .WithDefaultValue(() => {
                            return 123;
                        })
                        .ExactlyOne())
            });

            var result = command.Parse("something");

            var option = result.CommandResult["x"];

            option.GetValueOrDefault()
                  .Should()
                  .Be(123);
        }

        [Fact]
        public void An_option_with_a_default_value_can_be_converted_to_the_requested_type()
        {
            var command = new Command("something", "", new[] {
                new Option(
                    "-x", "",
                    new ArgumentBuilder()
                        .WithDefaultValue(() => "123")
                        .ExactlyOne())
            });

            var result = command.Parse("something");

            var option = result.CommandResult["x"];

            option.GetValueOrDefault<int>()
                  .Should()
                  .Be(123);
        }

        [Fact]
        public void Specifying_an_option_argument_overrides_the_default_value()
        {
            var command = new Command("something", "", new[] {
                new Option(
                    "-x",
                    "",
                    new ArgumentBuilder()
                        .WithDefaultValue(() => "123")
                        .ExactlyOne())
            });

            var result = command.Parse("something -x 456");

            var option = result.CommandResult["x"];

            option.GetValueOrDefault<string>().Should().Be("456");
        }

        [Fact]
        public void When_ParseArgumentsAs_is_used_and_an_argument_is_of_the_wrong_type_then_an_error_is_returned()
        {
            var command = new Command(
                "tally", "", new ArgumentBuilder()
                    .ParseArgumentsAs<int>(symbol => ArgumentParseResult.Failure("Could not parse int")));

            var result = command.Parse("tally one");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Could not parse int");
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrOne());

            var value = option.Parse("-x 123").ValueForOption<int>("x");

            value.Should().Be(123);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_decimal_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrOne());

            var value = option.Parse("-x 123.456").ValueForOption<decimal>("x");

            value.Should().Be(123.456m);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_double_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrOne());

            var value = option.Parse("-x 123.456").ValueForOption<double>("x");

            value.Should().Be(123.456d);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_float_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrOne());

            var value = option.Parse("-x 123.456").ValueForOption<float>("x");

            value.Should().Be(123.456f);
        }

        [Fact]
        public void Options_with_no_arguments_specified_can_be_correctly_converted_to_bool_without_the_parser_specifying_it()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrOne());

            option.Parse("-x").ValueForOption<bool>("x").Should().BeTrue();
        }

        [Fact]
        public void Options_with_arguments_specified_can_be_correctly_converted_to_bool_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrOne());

            option.Parse("-x false").ValueForOption<bool>("x").Should().BeFalse();
            option.Parse("-x true").ValueForOption<bool>("x").Should().BeTrue();
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_array_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrMore());

            var value = option.Parse("-x 1 -x 2 -x 3").ValueForOption<int[]>("x");

            value.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_List_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrMore());

            var value = option.Parse("-x 1 -x 2 -x 3").ValueForOption<List<int>>("x");

            value.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void Enum_values_can_be_correctly_converted_based_on_enum_value_name_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ParseArgumentsAs<DayOfWeek>());

            var value = option.Parse("-x Monday").ValueForOption<DayOfWeek>("x");

            value.Should().Be(DayOfWeek.Monday);
        }

        [Fact]
        public void Enum_values_that_cannot_be_parsed_result_in_an_informative_error()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ParseArgumentsAs<DayOfWeek>());

            var value = option.Parse("-x Notaday");

            value.Errors
                 .Select(e => e.Message)
                 .Should()
                 .Contain("Cannot parse argument 'Notaday' as System.DayOfWeek.");
        }

        [Fact]
        public void When_getting_values_and_specifying_a_conversion_type_that_is_not_supported_then_it_throws()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrOne());

            var result = option.Parse("-x not-an-int");

            Action getValue = () => result.ValueForOption<int>("x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Cannot parse argument 'not-an-int' as System.Int32.");
        }

        [Fact]
        public void When_getting_an_array_of_values_and_specifying_a_conversion_type_that_is_not_supported_then_it_throws()
        {
            var option = new Option(
                "-x",
                "",
                argument: new ArgumentBuilder().ZeroOrOne());

            var result = option.Parse("-x not-an-int -x 2");

            Action getValue = () => result.ValueForOption<int[]>("x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Cannot parse argument 'not-an-int' as System.Int32[].");
        }

        public class MyCustomType
        {
            private readonly List<string> values = new List<string>();

            public void Add(string value) => values.Add(value);

            public string[] Values => values.ToArray();
        }
    }
}
