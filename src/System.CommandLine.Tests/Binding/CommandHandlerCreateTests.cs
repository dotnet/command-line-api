﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class CommandHandlerCreateTests
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
                    command.Handler = CommandHandler.Create((ParseResult parseResult, bool boolValue, string stringValue) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                    }, option, argument);
                    break;
                case 2:
                    command.Handler = CommandHandler.Create((bool boolValue, ParseResult parseResult, string stringValue) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                    }, option, argument);
                    break;
                case 3:
                    command.Handler = CommandHandler.Create((bool boolValue, string stringValue, ParseResult parseResult) =>
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
                    command.Handler = CommandHandler.Create((ParseResult parseResult, bool boolValue, string stringValue) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                        return Task.FromResult(123);
                    }, option, argument);
                    break;
                case 2:
                    command.Handler = CommandHandler.Create((bool boolValue, ParseResult parseResult, string stringValue) =>
                    {
                        boundParseResult = parseResult;
                        boundBoolValue = boolValue;
                        boundStringValue = stringValue;
                        return Task.FromResult(123);
                    }, option, argument);
                    break;
                case 3:
                    command.Handler = CommandHandler.Create((bool boolValue, string stringValue, ParseResult parseResult) =>
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
            var genericMethodDef = typeof(CommandHandler)
                                   .GetMethods()
                                   .Where(m => m.Name == nameof(CommandHandler.Create))
                                   .Where(m => m.IsGenericMethod /* symbols + handler Func */)
                                   .Where(m => m.GetParameters().First().ParameterType.Name.StartsWith("Action"))
                                   .Single(m => m.GetGenericArguments().Length == arity);

            var genericParameterTypes = Enumerable.Range(1, arity)
                                                  .Select(_ => typeof(int))
                                                  .ToArray();

            var createMethod = genericMethodDef.MakeGenericMethod(genericParameterTypes);

            var parameters = new List<object>();

            parameters.Add(handlerFunc);
            parameters.Add(command.Arguments.ToArray());

            var handler = (ICommandHandler)createMethod.Invoke(null, parameters.ToArray());

            command.Handler = handler;

            command.Invoke(commandLine);

            receivedValues.Should().BeEquivalentTo(
                Enumerable.Range(1, arity),
                config => config.WithStrictOrdering());

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
            var genericMethodDef = typeof(CommandHandler)
                                   .GetMethods()
                                   .Where(m => m.Name == nameof(CommandHandler.Create))
                                   .Where(m => m.IsGenericMethod /* symbols + handler Func */)
                                   .Where(m => m.GetParameters().First().ParameterType.Name.StartsWith("Func"))
                                   .Single(m => m.GetGenericArguments().Length == arity);

            var genericParameterTypes = Enumerable.Range(1, arity)
                                                  .Select(_ => typeof(int))
                                                  .ToArray();

            var createMethod = genericMethodDef.MakeGenericMethod(genericParameterTypes);

            var parameters = new List<object>();

            parameters.Add(handlerFunc);
            parameters.Add(command.Arguments.ToArray());

            var handler = (ICommandHandler)createMethod.Invoke(null, parameters.ToArray());

            command.Handler = handler;

            command.Invoke(commandLine);

            receivedValues.Should().BeEquivalentTo(
                Enumerable.Range(1, arity),
                config => config.WithStrictOrdering());

            Task Received(params int[] values)
            {
                receivedValues.AddRange(values);
                return Task.CompletedTask;
            }
        }
    }
}