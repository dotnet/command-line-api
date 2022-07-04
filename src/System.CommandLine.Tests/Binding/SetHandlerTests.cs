// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class SetHandlerTests
    {
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
                handlerFunc
            };

            parameters.AddRange(command.Arguments);

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
            };
            parameters.AddRange(command.Arguments);

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

            var handle = () =>
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