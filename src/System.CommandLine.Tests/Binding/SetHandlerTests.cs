// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using static System.Environment;

namespace System.CommandLine.Tests.Binding
{
    public class SetHandlerTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Instances_from_service_provider_can_be_injected_at_any_position_relative_to_symbol_parameters_with_Action_overloads(int @case)
        {
            var option = new Option<bool>("-o");
            var argument = new Argument<string>("value");

            var command = new RootCommand
            {
                option,
                argument
            };

            ParseResult boundParseResult = default;
            bool boundBoolValue = default;
            string boundStringValue = default;
            switch (@case)
            {
                case 1:
                    command.SetHandler((ParseResult parseResult, bool boolValue, string stringValue) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                    }, option, argument);
                    break;
                case 2:
                    command.SetHandler((bool boolValue, ParseResult parseResult, string stringValue) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                    }, option, argument);
                    break;
                case 3:
                    command.SetHandler((bool boolValue, string stringValue, ParseResult parseResult) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                    }, option, argument);
                    break;
            }

            command.Invoke("-o hi");

            boundParseResult.Should().NotBeNull();
            boundBoolValue.Should().BeTrue();
            boundStringValue.Should().Be("hi");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Instances_from_service_provider_can_be_injected_at_any_position_relative_to_symbol_parameters_with_Func_overloads(int @case)
        {
            var option = new Option<bool>("-o");
            var argument = new Argument<string>("value");

            var command = new RootCommand
            {
                option,
                argument
            };

            ParseResult boundParseResult = default;
            bool boundBoolValue = default;
            string boundStringValue = default;
            switch (@case)
            {
                case 1:
                    command.SetHandler((ParseResult parseResult, bool boolValue, string stringValue) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                        return Task.FromResult(123);
                    }, option, argument);
                    break;
                case 2:
                    command.SetHandler((bool boolValue, ParseResult parseResult, string stringValue) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                        return Task.FromResult(123);
                    }, option, argument);
                    break;
                case 3:
                    command.SetHandler((bool boolValue, string stringValue, ParseResult parseResult) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                        return Task.FromResult(123);
                    }, option, argument);
                    break;
            }

            command.Invoke("-o hi");

            boundParseResult.Should().NotBeNull();
            boundBoolValue.Should().BeTrue();
            boundStringValue.Should().Be("hi");
        }

        [Fact]
        public void If_parameter_order_does_not_match_symbol_order_then_an_error_results()
        {
            var boolOption = new Option<bool>("-o");
            var stringArg = new Argument<string>("value");

            var command = new RootCommand
            {
                boolOption,
                stringArg
            };

            var wasCalled = false;

            command.SetHandler((bool boolValue, string stringValue) => wasCalled = true, 
                                                    stringArg, boolOption);

            var exitCode = command.Invoke("-o hi");

            wasCalled.Should().BeFalse();
            exitCode.Should().Be(1);
        }

        [Fact]
        public void If_service_is_not_found_then_an_error_results()
        {
            var command = new RootCommand();

            var wasCalled = false;
            command.SetHandler((ClassWithMultipleCtor instance) => wasCalled = true);

            var exitCode = command.Invoke("");

            wasCalled.Should().BeFalse();
            exitCode.Should().Be(1);
        }

        [Fact]
        public void If_no_symbol_was_passed_for_binding_then_the_error_message_suggests_a_fix_for_the_first_missing_symbol()
        {
            var boolOption = new Option<bool>("-o");
            var stringArg = new Argument<string>("value");

            var subcommand = new Command("TheCommand")
            {
                boolOption,
                stringArg
            };

            var command = new RootCommand
            {
                subcommand
            };

            subcommand.SetHandler((bool boolValue, string stringValue) => { });

            var console = new TestConsole();

            command.Invoke("TheCommand -o hi", console);

            console.Error.ToString().Should()
                   .Contain(
                       $"No binding target was provided to the handler for command 'TheCommand' for the parameter at position 0. Did you mean to pass one of these?{NewLine}Option<Boolean> -o");
        }

        [Fact]
        public void If_no_symbol_was_passed_for_binding_subsequent_parameter_then_the_error_message_suggests_a_fix_for_the_first_missing_symbol()
        {
            var boolOption = new Option<bool>("-o");
            var stringArg = new Argument<string>("value");

            var subcommand = new Command("TheCommand")
            {
                boolOption,
                stringArg
            };

            var command = new RootCommand
            {
                subcommand
            };

            subcommand.SetHandler((bool boolValue, string stringValue) => { }, boolOption);

            var console = new TestConsole();

            command.Invoke("TheCommand -o hi", console);

            console.Error.ToString().Should()
                   .Contain(
                       $"No binding target was provided to the handler for command 'TheCommand' for the parameter at position 1. Did you mean to pass one of these?{NewLine}Argument<String> value");
        }

        [Fact]
        public void Custom_types_can_be_bound()
        {
            var boolOption = new Option<int>("-i");
            var stringArg = new Argument<string>("value");

            var command = new RootCommand
            {
                boolOption,
                stringArg
            };

            CustomType boundInstance = default;
            command.SetHandler(
                (CustomType instance) => boundInstance = instance,
                new CustomBinder(boolOption, stringArg));

            var console = new TestConsole();
            command.Invoke("-i 123 hi", console);

            boundInstance.IntValue.Should().Be(123);
            boundInstance.StringValue.Should().Be("hi");
            boundInstance.Console.Should().NotBeNull();
        }

        public class CustomType
        {
            public string StringValue { get; set; }

            public int IntValue { get; set; }

            public IConsole Console { get; set; }
        }

        public class CustomBinder : BinderBase<CustomType>
        {
            private readonly Option<int> _intOption;
            private readonly Argument<string> _stringArg;

            public CustomBinder(Option<int> intOption, Argument<string> stringArg)
            {
                _intOption = intOption;
                _stringArg = stringArg;
            }

            protected override CustomType GetBoundValue(BindingContext bindingContext)
            {
                return new CustomType
                {
                    Console = bindingContext.Console,
                    IntValue = bindingContext.ParseResult.GetValueForOption(_intOption),
                    StringValue = bindingContext.ParseResult.GetValueForArgument(_stringArg),
                };
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(16)]
        public void Binding_is_correct_for_Action_overload_having_arity_(int arity)
        {
            var command = new RootCommand();
            var commandLine = "";

            for (var i = 1; i <= arity; i++)
            {
                command.AddArgument(new Argument<int>($"i{i}"));

                commandLine += $" {i}";
            }

            var receivedValues = new List<int>();
            Delegate handlerFunc = arity switch
            {
                1 => new Action<int>(
                    i1 =>
                        Received(i1)),
                2 => new Action<int, int>(
                    (i1, i2) =>
                        Received(i1, i2)),
                3 => new Action<int, int, int>(
                    (i1, i2, i3) =>
                        Received(i1, i2, i3)),
                4 => new Action<int, int, int, int>(
                    (i1, i2, i3, i4) =>
                        Received(i1, i2, i3, i4)),
                5 => new Action<int, int, int, int, int>(
                    (i1, i2, i3, i4, i5) =>
                        Received(i1, i2, i3, i4, i5)),
                6 => new Action<int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6) =>
                        Received(i1, i2, i3, i4, i5, i6)),
                7 => new Action<int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7) =>
                        Received(i1, i2, i3, i4, i5, i6, i7)),
                8 => new Action<int, int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7, i8) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8)),
                9 => new Action<int, int, int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9)),
                10 => new Action<int, int, int, int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10)),
                11 => new Action<int, int, int, int, int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11)),
                12 => new Action<int, int, int, int, int, int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12)),
                13 => new Action<int, int, int, int, int, int, int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13)),
                14 => new Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14)),
                15 => new Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15)),
                16 => new Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15, i16) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15, i16)),

                _ => throw new ArgumentOutOfRangeException()
            };

            // build up the method invocation
            var genericMethodDef = typeof(Handler)
                                   .GetMethods()
                                   .Where(m => m.Name == nameof(Handler.SetHandler))
                                   .Where(m => m.IsGenericMethod /* symbols + handler Func */)
                                   .Where(m => m.GetParameters().ElementAt(1).ParameterType.Name.StartsWith("Action"))
                                   .Single(m => m.GetGenericArguments().Length == arity);

            var genericParameterTypes = Enumerable.Range(1, arity)
                                                  .Select(_ => typeof(int))
                                                  .ToArray();

            var setHandler = genericMethodDef.MakeGenericMethod(genericParameterTypes);

            var parameters = new List<object>
            {
                command,
                handlerFunc,
                command.Arguments.ToArray()
            };

            setHandler.Invoke(null, parameters.ToArray());

            var exitCode = command.Invoke(commandLine);

            receivedValues.Should().BeEquivalentTo(
                Enumerable.Range(1, arity),
                config => config.WithStrictOrdering());

            exitCode.Should().Be(0);

            Task Received(params int[] values)
            {
                receivedValues.AddRange(values);
                return Task.CompletedTask;
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        [InlineData(14)]
        [InlineData(15)]
        [InlineData(16)]
        public void Binding_is_correct_for_Func_overload_having_arity_(int arity)
        {
            var command = new RootCommand();
            var commandLine = "";

            for (var i = 1; i <= arity; i++)
            {
                command.AddArgument(new Argument<int>($"i{i}"));

                commandLine += $" {i}";
            }

            var receivedValues = new List<int>();
            Delegate handlerFunc = arity switch
            {
                1 => new Func<int, Task>(
                    i1 =>
                        Received(i1)),
                2 => new Func<int, int, Task>(
                    (i1, i2) =>
                        Received(i1, i2)),
                3 => new Func<int, int, int, Task>(
                    (i1, i2, i3) =>
                        Received(i1, i2, i3)),
                4 => new Func<int, int, int, int, Task>(
                    (i1, i2, i3, i4) =>
                        Received(i1, i2, i3, i4)),
                5 => new Func<int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5) =>
                        Received(i1, i2, i3, i4, i5)),
                6 => new Func<int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6) =>
                        Received(i1, i2, i3, i4, i5, i6)),
                7 => new Func<int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7) =>
                        Received(i1, i2, i3, i4, i5, i6, i7)),
                8 => new Func<int, int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7, i8) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8)),
                9 => new Func<int, int, int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9)),
                10 => new Func<int, int, int, int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10)),
                11 => new Func<int, int, int, int, int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11)),
                12 => new Func<int, int, int, int, int, int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12)),
                13 => new Func<int, int, int, int, int, int, int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13)),
                14 => new Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14)),
                15 => new Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15)),
                16 => new Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, Task>(
                    (i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15, i16) =>
                        Received(i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15, i16)),

                _ => throw new ArgumentOutOfRangeException()
            };

            // build up the method invocation
            var genericMethodDef = typeof(Handler)
                                   .GetMethods()
                                   .Where(m => m.Name == nameof(Handler.SetHandler))
                                   .Where(m => m.IsGenericMethod /* symbols + handler Func */)
                                   .Where(m => m.GetParameters().ElementAt(1).ParameterType.Name.StartsWith("Func"))
                                   .Single(m => m.GetGenericArguments().Length == arity);

            var genericParameterTypes = Enumerable.Range(1, arity)
                                                  .Select(_ => typeof(int))
                                                  .ToArray();

            var setHandler = genericMethodDef.MakeGenericMethod(genericParameterTypes);

            var parameters = new List<object>
            {
                command,
                handlerFunc,
                command.Arguments.ToArray()
            };

            setHandler.Invoke(null, parameters.ToArray());
            
            var exitCode = command.Invoke(commandLine);

            receivedValues.Should().BeEquivalentTo(
                Enumerable.Range(1, arity),
                config => config.WithStrictOrdering());

            exitCode.Should().Be(123);

            Task Received(params int[] values)
            {
                receivedValues.AddRange(values);
                return Task.FromResult(123);
            }
        }

        [Fact]
        public async Task Unexpected_return_types_result_in_exit_code_0_if_no_exception_was_thrown()
        {
            var wasCalled = false;

            var command = new Command("wat");

            var handle = (ParseResult _) =>
            {
                wasCalled = true;
                return Task.FromResult(new { NovelType = true });
            };

            command.SetHandler(handle);

            var exitCode = await command.InvokeAsync("");
            wasCalled.Should().BeTrue();
            exitCode.Should().Be(0);
        }
    }
}