// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests
{
    public class MethodBinderTests
    {
        [Theory]
        [InlineData(typeof(IConsole))]
        [InlineData(typeof(InvocationContext))]
        [InlineData(typeof(ParseResult))]
        [InlineData(typeof(CancellationToken))]
        public void Options_are_not_built_for_infrastructure_types_exposed_by_method_parameters(Type type)
        {
            var targetType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(type);

            var target = Activator.CreateInstance(targetType);

            var binder = new MethodBinder(
                targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.Handle)),
                () => target);

            var options = binder.BuildOptions();

            options.Should()
                   .NotContain(o => o.Argument.ArgumentType == type);
        }

        [Theory]
        [InlineData(typeof(bool), "--value", true)]
        [InlineData(typeof(bool), "--value false", false)]
        [InlineData(typeof(string), "--value hello", "hello")]
        [InlineData(typeof(int), "--value 123", 123)]
        public async Task Option_arguments_are_bound_by_name_to_method_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var targetType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(type);

            var target = Activator.CreateInstance(targetType);

            var binder = new MethodBinder(
                targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.HandleAsync)),
                () => target);

            var command = new Command("the-command");
            command.ConfigureFrom(binder);
            var parser = new Parser(command);
            await binder.InvokeAsync(new InvocationContext(parser.Parse(commandLine), parser));

            object valueReceivedValue = ((dynamic)target).ReceivedValue;

            valueReceivedValue.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData(typeof(string), "hello", "hello")]
        [InlineData(typeof(int), "123", 123)]
        public async Task Command_arguments_are_bound_by_name_to_method_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var targetType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(type);

            var target = Activator.CreateInstance(targetType);

            var binder = new MethodBinder(
                targetType.GetMethod("HandleAsync"),
                () => target);

            var command = new Command("the-command")
                          {
                              Argument = new Argument
                                         {
                                             Name = "value",
                                             ArgumentType = type
                                         }
                          };
            var parser = new Parser(command);
            await binder.InvokeAsync(new InvocationContext(parser.Parse(commandLine), parser));

            object valueReceivedValue = ((dynamic)target).ReceivedValue;

            valueReceivedValue.Should().Be(expectedValue);
        }

        public class ClassWithMethodHavingParameter<T>
        {
            public int Handle(T value)
            {
                ReceivedValue = value;
                return 0;
            }

            public Task<int> HandleAsync(T value)
            {
               
                return Task.FromResult(Handle(value));
            }

            public T ReceivedValue { get; set; }
        }
    }
}
