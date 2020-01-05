﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class TypeConversionTests
    {
        [Fact]
        public void Custom_types_and_conversion_logic_can_be_specified()
        {
            var argument = new Argument<MyCustomType>((SymbolResult parsed, out MyCustomType value) =>
            {
                var custom = new MyCustomType();
                foreach (var a in parsed.Arguments)
                {
                    custom.Add(a);
                }

                value = custom;
                return true;
            })
            {
                Arity = ArgumentArity.ZeroOrMore,
                Name = "arg"
            };

            var parser = new Parser(
                new Command("custom")
                {
                    argument
                });

            var result = parser.Parse("custom one two three");

            var customType = result.CommandResult
                                   .GetArgumentValueOrDefault<MyCustomType>("arg");

            customType
                .Values
                .Should()
                .BeEquivalentTo("one", "two", "three");
        }

        [Fact]
        public void Option_argument_with_arity_of_one_can_be_bound_without_custom_conversion_logic_if_the_type_has_a_constructor_that_takes_a_single_string()
        {
            var option = new Option("--file")
            {
                Argument = new Argument<FileInfo>()
            };

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
        public void Command_argument_with_arity_of_one_can_be_bound_without_custom_conversion_logic_if_the_type_has_a_constructor_that_takes_a_single_string()
        {
            var option = new Command("the-command")
            {
                new Argument<FileInfo>()
            };

            var file = new FileInfo(Path.Combine(new DirectoryInfo("temp").FullName, "the-file.txt"));
            var result = option.Parse($"{file.FullName}");

            result.CommandResult
                  .GetValueOrDefault()
                  .Should()
                  .BeOfType<FileInfo>()
                  .Which
                  .Name
                  .Should()
                  .Be("the-file.txt");
        }

        [Fact]
        public void Command_argument_with_arity_of_zero_or_one_when_type_has_a_constructor_that_takes_a_single_string_returns_null_when_argument_is_not_provided()
        {
            var option = new Command("the-command")
            {
                new Argument<FileInfo>
                {
                    Arity = ArgumentArity.ZeroOrOne
                }
            };

            var result = option.Parse("");

            result.CommandResult
                  .GetValueOrDefault()
                  .Should()
                  .BeNull();
        }

        [Fact]
        public void Argument_with_arity_of_many_can_be_called_without_custom_conversion_logic_if_the_item_type_has_a_constructor_that_takes_a_single_string()
        {
            var option = new Option("--file")
            {
                Argument = new Argument<FileInfo[]>()
            };

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
            var parser = new Parser(new Option("-x")
            {
                Argument = new Argument<bool>()
            });

            var result = parser.Parse("-x");

            result.Errors.Should().BeEmpty();
            result.ValueForOption("x").Should().Be(true);
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

            var option = result.CommandResult["x"];

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

            var option = result.CommandResult["x"];

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
                .CommandResult["x"]
                .GetValueOrDefault()
                .Should()
                .Be(true);
        }

        [Fact]
        public void When_argument_cannot_be_parsed_as_the_specified_type_then_getting_value_throws()
        {
            var command = new Command("the-command")
            {
                new Option(new[] { "-o", "--one" })
                {
                    Argument = new Argument<int>((SymbolResult symbol, out int value) =>
                    {
                        if (int.TryParse(symbol.Arguments.Single(), out value))
                        {
                            return true;
                        }

                        symbol.ErrorMessage = $"'{symbol.Token.Value}' is not an integer";

                        return false;
                    }),
                    Description = ""
                }
            };

            var result = command.Parse("the-command -o not-an-int");

            Action getValue = () =>
                result.CommandResult.ValueForOption("o");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("'not-an-int' is not an integer");
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

            result.CommandResult
                  .ValueForOption("x")
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

            result.CommandResult
                  .ValueForOption("x")
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

            Action getValue = () => result.CommandResult.ValueForOption("x");

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
            var option = new Option("-x")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ZeroOrMore
                }
            };

            option.Argument.SetDefaultValue(() => "the-default");

            IReadOnlyCollection<Symbol> symbols = new[]
            {
                option
            };
            var command1 = new Command(
                "the-command",
                ""
            );

            foreach (var symbol in symbols)
            {
                command1.Add(symbol);
            }

            var command = command1;

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

            Action getValue = () => result.CommandResult.ValueForOption("x");

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
                   .CommandResult
                   .ValueForOption("x")
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

            command.Parse("the-command -x arg1").CommandResult
                   .ValueForOption("x")
                   .Should()
                   .BeEquivalentTo(new[] { "arg1" });
        }

        [Fact]
        public void The_default_value_of_a_command_with_no_arguments_is_null()
        {
            var result = new Command("x").Parse("").CommandResult;

            var valueOrDefault = result.GetValueOrDefault();

            valueOrDefault
                .Should()
                .BeNull();
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
                }
            };

            command.Argument = new Argument<string>
            {
                Arity = ArgumentArity.ZeroOrMore
            };

            var result = command.Parse(commandLine);

            result.CommandResult
                  .GetValueOrDefault()
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

            result.CommandResult
                  .ValueForOption<bool>("x")
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

            var option = result.CommandResult["x"];

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

            var option = result.CommandResult["x"];

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

            var option = result.CommandResult["x"];

            option.GetValueOrDefault<DirectoryInfo>().Should().Be(directoryInfo);
        }

        [Fact]
        public void A_default_value_with_a_custom_constructor_can_be_specified_for_a_command_argument()
        {
            var directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            var command = new Command("something")
            {
                new Argument<DirectoryInfo>(() => directoryInfo)
            };

            var result = command.Parse("something");

            result.Errors.Should().BeEmpty();

            var value = result.CommandResult.GetValueOrDefault();

            value.Should().Be(directoryInfo);
        }

        [Fact]
        public void An_option_argument_with_a_default_argument_can_be_converted_to_the_requested_type()
        {
            var command = new Command("something")
            {
                new Option("-x")
                {
                    Argument = new Argument<string>(() => "123")
                }
            };

            var result = command.Parse("something");

            var value = result.CommandResult.ValueForOption<int>("x");

            value.Should().Be(123);
        }

        [Fact]
        public void A_command_argument_with_a_default_value_can_be_converted_to_the_requested_type()
        {
            var command = new Command("something")
            {
                new Argument<string>(() => "123")
            };

            var result = command.Parse("something");

            var value = result.CommandResult.GetValueOrDefault<int>();

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

            var value = result.CommandResult.ValueForOption<int>("x");

            value.Should().Be(456);
        }

        [Fact]
        public void When_custom_converter_is_specified_and_an_argument_is_of_the_wrong_type_then_an_error_is_returned()
        {
            var command = new Command("tally")
            {
                new Argument<int>((SymbolResult symbolResult, out int value) =>
                {
                    value = default;
                    symbolResult.ErrorMessage = "Could not parse int";
                    return false;
                })
            };

            var result = command.Parse("tally one");

            result.Errors
                  .Select(e => e.Message)
                  .Should()
                  .Contain("Could not parse int");
        }

        [Fact]
        public void When_custom_conversion_fails_then_an_option_does_not_accept_further_arguments()
        {
            var command = new Command("the-command")
            {
                new Argument<string>(),
                new Option("-x")
                {
                    Argument = new Argument<string>((SymbolResult symbolResult, out string value) =>
                    {
                        value = null;
                        return false;
                    })
                }
            };

            var result = command.Parse("the-command -x nope yep");

            result.CommandResult.Arguments.Count.Should().Be(1);
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

            var value = option.Parse("-x 123").ValueForOption<int>("x");

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

            var value = option.Parse("").ValueForOption<int?>("x");

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

            var value = option.Parse("-x 123").ValueForOption<int?>("x");

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

            var value = option.Parse("-x 123.456").ValueForOption<decimal>("x");

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

            var value = option.Parse("-x 123.456").ValueForOption<double>("x");

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

            var value = option.Parse("-x 123.456").ValueForOption<float>("x");

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

            option.Parse("-x").ValueForOption<bool>("x").Should().BeTrue();
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

            option.Parse("-x false").ValueForOption<bool>("x").Should().BeFalse();
            option.Parse("-x true").ValueForOption<bool>("x").Should().BeTrue();
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

            var value = option.Parse("-x 1 -x 2 -x 3").ValueForOption<int[]>("x");

            value.Should().BeEquivalentTo(1, 2, 3);
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

            var value = option.Parse("-x 1 -x 2 -x 3").ValueForOption<List<int>>("x");

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

            var value = option.Parse("-x 1 -x 2 -x 3").ValueForOption<IEnumerable<int>>("x");

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

            var value = parseResult.ValueForOption<DayOfWeek>("x");

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

            Action getValue = () => result.ValueForOption<int>("x");

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

            Action getValue = () => result.ValueForOption<int[]>("x");

            getValue.Should()
                    .Throw<InvalidOperationException>()
                    .Which
                    .Message
                    .Should()
                    .Be("Option '-x' expects a single argument but 2 were provided.");
        }

        [Fact]
        public async Task Custom_argument_converter_is_only_called_once()
        {
            var callCount = 0;
            var handlerWasCalled = false;

            var parser = new CommandLineBuilder(
                             new RootCommand
                             {
                                 Handler = CommandHandler.Create<int>(Run)
                             })
                         .AddOption(new Option("--value")
                         {
                             Argument = new Argument<int>(TryConvertInt)
                         })
                         .UseDefaults()
                         .Build();

            await parser.InvokeAsync("--value 42");

            callCount.Should().Be(1);
            handlerWasCalled.Should().BeTrue();

            bool TryConvertInt(SymbolResult result, out int value)
            {
                callCount++;
                return int.TryParse(result.Token.Value, out value);
            }

            void Run(int value) => handlerWasCalled = true;
        }

        [Fact]
        public void Default_value_and_custom_argument_converter_can_be_used_together()
        {
            bool TryConvertArgument(SymbolResult _, out int value)
            {
                value = 789;
                return true;
            }

            var argument = new Argument<int>(
                TryConvertArgument,
                () => 123);

            var result = new RootCommand { argument }.Parse("");

            var argumentResult = result.FindResultFor(argument);

            argumentResult
                  .GetValueOrDefault<int>()
                  .Should()
                  .Be(123);
        }

        public class MyCustomType
        {
            private readonly List<string> values = new List<string>();

            public void Add(string value) => values.Add(value);

            public string[] Values => values.ToArray();
        }
    }
}