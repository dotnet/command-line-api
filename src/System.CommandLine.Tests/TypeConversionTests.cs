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
                new CommandDefinition("custom", "", symbolDefinitions: null, argumentDefinition: new ArgumentDefinitionBuilder()
                                          .ParseArgumentsAs<MyCustomType>(parsed => {
                                              var custom = new MyCustomType();
                                              foreach (var argument in parsed.Arguments)
                                              {
                                                  custom.Add(argument);
                                              }

                                              return ArgumentParseResult.Success(custom);
                                          }, ArgumentArity.Many)));

            var result = parser.Parse("custom one two three");

            var customType = result.Command().GetValueOrDefault<MyCustomType>();

            customType
                .Values
                .Should()
                .BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void ParseArgumentsAs_with_arity_of_One_can_be_called_without_custom_conversion_logic_if_the_type_has_a_constructor_thats_takes_a_single_string()
        {
            var definition = new OptionDefinition(
                "--file",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ParseArgumentsAs<FileInfo>());

            var file = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "the-file.txt"));
            var result = definition.Parse($"--file {file.FullName}");

            result.ValueForOption("--file")
                  .Should()
                  .BeOfType<FileInfo>()
                  .Which
                  .Name
                  .Should()
                  .Be("the-file.txt");
        }

        [Fact]
        public void ParseArgumentsAs_with_arity_of_Many_can_be_called_without_custom_conversion_logic_if_the_item_type_has_a_constructor_thats_takes_a_single_string()
        {
            var definition = new OptionDefinition(
                "--file",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ParseArgumentsAs<FileInfo[]>());

            var file1 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file1.txt"));
            var file2 = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "file2.txt"));
            var result = definition.Parse($"--file {file1.FullName} --file {file2.FullName}");

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
            var definition = new ArgumentDefinitionBuilder().ParseArgumentsAs<int>(s => ArgumentParseResult.Success(1));

            definition.ArgumentArity.Should().Be(ArgumentArity.One);
        }

        [Fact]
        public void ParseArgumentsAs_defaults_arity_to_One_for_string()
        {
            var definition = new ArgumentDefinitionBuilder().ParseArgumentsAs<string>(s => ArgumentParseResult.Success(1));

            definition.ArgumentArity.Should().Be(ArgumentArity.One);
        }

        [Fact]
        public void ParseArgumentsAs_infers_arity_of_IEnumerable_types_as_Many()
        {
            var definition = new ArgumentDefinitionBuilder().ParseArgumentsAs<int[]>(s => ArgumentParseResult.Success(1));

            definition.ArgumentArity.Should().Be(ArgumentArity.Many);
        }

        [Fact]
        public void When_argument_cannot_be_parsed_as_the_specified_type_then_getting_value_throws()
        {
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    new[] { "-o", "--one" },
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder()
                        .ParseArgumentsAs<int>(symbol => {
                            if (int.TryParse(symbol.Arguments.Single(), out int intValue))
                            {
                                return ArgumentParseResult.Success(intValue);
                            }

                            return ArgumentParseResult.Failure($"'{symbol.Token}' is not an integer");
                        }))
            });

            var result = definition.Parse("the-command -o not-an-int");

            Action getValue = () =>
                result.Command().ValueForOption("o");

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
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrOne())
            });

            var result = definition.Parse("the-command -x the-argument");

            result.Command()
                  .ValueForOption("x")
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void By_default_an_option_with_exactly_one_argument_parses_as_the_argument_string_value_by_default()
        {
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())
            });

            var result = definition.Parse("the-command -x the-argument");

            result.Command()
                  .ValueForOption("x")
                  .Should()
                  .Be("the-argument");
        }

        [Fact]
        public void When_exactly_one_argument_is_expected_and_none_are_provided_then_getting_value_throws()
        {
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ExactlyOne())
            });

            var result = definition.Parse("the-command -x");

            Action getValue = () => result.Command().ValueForOption("x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be(ValidationMessages.Instance.RequiredArgumentMissingForOption("-x"));
        }

        [Fact]
        public void When_zero_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_getting_value_returns_an_empty_sequence_of_strings()
        {
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore())
            });

            var result = definition.Parse("the-command -x");

            result.Command()
                  .ValueForOption("x")
                  .Should()
                  .BeAssignableTo<IReadOnlyCollection<string>>()
                  .Which
                  .Should()
                  .BeEmpty();
        }

        [Fact]
        public void When_one_or_more_arguments_of_unspecified_type_are_expected_and_none_are_provided_then_getting_value_throws()
        {
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().OneOrMore())
            });

            var result = definition.Parse("the-command -x");

            Action getValue = () => result.Command().ValueForOption("x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be(ValidationMessages.Instance.RequiredArgumentMissingForOption("-x"));
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_multiple_arguments_parses_as_a_sequence_of_strings()
        {
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore())
            });

            definition.Parse("the-command -x arg1 -x arg2")
                      .Command()
                      .ValueForOption("x")
                      .Should()
                      .BeEquivalentTo(new[] { "arg1", "arg2" });
        }

        [Fact]
        public void By_default_an_option_that_allows_multiple_arguments_and_is_passed_one_argument_parses_as_a_sequence_of_strings()
        {
            var definition = new CommandDefinition("the-command", "", new[] {
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore())
            });

            definition.Parse("the-command -x arg1")
                      .Command()
                      .ValueForOption("x")
                      .Should()
                      .BeEquivalentTo(new[] { "arg1" });
        }

        [Fact]
        public void By_default_an_option_without_arguments_parses_as_true_when_it_is_applied()
        {
            var definition = new CommandDefinitionBuilder("something")
                             .AddOption("-x", "")
                             .BuildCommandDefinition();

            var result = definition.Parse("something -x");

            result.Command()
                  .ValueForOption<bool>("x")
                  .Should()
                  .BeTrue();
        }

        [Fact]
        public void By_default_an_option_without_arguments_parses_as_false_when_it_is_not_applied()
        {
            var definition = new CommandDefinition("something", "", new[] {
                new OptionDefinition(
                    "-x",
                    "")});

            var result = definition.Parse("something");

            result.Command().ValueForOption<bool>("x").Should().BeFalse();
        }

        [Fact]
        public void An_option_with_a_default_value_parses_as_the_default_value_when_it_the_option_has_not_been_applied()
        {
            var definition = new CommandDefinition("something", "", new[] {
                new OptionDefinition(
                    "-x",
                    "",
                    argumentDefinition: new ArgumentDefinitionBuilder().WithDefaultValue(() => "123").ExactlyOne())
            });

            var result = definition.Parse("something");

            var option = result.Command()["x"];

            option.GetValueOrDefault<string>().Should().Be("123");
        }

        [Fact]
        public void When_OfType_is_used_and_an_argument_is_of_the_wrong_type_then_an_error_is_returned()
        {
            var definition = new CommandDefinition("tally", "", symbolDefinitions: null, argumentDefinition: new ArgumentDefinitionBuilder()
                                                       .ParseArgumentsAs<int>(symbol => {
                                                           if (int.TryParse(symbol.Token, out var i))
                                                           {
                                                               return ArgumentParseResult.Success(i);
                                                           }

                                                           return ArgumentParseResult.Failure("Could not parse int");
                                                       }));

            var result = definition.Parse("tally one");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Could not parse int");
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_int_without_the_parser_specifying_a_custom_converter()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrOne());

            var value = definition.Parse("-x 123").ValueForOption<int>("x");

            value.Should().Be(123);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_decimal_without_the_parser_specifying_a_custom_converter()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrOne());

            var value = definition.Parse("-x 123.456").ValueForOption<decimal>("x");

            value.Should().Be(123.456m);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_double_without_the_parser_specifying_a_custom_converter()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrOne());

            var value = definition.Parse("-x 123.456").ValueForOption<double>("x");

            value.Should().Be(123.456d);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_float_without_the_parser_specifying_a_custom_converter()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrOne());

            var value = definition.Parse("-x 123.456").ValueForOption<float>("x");

            value.Should().Be(123.456f);
        }

        [Fact]
        public void Options_with_no_arguments_specified_can_be_correctly_converted_to_bool_without_the_parser_specifying_it()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrOne());

            definition.Parse("-x").ValueForOption<bool>("x").Should().BeTrue();
        }

        [Fact]
        public void Options_with_arguments_specified_can_be_correctly_converted_to_bool_without_the_parser_specifying_a_custom_converter()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrOne());

            definition.Parse("-x false").ValueForOption<bool>("x").Should().BeFalse();
            definition.Parse("-x true").ValueForOption<bool>("x").Should().BeTrue();
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_array_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore());

            var value = definition.Parse("-x 1 -x 2 -x 3").ValueForOption<int[]>("x");

            value.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void Values_can_be_correctly_converted_to_List_of_int_without_the_parser_specifying_a_custom_converter()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrMore());

            var value = definition.Parse("-x 1 -x 2 -x 3").ValueForOption<List<int>>("x");

            value.Should().BeEquivalentTo(1, 2, 3);
        }

        [Fact]
        public void Enum_values_can_be_correctly_converted_based_on_enum_value_name_without_the_parser_specifying_a_custom_converter()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ParseArgumentsAs<DayOfWeek>());

            var value = definition.Parse("-x Monday").ValueForOption<DayOfWeek>("x");

            value.Should().Be(DayOfWeek.Monday);
        }

        [Fact]
        public void Enum_values_that_cannot_be_parsed_result_in_an_informative_error()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ParseArgumentsAs<DayOfWeek>());

            var value = definition.Parse("-x Notaday");

            value.Errors
                 .Select(e => e.Message)
                 .Should()
                 .Contain("Cannot parse argument 'Notaday' as System.DayOfWeek.");
        }

        [Fact]
        public void When_getting_values_and_specifying_a_conversion_type_that_is_not_supported_then_it_throws()
        {
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrOne());

            var result = definition.Parse("-x not-an-int");

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
            var definition = new OptionDefinition(
                "-x",
                "",
                argumentDefinition: new ArgumentDefinitionBuilder().ZeroOrOne());

            var result = definition.Parse("-x not-an-int -x 2");

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
