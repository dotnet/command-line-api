﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class TypeConversionTests
    {
        [Fact]
        public void Option_argument_of_FileInfo_can_be_bound_without_custom_conversion_logic()
        {
            var option = new Option<FileInfo>("--file");

            var file = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "the-file.txt"));
            var result = option.Parse($"--file {file.FullName}");

            result.GetValueForOption(option)
                  .Name
                  .Should()
                  .Be("the-file.txt");
        }

        [Fact]
        public void Command_argument_of_FileInfo_can_be_bound_without_custom_conversion_logic()
        {
            var argument = new Argument<FileInfo>("the-arg");

            var command = new Command("the-command")
            {
                argument
            };

            var file = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "the-file.txt"));
            var result = command.Parse($"{file.FullName}");

            result.GetValueForArgument(argument)
                  .Name
                  .Should()
                  .Be("the-file.txt");
        }

        [Fact]
        public void Command_argument_of_FileInfo_returns_null_when_argument_is_not_provided()
        {
            var argument = new Argument<FileInfo>("the-arg")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            var command = new Command("the-command")
            {
                argument
            };

            var result = command.Parse("");

            result.GetValueForArgument(argument)
                  .Should()
                  .BeNull();
        }

        [Fact]
        public void Argument_of_array_of_FileInfo_can_be_called_without_custom_conversion_logic()
        {
            var option = new Option<FileInfo[]>("--file");

            var file1 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file1.txt"));
            var file2 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file2.txt"));
            var result = option.Parse($"--file {file1.FullName} --file {file2.FullName}");

            result.GetValueForOption(option)
                  .Select(fi => fi.Name)
                  .Should()
                  .BeEquivalentTo("file1.txt", "file2.txt");
        }

        [Fact]
        public void Argument_defaults_arity_to_One_for_non_IEnumerable_types()
        {
            var argument = new Argument<int>();

            argument.Arity.Should().BeEquivalentTo(ArgumentArity.ExactlyOne);
        }

        [Fact]
        public void Argument_defaults_arity_to_ExactlyOne_for_string()
        {
            var argument = new Argument<string>();

            argument.Arity.Should().BeEquivalentTo(ArgumentArity.ExactlyOne);
        }

        [Fact]
        public void Command_Argument_defaults_arity_to_ZeroOrOne_for_nullable_types()
        {
            var command = new Command("the-command")
            {
                new Argument<int?>()
            };

            command.Arguments.Single().Arity.Should().BeEquivalentTo(ArgumentArity.ZeroOrOne);
        }

        [Theory]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(List<int>))]
        public void Argument_infers_arity_of_IEnumerable_types_as_OneOrMore(Type type)
        {
            var argument = new Argument { ValueType = type };

            argument.Arity.Should().BeEquivalentTo(ArgumentArity.OneOrMore);
        }

        [Fact]
        public void Argument_parses_as_the_default_value_when_the_option_has_not_been_applied()
        {
            var option = new Option<int>("-x", () => 123);

            var command = new Command("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValueForOption(option).Should().Be(123);
        }

        [Fact]
        public void Option_does_not_parse_as_the_default_value_when_the_option_has_been_applied()
        {
            var option = new Option<int>("-x", () => 123);

            var command = new Command("something")
            {
                option
            };

            var result = command.Parse("something -x 456");

            result.GetValueForOption(option).Should().Be(456);
        }

        [Theory]
        [InlineData("the-command -x")]
        [InlineData("the-command -x true")]
        [InlineData("the-command -x:true")]
        [InlineData("the-command -x=true")]
        public void Bool_parses_as_true_when_the_option_has_been_applied(string commandLine)
        {
            var option = new Option<bool>("-x");

            var command = new Command("the-command")
            {
                option
            };

            command
                .Parse(commandLine)
                .GetValueForOption(option)
                .Should()
                .BeTrue();
        }

        [Theory]
        [InlineData("the-command -x")]
        [InlineData("the-command -x true")]
        [InlineData("the-command -x:true")]
        [InlineData("the-command -x=true")]
        public void Nullable_bool_parses_as_true_when_the_option_has_been_applied(string commandLine)
        {
            var option = new Option<bool?>("-x");

            var command = new Command("the-command")
            {
                option
            };

            command
                .Parse(commandLine)
                .GetValueForOption(option)
                .Should()
                .BeTrue();
        }

        [Theory]
        [InlineData("the-command -x false")]
        [InlineData("the-command -x:false")]
        [InlineData("the-command -x=false")]
        public void Nullable_bool_parses_as_false_when_the_option_has_been_applied(string commandLine)
        {
            var option = new Option<bool?>("-x");

            var command = new Command("the-command")
            {
                option
            };

            command
                .Parse(commandLine)
                .GetValueForOption(option)
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Nullable_bool_parses_as_null_when_the_option_has_not_been_applied()
        {
            var option = new Option<bool?>("-x");
            
            option
                .Parse("")
                .GetValueForOption(option)
                .Should()
                .Be(null);
        }

        [Fact]
        public void By_default_an_option_with_zero_or_one_argument_parses_as_the_argument_string_value()
        {
            var option = new Option("-x", arity: ArgumentArity.ZeroOrOne);

            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command -x the-argument");

            result.GetValueForOption(option)
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void By_default_an_option_with_exactly_one_argument_parses_as_the_argument_string_value()
        {
            var option = new Option("-x", arity: ArgumentArity.ExactlyOne);

            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command -x the-argument");

            result.GetValueForOption(option)
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void When_exactly_one_argument_is_expected_and_none_are_provided_then_getting_value_throws()
        {
            var option = new Option("-x", arity: ArgumentArity.ExactlyOne);

            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command -x");

            Action getValue = () => result.GetValueForOption(option);

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Required argument missing for option: '-x'.");
        }

        [Fact]
        public void When_zero_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_getting_value_returns_an_empty_sequence_of_strings()
        {
            var option = new Option("-x", arity: ArgumentArity.ZeroOrMore);

            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command -x");

            result.GetValueForOption(option)
                  .Should()
                  .BeAssignableTo<IReadOnlyCollection<string>>()
                  .Which
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void When_one_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_getting_value_throws()
        {
            var option = new Option("-x", arity: ArgumentArity.OneOrMore);

            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command -x");

            Action getValue = () => result.GetValueForOption(option);

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Required argument missing for option: '-x'.");
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_multiple_arguments_parses_as_a_sequence_of_strings()
        {
            var option = new Option("-x", arity: ArgumentArity.ZeroOrMore);

            var command = new Command("the-command")
            {
                option
            };

            command.Parse("the-command -x arg1 -x arg2")
                   .GetValueForOption(option)
                   .Should()
                   .BeEquivalentTo(new[] { "arg1", "arg2" });
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_one_argument_parses_as_a_sequence_of_strings()
        {
            var option = new Option("-x", arity: ArgumentArity.ZeroOrMore);

            var command = new Command("the-command")
            {
                option
            };

            command.Parse("the-command -x arg1")
                   .GetValueForOption(option)
                   .Should()
                   .BeEquivalentTo(new[] { "arg1" });
        }

        [Theory]
        [InlineData("c -a o c c")]
        [InlineData("c c -a o c")]
        [InlineData("c c c")]
        public void When_command_argument_has_arity_greater_than_one_it_captures_arguments_before_and_after_option(string commandLine)
        {
            var argument = new Argument<string[]>("the-arg")
            {
                Arity = ArgumentArity.ZeroOrMore
            };

            var command = new Command("the-command")
            {
                new Option<string>("-a"),
                argument
            };

            var result = command.Parse(commandLine);

            result.GetValueForArgument(argument)
                  .Should()
                  .BeEquivalentTo(new[] { "c", "c", "c" });
        }

        [Fact]
        public void The_default_value_of_an_option_with_no_arguments_is_true()
        {
            var option = new Option("-x");

            var command =
                new Command("the-command")
                {
                    option
                };

            var result = command.Parse("-x");

            result.GetValueForOption(option)
                  .Should()
                  .Be(true);
        }

        [Fact]
        public void By_default_an_option_without_arguments_parses_as_false_when_it_is_not_applied()
        {
            var option = new Option("-x");

            var command = new Command("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValueForOption(option)
                  .Should()
                  .Be(false);
        }

        [Fact]
        public void An_option_with_a_default_value_parses_as_the_default_value_when_the_option_has_not_been_applied()
        {
            var option = new Option<string>("-x", () => "123");

            var command = new Command("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValueForOption(option)
                  .Should()
                  .Be("123");
        }

        [Fact]
        public void An_option_with_a_default_value_of_null_parses_as_null_when_the_option_has_not_been_applied()
        {
            var option = new Option<string>("-x", () => null);

            var command = new Command("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValueForOption(option)
                  .Should()
                  .Be(null);
        }

        [Fact]
        public void A_default_value_of_a_non_string_type_can_be_specified()
        {
            var option = new Option<int>("-x", () => 123);

            var command = new Command("something")
            {
                option
            };

            command.Parse("something")
                   .GetValueForOption(option)
                   .Should()
                   .Be(123);
        }

        [Fact]
        public void A_default_value_with_a_custom_constructor_can_be_specified_for_an_option_argument()
        {
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            var option = new Option<DirectoryInfo>("-x", () => directoryInfo);

            var command = new Command("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValueForOption(option).Should().Be(directoryInfo);
        }

        [Fact]
        public void A_default_value_with_a_custom_constructor_can_be_specified_for_a_command_argument()
        {
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            var argument = new Argument<DirectoryInfo>("the-arg", () => directoryInfo);

            var command = new Command("something")
            {
                argument
            };

            var result = command.Parse("something");

            result.Errors.Should().BeEmpty();

            var value = result.GetValueForArgument(argument);

            value.Should().Be(directoryInfo);
        }

        [Fact]
        public void Specifying_an_option_argument_overrides_the_default_value()
        {
            var option = new Option<int>("-x", () => 123);

            var command = new Command("something")
            {
                option
            };

            var result = command.Parse("something -x 456");

            var value = result.GetValueForOption(option);

            value.Should().Be(456);
        }


        [Fact]
        public void Values_can_be_correctly_converted_to_DateTime_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<DateTime>("-x");

            var dateString = "2022-02-06T01:46:03.0000000-08:00";
            var value = option.Parse($"-x {dateString}").GetValueForOption(option);

            value.Should().Be(DateTime.Parse(dateString));
        }


        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_DateTime_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<DateTime?>("-x");

            var dateString = "2022-02-06T01:46:03.0000000-08:00";
            var value = option.Parse($"-x {dateString}").GetValueForOption(option);

            value.Should().Be(DateTime.Parse(dateString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_DateTimeOffset_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<DateTimeOffset>("-x");

            var dateString = "2022-02-06T09:52:54.5275055-08:00";
            var value = option.Parse($"-x {dateString}").GetValueForOption(option);

            value.Should().Be(DateTime.Parse(dateString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_DateTimeOffset_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<DateTimeOffset?>("-x");

            var dateString = "2022-02-06T09:52:54.5275055-08:00";
            var value = option.Parse($"-x {dateString}").GetValueForOption(option);

            value.Should().Be(DateTime.Parse(dateString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_decimal_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<decimal>("-x");

            var result = option.Parse("-x 123.456");

            var value = result.GetValueForOption(option);

            value.Should().Be(123.456m);
        }
        
        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_decimal_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<decimal?>("-x");

            var value = option.Parse("-x 123.456").GetValueForOption(option);

            value.Should().Be(123.456m);
        }
    
        [Fact]
        public void Values_can_be_correctly_converted_to_double_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<double>("-x");

            var value = option.Parse("-x 123.456").GetValueForOption(option);

            value.Should().Be(123.456d);
        }
        
        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_double_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<double?>("-x");

            var value = option.Parse("-x 123.456").GetValueForOption(option);

            value.Should().Be(123.456d);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_float_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<float>("-x");

            var value = option.Parse("-x 123.456").GetValueForOption(option);

            value.Should().Be(123.456f);
        }
        
        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_float_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<float?>("-x");

            var value = option.Parse("-x 123.456").GetValueForOption(option);

            value.Should().Be(123.456f);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_Guid_without_the_parser_specifying_a_custom_converter()
        {
            var guidString = "75517282-018F-46BB-B15F-1D8DBFE23F6E";
            var option = new Option<Guid>("-x");

            var value = option.Parse($"-x {guidString}").GetValueForOption(option);

            value.Should().Be(Guid.Parse(guidString));
        }
        
        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_Guid_without_the_parser_specifying_a_custom_converter()
        {
            var guidString = "75517282-018F-46BB-B15F-1D8DBFE23F6E";
            var option = new Option<Guid?>("-x");

            var value = option.Parse($"-x {guidString}").GetValueForOption(option);

            value.Should().Be(Guid.Parse(guidString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_Uri_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<Uri>("-x");

            var value = option.Parse("-x http://example.com").GetValueForOption(option);

            value.Should().BeEquivalentTo(new Uri("http://example.com"));
        }

        [Fact]
        public void Options_with_arguments_specified_can_be_correctly_converted_to_bool_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<bool>("-x");

            option.Parse("-x false").GetValueForOption(option).Should().BeFalse();
            option.Parse("-x true").GetValueForOption(option).Should().BeTrue();
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_long_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<long>("-x");

            var value = option.Parse("-x 123456790").GetValueForOption(option);

            value.Should().Be(123456790L);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_long_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<long?>("-x");

            var value = option.Parse("-x 1234567890").GetValueForOption(option);

            value.Should().Be(1234567890L);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_short_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<short>("-s");

            var value = option.Parse("-s 1234").GetValueForOption(option);

            value.Should().Be(1234);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_short_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<short?>("-s");

            var value = option.Parse("-s 1234").GetValueForOption(option);

            value.Should().Be(1234);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_ulong_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<ulong>("-x");

            var value = option.Parse("-x 1234").GetValueForOption(option);

            value.Should().Be(1234);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_ulong_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<ulong?>("-x");

            var value = option.Parse("-x 1234").GetValueForOption(option);

            value.Should().Be(1234);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_ushort_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<ushort>("-x");

            var value = option.Parse("-x 1234").GetValueForOption(option);

            value.Should().Be(1234);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_ushort_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<ushort?>("-x");

            var value = option.Parse("-x 1234").GetValueForOption(option);

            value.Should().Be(1234);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_array_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<int[]>("-x");

            var value = option.Parse("-x 1 -x 2 -x 3").GetValueForOption(option);

            value.Should().BeEquivalentTo(1, 2, 3);
        }
        
        [Theory]
        [InlineData(0, 100_000, typeof(string[]))]
        [InlineData(0, 3, typeof(string[]))]
        [InlineData(0, 100_000, typeof(IEnumerable<string>))]
        [InlineData(0, 3, typeof(IEnumerable<string>))]
        [InlineData(0, 100_000, typeof(List<string>))]
        [InlineData(0, 3, typeof(List<string>))]
        [InlineData(0, 100_000, typeof(IList<string>))]
        [InlineData(0, 3, typeof(IList<string>))]
        [InlineData(0, 100_000, typeof(ICollection<string>))]
        [InlineData(0, 3, typeof(ICollection<string>))]
        
        [InlineData(1, 100_000, typeof(string[]))]
        [InlineData(1, 3, typeof(string[]))]
        [InlineData(1, 100_000, typeof(IEnumerable<string>))]
        [InlineData(1, 3, typeof(IEnumerable<string>))]
        [InlineData(1, 100_000, typeof(List<string>))]
        [InlineData(1, 3, typeof(List<string>))]
        [InlineData(1, 100_000, typeof(IList<string>))]
        [InlineData(1, 3, typeof(IList<string>))]
        [InlineData(1, 100_000, typeof(ICollection<string>))]
        [InlineData(1, 3, typeof(ICollection<string>))]
        public void Max_arity_greater_than_1_converts_to_enumerable_types(
            int minArity,
            int maxArity,
            Type argumentType)
        {
            var option = new Option("--items", argumentType: argumentType, arity: new ArgumentArity(minArity, maxArity));

            var command = new RootCommand
            {
                option
            };

            var result = command.Parse("--items one --items two --items three");

            result.Errors.Should().BeEmpty();
            result.FindResultFor(option).GetValueOrDefault().Should().BeAssignableTo(argumentType);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_List_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<List<int>>("-x");

            var value = option.Parse("-x 1 -x 2 -x 3").GetValueForOption(option);

            value.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_IEnumerable_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<IEnumerable<int>>("-x");

            var value = option.Parse("-x 1 -x 2 -x 3").GetValueForOption(option);

            value.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void Enum_values_can_be_correctly_converted_based_on_enum_value_name_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<DayOfWeek>("-x");

            var parseResult = option.Parse("-x Monday");

            var value = parseResult.GetValueForOption(option);

            value.Should().Be(DayOfWeek.Monday);
        }

        [Fact]
        public void Nullable_enum_values_can_be_correctly_converted_based_on_enum_value_name_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option<DayOfWeek?>("-x");

            var parseResult = option.Parse("-x Monday");

            var value = parseResult.GetValueForOption(option);

            value.Should().Be(DayOfWeek.Monday);
        }

        [Fact]
        public void Enum_values_that_cannot_be_parsed_result_in_an_informative_error()
        {
            var option = new Option<DayOfWeek>("-x");

            var value = option.Parse("-x Notaday");

            value.Errors
                 .Should()
                 .ContainSingle()
                 .Which
                 .Message
                 .Should()
                 .Contain("Cannot parse argument 'Notaday' for option '-x' as expected type 'System.DayOfWeek'.");
        }

        [Fact]
        public void When_getting_a_single_value_and_specifying_a_conversion_type_that_is_not_supported_then_it_throws()
        {
            var option = new Option<int>("-x");

            var result = option.Parse("-x not-an-int");

            Action getValue = () => result.GetValueForOption(option);

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Cannot parse argument 'not-an-int' for option '-x' as expected type 'System.Int32'.");
        }

        [Fact]
        public void When_getting_an_array_of_values_and_specifying_a_conversion_type_that_is_not_supported_then_it_throws()
        {
            var option = new Option<int[]>("-x");

            var result = option.Parse("-x not-an-int -x 2");

            Action getValue = () => result.GetValueForOption(option);

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Cannot parse argument 'not-an-int' for option '-x' as expected type 'System.Int32'.");
        }

        [Fact]
        public void String_defaults_to_null_when_not_specified()
        {
            var argument = new Argument<string>();
            var command = new Command("mycommand")
            {
                argument
            };

            var result = command.Parse("mycommand");
            result.GetValueForArgument(argument)
                  .Should()
                  .BeNull();
        }

        [Theory]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(List<int>))]
        [InlineData(typeof(List<FileAccess>))]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(IEnumerable<FileAccess>))]
        [InlineData(typeof(ICollection<string>))]
        [InlineData(typeof(ICollection<int>))]
        [InlineData(typeof(ICollection<FileAccess>))]
        [InlineData(typeof(IList<string>))]
        [InlineData(typeof(IList<int>))]
        [InlineData(typeof(IList<FileAccess>))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(FileAccess[]))]
        [InlineData(typeof(IEnumerable))]
        [InlineData(typeof(ICollection))]
        [InlineData(typeof(IList))]
        public void Sequence_type_defaults_to_empty_when_not_specified(Type sequenceType)
        {
            var argument = Activator.CreateInstance(typeof(Argument<>).MakeGenericType(sequenceType));

            AssertParsedValueIsEmpty((dynamic)argument);
        }

        private void AssertParsedValueIsEmpty<T>(Argument<T> argument) where T : IEnumerable
        {
            var result = argument.Parse("");

            result.GetValueForArgument(argument)
                  .Should()
                  .BeEmpty();
        }
    }
}