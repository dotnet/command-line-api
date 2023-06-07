// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Utility;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class TypeConversionTests
    {
        protected virtual T GetValue<T>(CliOption<T> option, string commandLine)
        {
            var result = new CliRootCommand { option }.Parse(commandLine);
            return result.GetValue(option);
        }

        protected virtual T GetValue<T>(CliArgument<T> argument, string commandLine)
        {
            var result = new CliRootCommand { argument }.Parse(commandLine);
            return result.GetValue(argument);
        }

        [Fact]
        public void Option_argument_of_FileInfo_can_be_bound_without_custom_conversion_logic()
        {
            var option = new CliOption<FileInfo>("--file");

            var file = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "the-file.txt"));

            GetValue(option, $"--file {file.FullName}")
                  .Name
                  .Should()
                  .Be("the-file.txt");
        }

        [Fact]
        public void Command_argument_of_FileInfo_can_be_bound_without_custom_conversion_logic()
        {
            var argument = new CliArgument<FileInfo>("the-arg");

            var command = new CliCommand("the-command")
            {
                argument
            };

            var file = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "the-file.txt"));
            var result = command.Parse($"{file.FullName}");

            result.GetValue(argument)
                  .Name
                  .Should()
                  .Be("the-file.txt");
        }

        [Fact]
        public void Command_argument_of_FileInfo_returns_null_when_argument_is_not_provided()
        {
            var argument = new CliArgument<FileInfo>("the-arg")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            var command = new CliCommand("the-command")
            {
                argument
            };

            var result = command.Parse("");

            result.GetValue(argument)
                  .Should()
                  .BeNull();
        }

        [Fact]
        public void Argument_of_FileInfo_that_is_empty_results_in_an_informative_error()
        {
            var option = new CliOption<FileInfo>("--file");
            var result = new CliRootCommand { option }.Parse(new string[] { "--file", "" });

            result.Errors
                  .Should()
                  .ContainSingle()
                  .Which
                  .Message
                  .Should()
                  .Contain("Cannot parse argument '' for option '--file'");
        }

        [Fact]
        public void Argument_of_array_of_FileInfo_can_be_called_without_custom_conversion_logic()
        {
            var option = new CliOption<FileInfo[]>("--file");

            var file1 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file1.txt"));
            var file2 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file2.txt"));

            GetValue(option, $"--file {file1.FullName} --file {file2.FullName}")
                .Select(fi => fi.Name)
                .Should()
                .BeEquivalentTo("file1.txt", "file2.txt");
        }

        [Fact]
        public void Argument_defaults_arity_to_One_for_non_IEnumerable_types()
        {
            var argument = new CliArgument<int>("arg");

            argument.Arity.Should().BeEquivalentTo(ArgumentArity.ExactlyOne);
        }

        [Fact]
        public void Argument_defaults_arity_to_ExactlyOne_for_string()
        {
            var argument = new CliArgument<string>("arg");

            argument.Arity.Should().BeEquivalentTo(ArgumentArity.ExactlyOne);
        }

        [Fact]
        public void Command_Argument_defaults_arity_to_ZeroOrOne_for_nullable_types()
        {
            var command = new CliCommand("the-command")
            {
                new CliArgument<int?>("arg")
            };

            command.Arguments.Single().Arity.Should().BeEquivalentTo(ArgumentArity.ZeroOrOne);
        }

        [Theory]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(List<int>))]
        public void Argument_infers_arity_of_IEnumerable_types_as_OneOrMore(Type type)
        {
            var argument = ArgumentBuilder.CreateArgument(type);

            argument.Arity.Should().BeEquivalentTo(ArgumentArity.OneOrMore);
        }

        [Fact]
        public void Argument_parses_as_the_default_value_when_the_option_has_not_been_applied()
        {
            var option = new CliOption<int>("-x") { DefaultValueFactory = (_) => 123 };

            var command = new CliCommand("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValue(option).Should().Be(123);
        }

        [Fact]
        public void Option_does_not_parse_as_the_default_value_when_the_option_has_been_applied()
        {
            var option = new CliOption<int>("-x") { DefaultValueFactory = (_) => 123 };

            var command = new CliCommand("something")
            {
                option
            };

            var result = command.Parse("something -x 456");

            result.GetValue(option).Should().Be(456);
        }

        [Theory]
        [InlineData("the-command -x")]
        [InlineData("the-command -x true")]
        [InlineData("the-command -x:true")]
        [InlineData("the-command -x=true")]
        public void Bool_parses_as_true_when_the_option_has_been_applied(string commandLine)
        {
            var option = new CliOption<bool>("-x");

            var command = new CliCommand("the-command")
            {
                option
            };

            command
                .Parse(commandLine)
                .GetValue(option)
                .Should()
                .BeTrue();
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/2210
        public void Nullable_bool_with_unparseable_argument_does_not_throw()
        {
            CliRootCommand rootCommand = new();
            CliOption<bool?> option = new("--test");
            rootCommand.Options.Add(option);
            var result = rootCommand.Parse("--test ouch");

            result.Invoking(r =>  r.GetValue(option))
                  .Should().NotThrow();
        }

        [Fact] // https://github.com/dotnet/command-line-api/issues/2210
        public void Bool_with_unparseable_argument_does_not_throw()
        {
            CliRootCommand rootCommand = new();
            CliOption<bool> option = new("--test");
            rootCommand.Options.Add(option);
            var result = rootCommand.Parse("--test ouch");

            result.Invoking(r => r.GetValue(option))
                  .Should().NotThrow();
        }

        [Theory]
        [InlineData("the-command -x")]
        [InlineData("the-command -x true")]
        [InlineData("the-command -x:true")]
        [InlineData("the-command -x=true")]
        public void Nullable_bool_parses_as_true_when_the_option_has_been_applied(string commandLine)
        {
            var option = new CliOption<bool?>("-x");

            var command = new CliCommand("the-command")
            {
                option
            };

            command
                .Parse(commandLine)
                .GetValue(option)
                .Should()
                .BeTrue();
        }

        [Theory]
        [InlineData("the-command -x false")]
        [InlineData("the-command -x:false")]
        [InlineData("the-command -x=false")]
        public void Nullable_bool_parses_as_false_when_the_option_has_been_applied(string commandLine)
        {
            var option = new CliOption<bool?>("-x");

            var command = new CliCommand("the-command")
            {
                option
            };

            command
                .Parse(commandLine)
                .GetValue(option)
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Nullable_bool_parses_as_null_when_the_option_has_not_been_applied()
        {
            var option = new CliOption<bool?>("-x");

            GetValue(option, "")
                .Should()
                .Be(null);
        }

        [Fact]
        public void When_exactly_one_argument_is_expected_and_none_are_provided_then_getting_value_throws()
        {
            var option = new CliOption<string>("-x");

            var command = new CliCommand("the-command")
            {
                option
            };

            var result = command.Parse("the-command -x");

            Action getValue = () => result.GetValue(option);

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Required argument missing for option: '-x'.");
        }

        [Theory]
        [InlineData("c -a o c c")]
        [InlineData("c c -a o c")]
        [InlineData("c c c")]
        public void When_command_argument_has_arity_greater_than_one_it_captures_arguments_before_and_after_option(string commandLine)
        {
            var argument = new CliArgument<string[]>("the-arg")
            {
                Arity = ArgumentArity.ZeroOrMore
            };

            var command = new CliCommand("the-command")
            {
                new CliOption<string>("-a"),
                argument
            };

            var result = command.Parse(commandLine);

            result.GetValue(argument)
                  .Should()
                  .BeEquivalentTo(new[] { "c", "c", "c" });
        }

        [Fact]
        public void The_default_value_of_a_bool_option_with_no_arguments_is_true()
        {
            var option = new CliOption<bool>("-x");

            var command =
                new CliCommand("the-command")
                {
                    option
                };

            var result = command.Parse("-x");

            result.GetValue(option)
                  .Should()
                  .Be(true);
        }

        [Fact]
        public void By_default_a_bool_option_without_arguments_parses_as_false_when_it_is_not_applied()
        {
            var option = new CliOption<bool>("-x");

            var command = new CliCommand("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValue(option)
                  .Should()
                  .Be(false);
        }

        [Fact]
        public void An_option_with_a_default_value_parses_as_the_default_value_when_the_option_has_not_been_applied()
        {
            var option = new CliOption<string>("-x") { DefaultValueFactory = (_) => "123" };

            var command = new CliCommand("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValue(option)
                  .Should()
                  .Be("123");
        }

        [Fact]
        public void An_option_with_a_default_value_of_null_parses_as_null_when_the_option_has_not_been_applied()
        {
            var option = new CliOption<string>("-x") { DefaultValueFactory = (_) => null };

            var command = new CliCommand("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValue(option)
                  .Should()
                  .Be(null);
        }

        [Fact]
        public void A_default_value_of_a_non_string_type_can_be_specified()
        {
            var option = new CliOption<int>("-x") { DefaultValueFactory = (_) => 123 };

            var command = new CliCommand("something")
            {
                option
            };

            command.Parse("something")
                   .GetValue(option)
                   .Should()
                   .Be(123);
        }

        [Fact]
        public void A_default_value_with_a_custom_constructor_can_be_specified_for_an_option_argument()
        {
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            var option = new CliOption<DirectoryInfo>("-x") { DefaultValueFactory = (_) => directoryInfo };

            var command = new CliCommand("something")
            {
                option
            };

            var result = command.Parse("something");

            result.GetValue(option).Should().Be(directoryInfo);
        }

        [Fact]
        public void A_default_value_with_a_custom_constructor_can_be_specified_for_a_command_argument()
        {
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            var argument = new CliArgument<DirectoryInfo>("the-arg") { DefaultValueFactory = (_) => directoryInfo };

            var command = new CliCommand("something")
            {
                argument
            };

            var result = command.Parse("something");

            result.Errors.Should().BeEmpty();

            var value = result.GetValue(argument);

            value.Should().Be(directoryInfo);
        }

        [Fact]
        public void Specifying_an_option_argument_overrides_the_default_value()
        {
            var option = new CliOption<int>("-x") { DefaultValueFactory = (_) => 123 };

            var command = new CliCommand("something")
            {
                option
            };

            var result = command.Parse("something -x 456");

            var value = result.GetValue(option);

            value.Should().Be(456);
        }


        [Fact]
        public void Values_can_be_correctly_converted_to_DateTime_without_the_parser_specifying_a_custom_converter()
        {
            var option = new CliOption<DateTime>("-x");

            var dateString = "2022-02-06T01:46:03.0000000-08:00";

            GetValue(option, $"-x {dateString}").Should().Be(DateTime.Parse(dateString));
        }


        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_DateTime_without_the_parser_specifying_a_custom_converter()
        {
            var option = new CliOption<DateTime?>("-x");

            var dateString = "2022-02-06T01:46:03.0000000-08:00";

            GetValue(option, $"-x {dateString}").Should().Be(DateTime.Parse(dateString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_DateTimeOffset_without_the_parser_specifying_a_custom_converter()
        {
            var option = new CliOption<DateTimeOffset>("-x");

            var dateString = "2022-02-06T09:52:54.5275055-08:00";

            GetValue(option, $"-x {dateString}").Should().Be(DateTime.Parse(dateString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_DateTimeOffset_without_the_parser_specifying_a_custom_converter()
        {
            var option = new CliOption<DateTimeOffset?>("-x");

            var dateString = "2022-02-06T09:52:54.5275055-08:00";

            GetValue(option, $"-x {dateString}").Should().Be(DateTime.Parse(dateString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_decimal_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<decimal>("-x"), "-x 123.456").Should().Be(123.456m);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_decimal_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<decimal?>("-x"), "-x 123.456").Should().Be(123.456m);

        [Fact]
        public void Values_can_be_correctly_converted_to_double_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<double>("-x"), "-x 123.456").Should().Be(123.456d);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_double_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<double?>("-x"), "-x 123.456").Should().Be(123.456d);

        [Fact]
        public void Values_can_be_correctly_converted_to_float_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<float>("-x"), "-x 123.456").Should().Be(123.456f);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_float_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<float?>("-x"), "-x 123.456").Should().Be(123.456f);

        [Fact]
        public void Values_can_be_correctly_converted_to_Guid_without_the_parser_specifying_a_custom_converter()
        {
            const string guidString = "75517282-018F-46BB-B15F-1D8DBFE23F6E";

            GetValue(new CliOption<Guid>("-x"), $"-x {guidString}").Should().Be(Guid.Parse(guidString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_Guid_without_the_parser_specifying_a_custom_converter()
        {
            const string guidString = "75517282-018F-46BB-B15F-1D8DBFE23F6E";

            GetValue(new CliOption<Guid?>("-x"), $"-x {guidString}").Should().Be(Guid.Parse(guidString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_TimeSpan_without_the_parser_specifying_a_custom_converter()
        {
            const string timeSpanString = "30";

            GetValue(new CliOption<TimeSpan>("-x"), $"-x {timeSpanString}").Should().Be(TimeSpan.Parse(timeSpanString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_TimeSpan_without_the_parser_specifying_a_custom_converter()
        {
            const string timeSpanString = "30";

            GetValue(new CliOption<TimeSpan?>("-x"), $"-x {timeSpanString}").Should().Be(TimeSpan.Parse(timeSpanString));
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_Uri_when_custom_parser_is_provided()
        {
            CliOption<Uri> option = new ("-x")
            {
                CustomParser = (argumentResult) => Uri.TryCreate(argumentResult.Tokens.Last().Value, UriKind.RelativeOrAbsolute, out var uri) ? uri : null
            };

            GetValue(option, "-x http://example.com").Should().BeEquivalentTo(new Uri("http://example.com"));
        }

        [Fact]
        public void Options_with_arguments_specified_can_be_correctly_converted_to_bool_without_the_parser_specifying_a_custom_converter()
        {
            GetValue(new CliOption<bool>("-x"), "-x false").Should().BeFalse();
            GetValue(new CliOption<bool>("-x"), "-x true").Should().BeTrue();
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_long_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<long>("-x"), "-x 123456790").Should().Be(123456790L);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_long_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<long?>("-x"), "-x 123456790").Should().Be(123456790L);

        [Fact]
        public void Values_can_be_correctly_converted_to_short_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<short>("-s"), "-s 1234").Should().Be(1234);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_short_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<short?>("-s"), "-s 1234").Should().Be(1234);

        [Fact]
        public void Values_can_be_correctly_converted_to_ulong_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<ulong>("-x"), "-x 1234").Should().Be(1234);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_ulong_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<ulong?>("-x"), "-x 1234").Should().Be(1234);

        [Fact]
        public void Values_can_be_correctly_converted_to_ushort_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<ushort>("-s"), "-s 1234").Should().Be(1234);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_ushort_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<ushort?>("-s"), "-s 1234").Should().Be(1234);

        [Fact]
        public void Values_can_be_correctly_converted_to_sbyte_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<sbyte>("-us"), "-us 123").Should().Be(123);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_sbyte_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<sbyte?>("-us"), "-us 123").Should().Be(123);

        [Fact]
        public void Values_can_be_correctly_converted_to_ipaddress_when_custom_parser_is_provided()
        {
            CliOption<IPAddress> option = new ("-us")
            { 
                CustomParser = (argumentResult) => IPAddress.Parse(argumentResult.Tokens.Last().Value)
            };

            GetValue(option, "-us 1.2.3.4").Should().Be(IPAddress.Parse("1.2.3.4"));
        }

#if NETCOREAPP3_0_OR_GREATER
        [Fact]
        public void Values_can_be_correctly_converted_to_ipendpoint_when_custom_parser_is_provided()
        {
            CliOption<IPEndPoint> option = new("-us")
            {
                CustomParser = (argumentResult) => IPEndPoint.Parse(argumentResult.Tokens.Last().Value)
            };

            GetValue(option, "-us 1.2.3.4:56").Should().Be(IPEndPoint.Parse("1.2.3.4:56"));
        }
#endif

#if NET6_0_OR_GREATER
        [Fact]
        public void Values_can_be_correctly_converted_to_dateonly_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<DateOnly>("-us"), "-us 2022-03-02").Should().Be(DateOnly.Parse("2022-03-02"));

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_dateonly_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<DateOnly?>("-x"), "-x 2022-03-02").Should().Be(DateOnly.Parse("2022-03-02"));

        [Fact]
        public void Values_can_be_correctly_converted_to_timeonly_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<TimeOnly>("-us"), "-us 12:34:56").Should().Be(TimeOnly.Parse("12:34:56"));

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_timeonly_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<TimeOnly?>("-x"), "-x 12:34:56").Should().Be(TimeOnly.Parse("12:34:56"));
#endif

        [Fact]
        public void Values_can_be_correctly_converted_to_byte_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<byte>("-us"), "-us 123").Should().Be(123);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_byte_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<byte?>("-us"), "-us 123").Should().Be(123);

        [Fact]
        public void Values_can_be_correctly_converted_to_uint_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<uint>("-us"), "-us 1234").Should().Be(1234);

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_uint_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<uint?>("-us"), "-us 1234").Should().Be(1234);

        [Fact]
        public void Values_can_be_correctly_converted_to_array_of_int_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<int[]>("-x"), "-x 1 -x 2 -x 3").Should().BeEquivalentTo(1, 2, 3);

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
            var option = OptionBuilder.CreateOption("--items", valueType: argumentType);
            option.Arity = new ArgumentArity(minArity, maxArity);

            var command = new CliRootCommand
            {
                option
            };

            var result = command.Parse("--items one --items two --items three");

            result.Errors.Should().BeEmpty();
            result.GetResult(option).GetValueOrDefault<object>().Should().BeAssignableTo(argumentType);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_List_of_int_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<List<int>>("-x"), "-x 1 -x 2 -x 3").Should().BeEquivalentTo(1, 2, 3);

        [Fact]
        public void Values_can_be_correctly_converted_to_IEnumerable_of_int_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<IEnumerable<int>>("-x"), "-x 1 -x 2 -x 3").Should().BeEquivalentTo(1, 2, 3);

        [Fact]
        public void Enum_values_can_be_correctly_converted_based_on_enum_value_name_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<DayOfWeek>("-x"), "-x Monday").Should().Be(DayOfWeek.Monday);

        [Fact]
        public void Nullable_enum_values_can_be_correctly_converted_based_on_enum_value_name_without_the_parser_specifying_a_custom_converter()
            => GetValue(new CliOption<DayOfWeek?>("-x"), "-x Monday").Should().Be(DayOfWeek.Monday);

        [Fact]
        public void Enum_values_that_cannot_be_parsed_result_in_an_informative_error()
        {
            var option = new CliOption<DayOfWeek>("-x");

            var value = new CliRootCommand { option }.Parse("-x Notaday");

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
            var option = new CliOption<int>("-x");

            var result = new CliRootCommand { option }.Parse("-x not-an-int");

            Action getValue = () => result.GetValue(option);

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
            Action getValue = () => GetValue(new CliOption<int[]>("-x"), "-x not-an-int -x 2");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Cannot parse argument 'not-an-int' for option '-x' as expected type 'System.Int32'.");
        }

        [Fact]
        public void String_defaults_to_null_when_not_specified_only_for_not_required_arguments()
            => GetValue(
                new CliArgument<string>("arg")
                { 
                    Arity = ArgumentArity.ZeroOrMore
                }, "").Should().BeNull();

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
        public void Sequence_type_defaults_to_empty_when_not_specified(Type sequenceType)
        {
            var argument = Activator.CreateInstance(typeof(CliArgument<>).MakeGenericType(sequenceType), new object[] { "argName" });

            AssertParsedValueIsEmpty((dynamic)argument);
        }

        private void AssertParsedValueIsEmpty<T>(CliArgument<T> argument) where T : IEnumerable
            => GetValue(argument, "").Should().BeEmpty();
    }
}