// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Tests.Binding
{
    public class ModelBindingCommandHandlerTests
    {
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

            var handlerMethod = targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.HandleAsync));

            var handlerDescriptor = HandlerDescriptor.FromMethodInfo(handlerMethod);

            var handler = handlerDescriptor.GetCommandHandler();

            var command = new Command("the-command")
                          {
                              new Option("--value")
                              {
                                  Argument = new Argument
                                             {
                                                 Name = "value",
                                                 ArgumentType = type
                                             }
                              }
                          };

            var console = new TestConsole();

            await handler.InvokeAsync(
                new InvocationContext(command.Parse(commandLine), console));

            console.Out.ToString().Should().Be(expectedValue.ToString());
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

            var handlerMethod = targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.HandleAsync));

            var handlerDescriptor = HandlerDescriptor.FromMethodInfo(handlerMethod);

            var handler = handlerDescriptor.GetCommandHandler();

            var command = new Command("the-command")
                          {
                              Argument = new Argument
                                         {
                                             Name = "value",
                                             ArgumentType = type
                                         }
                          };
            var parser = new Parser(command);

            var console = new TestConsole();

            await handler.InvokeAsync(
                new InvocationContext(parser.Parse(commandLine), console));

            console.Out.ToString().Should().Be(expectedValue.ToString());
        }
    }
}
