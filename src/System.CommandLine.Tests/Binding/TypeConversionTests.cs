// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using FluentAssertions;
using System.Linq;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class TypeConversionTests
    {
        [Fact]
        public void Option_argument_with_arity_of_one_can_be_bound_without_custom_conversion_logic_if_the_type_has_a_constructor_that_takes_a_single_string()
        {
            var option = new Option<FileInfo>("--file");

            var file = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "the-file.txt"));
            var result = option.Parse($"--file {file.FullName}");

            result.ValueForOption(option)
                  .Name
                  .Should()
                  .Be("the-file.txt");
        }

        [Fact]
        public void Command_argument_with_arity_of_one_can_be_bound_without_custom_conversion_logic_if_the_type_has_a_constructor_that_takes_a_single_string()
        {
            var argument = new Argument<FileInfo>("the-arg");

            var command = new Command("the-command")
            {
                argument
            };

            var file = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "the-file.txt"));
            var result = command.Parse($"{file.FullName}");

            result.ValueForArgument(argument)
                  .Name
                  .Should()
                  .Be("the-file.txt");
        }

        [Fact]
        public void Command_argument_with_arity_of_zero_or_one_when_type_has_a_constructor_that_takes_a_single_string_returns_null_when_argument_is_not_provided()
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

            result.ValueForArgument(argument)
                  .Should()
                  .BeNull();
        }

        [Fact]
        public void Argument_with_arity_of_many_can_be_called_without_custom_conversion_logic_if_the_item_type_has_a_constructor_that_takes_a_single_string()
        {
            var option = new Option<FileInfo[]>("--file");

            var file1 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file1.txt"));
            var file2 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file2.txt"));
            var result = option.Parse($"--file {file1.FullName} --file {file2.FullName}");

            result.ValueForOption(option)
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

        [Fact]
        public void Option_Argument_defaults_arity_to_ExactlyOne_for_nullable_types()
        {
            var option = new Option("-i")
            {
                Argument = new Argument<int?>()
            };

            option.Argument.Arity.Should().BeEquivalentTo(ArgumentArity.ExactlyOne);
        }

        [Theory]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(List<int>))]
        public void Argument_infers_arity_of_IEnumerable_types_as_OneOrMore(Type type)
        {
            var argument = new Argument { ArgumentType = type };

            argument.Arity.Should().BeEquivalentTo(ArgumentArity.OneOrMore);
        }

        [Fact]
        public void Argument_bool_will_default_to_true_when_no_argument_is_passed()
        {
            var option = new Option<bool>("-x");
            var parser = new Parser(option);

            var result = parser.Parse("-x");

            result.Errors.Should().BeEmpty();
            result.ValueForOption(option).Should().Be(true);
        }

        [Fact]
        public void Argument_parses_as_the_default_value_when_the_option_has_not_been_applied()
        {
            var command = new Command("something")
            {
                new Option("-x")
                {
                    Argument = new Argument<int>(() => 123)
                }
            };

            var result = command.Parse("something");

            var option = result.CommandResult["-x"];

            option.GetValueOrDefault().Should().Be(123);
        }

        [Fact]
        public void Argument_does_not_parse_as_the_default_value_when_the_option_has_been_applied()
        {
            var command = new Command("something")
            {
                new Option("-x")
                {
                    Argument = new Argument<int>(() => 123)
                }
            };

            var result = command.Parse("something -x 456");

            var option = result.CommandResult["-x"];

            option.GetValueOrDefault().Should().Be(456);
        }

        [Theory]
        [InlineData("the-command -x")]
        [InlineData("the-command -x true")]
        [InlineData("the-command -x:true")]
        [InlineData("the-command -x=true")]
        public void Bool_does_not_parse_as_the_default_value_when_the_option_has_been_applied(string commandLine)
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument<bool>(() => false)
                }
            };

            command
                .Parse(commandLine)
                .CommandResult["-x"]
                .GetValueOrDefault()
                .Should()
                .Be(true);
        }

        [Fact]
        public void By_default_an_option_with_zero_or_one_argument_parses_as_the_argument_string_value_by_default()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    }
                }
            };

            var result = command.Parse("the-command -x the-argument");

            result.ValueForOption("-x")
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void By_default_an_option_with_exactly_one_argument_parses_as_the_argument_string_value_by_default()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                }
            };

            var result = command.Parse("the-command -x the-argument");

            result.ValueForOption("-x")
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void When_exactly_one_argument_is_expected_and_none_are_provided_then_getting_value_throws()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                }
            };

            var result = command.Parse("the-command -x");

            Action getValue = () => result.ValueForOption("-x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Required argument missing for option: -x");
        }

        [Fact]
        public void When_zero_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_getting_value_returns_an_empty_sequence_of_strings()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }
                }
            };

            var result = command.Parse("the-command -x");

            result.ValueForOption("-x")
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
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };

            option.Argument.SetDefaultValueFactory(() => "the-default");

            var command = new Command("the-command")
            {
                option
            };

            var result = command.Parse("the-command");

            result.ValueForOption("-x")
                  .Should()
                  .BeAssignableTo<IReadOnlyCollection<string>>()
                  .Which
                  .Should()
                  .BeEquivalentTo("the-default");
        }

        [Fact]
        public void When_one_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_getting_value_throws()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.OneOrMore
                    }
                }
            };

            var result = command.Parse("the-command -x");

            Action getValue = () => result.ValueForOption("-x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Required argument missing for option: -x");
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_multiple_arguments_parses_as_a_sequence_of_strings()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }
                }
            };

            command.Parse("the-command -x arg1 -x arg2")
                   .ValueForOption("-x")
                   .Should()
                   .BeEquivalentTo(new[] { "arg1", "arg2" });
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_one_argument_parses_as_a_sequence_of_strings()
        {
            var command = new Command("the-command")
            {
                new Option("-x")
                {
                    Argument = new Argument
                    {
                        Arity = ArgumentArity.ZeroOrMore
                    }
                }
            };

            command.Parse("the-command -x arg1")
                   .ValueForOption("-x")
                   .Should()
                   .BeEquivalentTo(new[] { "arg1" });
        }

        [Theory]
        [InlineData("c -a o c c")]
        [InlineData("c c -a o c")]
        [InlineData("c c c")]
        public void When_command_argument_has_arity_greater_than_one_it_captures_arguments_before_and_after_option(string commandLine)
        {
            var command = new Command("the-command")
            {
                new Option("-a")
                {
                    Argument = new Argument<string>()
                },
                new Argument<string>("the-arg")
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };


            var result = command.Parse(commandLine);

            result.ValueForArgument("the-arg")
                  .Should()
                  .BeEquivalentTo(new[] { "c", "c", "c" });
        }

        [Fact]
        public void The_default_value_of_an_option_with_no_arguments_is_null()
        {
            var option = new Option("-x");

            var command =
                new Command("the-command")
                {
                    option
                };

            var result = command.Parse("-x");

            result.FindResultFor(option)
                  .GetValueOrDefault()
                  .Should()
                  .BeNull();
        }

        [Fact]
        public void By_default_an_option_without_arguments_parses_as_false_when_it_is_not_applied()
        {
            var command = new Command("something")
            {
                new Option("-x")
            };

            var result = command.Parse("something");

            result.ValueForOption<bool>("-x")
                  .Should()
                  .BeFalse();
        }

        [Fact]
        public void An_option_with_a_default_value_parses_as_the_default_value_when_the_option_has_not_been_applied()
        {
            var command = new Command("something")
            {
                new Option("-x")
                {
                    Argument = new Argument<string>(() => "123")
                }
            };

            var result = command.Parse("something");

            var option = result.CommandResult["-x"];

            option.GetValueOrDefault()
                  .Should()
                  .Be("123");
        }

        [Fact]
        public void A_default_value_of_a_non_string_type_can_be_specified()
        {
            var command = new Command("something")
            {
                new Option("-x")
                {
                    Argument = new Argument<int>(() => 123)
                }
            };

            var result = command.Parse("something");

            var option = result.CommandResult["-x"];

            option.GetValueOrDefault()
                  .Should()
                  .Be(123);
        }

        [Fact]
        public void A_default_value_with_a_custom_constructor_can_be_specified_for_an_option_argument()
        {
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            var command = new Command("something")
            {
                new Option("-x")
                {
                    Argument = new Argument<DirectoryInfo>(() => directoryInfo)
                }
            };

            var result = command.Parse("something");

            var option = result.CommandResult["-x"];

            option.GetValueOrDefault<DirectoryInfo>().Should().Be(directoryInfo);
        }

        [Fact]
        public void A_default_value_with_a_custom_constructor_can_be_specified_for_a_command_argument()
        {
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            var command = new Command("something")
            {
                new Argument<DirectoryInfo>("the-arg", () => directoryInfo)
            };

            var result = command.Parse("something");

            result.Errors.Should().BeEmpty();

            var value = result.CommandResult.GetArgumentValueOrDefault("the-arg");

            value.Should().Be(directoryInfo);
        }

        [Fact]
        public void An_option_argument_with_a_default_argument_can_be_converted_to_the_requested_type()
        {
            var option = new Option("-x")
            {
                Argument = new Argument<string>(() => "123")
            };

            var command = new Command("something")
            {
                option
            };

            var result = command.Parse("something");

            var value = result.ValueForOption<int>(option);

            value.Should().Be(123);
        }

        [Fact]
        public void Specifying_an_option_argument_overrides_the_default_value()
        {
            var command = new Command("something")
            {
                new Option("-x")
                {
                    Argument = new Argument<int>(() => 123)
                }
            };

            var result = command.Parse("something -x 456");

            var value = result.ValueForOption<int>("-x");

            value.Should().Be(456);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            var value = option.Parse("-x 123").ValueForOption<int>(option);

            value.Should().Be(123);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_int_with_no_value_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            var value = option.Parse("").ValueForOption<int?>("-x");

            value.Should().BeNull();
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_nullable_int_with_a_value_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            var value = option.Parse("-x 123").ValueForOption<int?>("-x");

            value.Should().Be(123);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_decimal_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            var value = option.Parse("-x 123.456").ValueForOption<decimal>("-x");

            value.Should().Be(123.456m);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_double_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            var value = option.Parse("-x 123.456").ValueForOption<double>("-x");

            value.Should().Be(123.456d);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_float_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            var value = option.Parse("-x 123.456").ValueForOption<float>("-x");

            value.Should().Be(123.456f);
        }

        [Fact]
        public void Options_with_no_arguments_specified_can_be_correctly_converted_to_bool_without_the_parser_specifying_it()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            option.Parse("-x").ValueForOption<bool>("-x").Should().BeTrue();
        }

        [Fact]
        public void Options_with_arguments_specified_can_be_correctly_converted_to_bool_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            option.Parse("-x false").ValueForOption<bool>("-x").Should().BeFalse();
            option.Parse("-x true").ValueForOption<bool>("-x").Should().BeTrue();
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_array_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };

            var value = option.Parse("-x 1 -x 2 -x 3").ValueForOption<int[]>("-x");

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
            var argument = new Argument
            {
                ArgumentType = argumentType,
                Arity = new ArgumentArity(minArity, maxArity)
            };

            var option = new Option("--items")
            {
                Argument = argument
            };

            var command = new RootCommand
            {
                option
            };

            var result = command.Parse("--items one two three");

            result.Errors.Should().BeEmpty();
            result.FindResultFor(option).GetValueOrDefault().Should().BeAssignableTo(argumentType);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_List_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };

            var value = option.Parse("-x 1 -x 2 -x 3").ValueForOption<List<int>>("-x");

            value.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_IEnumerable_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };

            var value = option.Parse("-x 1 -x 2 -x 3").ValueForOption<IEnumerable<int>>("-x");

            value.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void Enum_values_can_be_correctly_converted_based_on_enum_value_name_without_the_parser_specifying_a_custom_converter()
        {
            var option = new Option("-x")
            {
                Argument = new Argument<DayOfWeek>()
            };

            var parseResult = option.Parse("-x Monday");

            var value = parseResult.ValueForOption<DayOfWeek>("-x");

            value.Should().Be(DayOfWeek.Monday);
        }

        [Fact]
        public void Enum_values_that_cannot_be_parsed_result_in_an_informative_error()
        {
            var option = new Option("-x")
            {
                Argument = new Argument<DayOfWeek>()
            };

            var value = option.Parse("-x Notaday");

            value.Errors
                 .Select(e => e.Message)
                 .Should()
                 .Contain("Cannot parse argument 'Notaday' for option '-x' as expected type System.DayOfWeek.");
        }

        [Fact]
        public void When_getting_values_and_specifying_a_conversion_type_that_is_not_supported_then_it_throws()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            var result = option.Parse("-x not-an-int");

            Action getValue = () => result.ValueForOption<int>("-x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Cannot parse argument 'not-an-int' for option '-x' as expected type System.Int32.");
        }

        [Fact]
        public void When_getting_an_array_of_values_and_specifying_a_conversion_type_that_is_not_supported_then_it_throws()
        {
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            var result = option.Parse("-x not-an-int -x 2");

            Action getValue = () => result.ValueForOption<int[]>("-x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Option '-x' expects a single argument but 2 were provided.");
        }

        public class MyCustomType
        {
            private readonly List<string> values = new List<string>();

            public void Add(string value) => values.Add(value);

            public string[] Values => values.ToArray();
        }
    }
}