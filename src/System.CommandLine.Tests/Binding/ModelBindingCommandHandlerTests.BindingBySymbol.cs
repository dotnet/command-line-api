// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.Linq;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public partial class ModelBindingCommandHandlerTests
    {
        public class BindingBySymbol
        {
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
            public void Binding_is_correct_for_overload_having_arity_(int arity)
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
                                       .Single(m => m.GetParameters().Length == arity + 1);

                var genericParameterTypes = Enumerable.Range(1, arity)
                                                      .Select(_ => typeof(int))
                                                      .ToArray();

                var createMethod = genericMethodDef.MakeGenericMethod(genericParameterTypes);

                var parameters = new List<object>();

                parameters.AddRange(command.Arguments);
                parameters.Add(handlerFunc);

                var handler = (ICommandHandler) createMethod.Invoke(null, parameters.ToArray());

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
}