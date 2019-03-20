// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
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

            var handler = HandlerDescriptor.FromMethodInfo(handlerMethod)
                                           .GetCommandHandler();

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
        [InlineData(typeof(bool), "--value", true)]
        [InlineData(typeof(bool), "--value false", false)]
        [InlineData(typeof(string), "--value hello", "hello")]
        [InlineData(typeof(int), "--value 123", 123)]
        public async Task Option_arguments_are_bound_by_name_to_the_properties_of_method_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var complexParameterType = typeof(ClassWithSetter<>).MakeGenericType(type);

            var handlerType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(complexParameterType);

            var handlerMethod = handlerType.GetMethod("HandleAsync");

            var handler = HandlerDescriptor.FromMethodInfo(handlerMethod)
                                           .GetCommandHandler();

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

            console.Out.ToString().Should().Be($"ClassWithSetter<{type.Name}>: {expectedValue}");
        }

        [Theory]
        [InlineData(typeof(bool), "--value", true)]
        [InlineData(typeof(bool), "--value false", false)]
        [InlineData(typeof(string), "--value hello", "hello")]
        [InlineData(typeof(int), "--value 123", 123)]
        public async Task Option_arguments_are_bound_by_name_to_the_constructor_parameters_of_method_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var complexParameterType = typeof(ClassWithCtorParameter<>).MakeGenericType(type);

            var handlerType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(complexParameterType);

            var handlerMethod = handlerType.GetMethod("HandleAsync");

            var handler = HandlerDescriptor.FromMethodInfo(handlerMethod)
                                           .GetCommandHandler();

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

            console.Out.ToString().Should().Be($"ClassWithCtorParameter<{type.Name}>: {expectedValue}");
        }

        [Theory]
        [InlineData(typeof(string), "hello", "hello")]
        [InlineData(typeof(int), "123", 123)]
        public async Task Command_arguments_are_bound_by_name_to_handler_method_parameters(
            Type type,
            string commandLine,
            object expectedValue)
        {
            var targetType = typeof(ClassWithMethodHavingParameter<>).MakeGenericType(type);

            var handlerMethod = targetType.GetMethod(nameof(ClassWithMethodHavingParameter<int>.HandleAsync));

            var handler = HandlerDescriptor.FromMethodInfo(handlerMethod)
                                           .GetCommandHandler();

            var command = new Command("the-command")
                          {
                              Argument = new Argument
                                         {
                                             Name = "value",
                                             ArgumentType = type
                                         }
                          };

            var console = new TestConsole();

            await handler.InvokeAsync(
                new InvocationContext(command.Parse(commandLine), console));

            console.Out.ToString().Should().Be(expectedValue.ToString());
        }

        [Theory]
        [InlineData(typeof(string), null)]
        [InlineData(typeof(FileInfo), null)]
        [InlineData(typeof(int), 0)]
        [InlineData(typeof(int?), null)]
        public async Task Unspecified_option_arguments_with_no_default_value_are_bound_to_type_default(
            Type parameterType,
            object expectedValue)
        {
            var captureMethod = GetType()
                                .GetMethod(nameof(Capture), BindingFlags.NonPublic | BindingFlags.Instance)
                                .MakeGenericMethod(parameterType);

            var handler = CommandHandler.Create(captureMethod);

            var command = new Command(
                              "command")
                          {
                              new Option("-x",
                                         argument: new Argument
                                                   {
                                                       Name = "value",
                                                       ArgumentType = parameterType
                                                   })
                          };

            command.Handler = handler;

            var parseResult = command.Parse("");

            var invocationContext = new InvocationContext(parseResult);

            await handler.InvokeAsync(invocationContext);

            var boundValue = ((BoundValueCapturer)invocationContext.InvocationResult).BoundValue;

            boundValue.Should().Be(expectedValue);
        }

        private void Capture<T>(T value, InvocationContext invocationContext)
        {
            invocationContext.InvocationResult = new BoundValueCapturer(value);
        }

        private class BoundValueCapturer : IInvocationResult
        {
            public BoundValueCapturer(object boundValue)
            {
                BoundValue = boundValue;
            }

            public object BoundValue { get; }

            public void Apply(InvocationContext context)
            {
            }
        }

        [Theory]
        [MemberData(nameof(BindingCases))]
        public async Task Handler_method_receives_command_arguments_bound_to_the_specified_type(
            string commandLine,
            Type parameterType,
            Action<object> assert)
        {
            var captureMethod = GetType()
                                .GetMethod(nameof(Capture), BindingFlags.NonPublic | BindingFlags.Instance)
                                .MakeGenericMethod(parameterType);

            var handler = CommandHandler.Create(captureMethod);

            var command = new Command(
                "command",
                argument: new Argument
                          {
                              Name = "value",
                              ArgumentType = parameterType
                          },
                handler: handler);

            var parseResult = command.Parse(commandLine);

            var invocationContext = new InvocationContext(parseResult);

            await handler.InvokeAsync(invocationContext);

            var boundValue = ((BoundValueCapturer)invocationContext.InvocationResult).BoundValue;

            boundValue.Should().BeOfType(parameterType);

            assert(boundValue);
        }

        public static IEnumerable<object[]> BindingCases()
        {
            // yield return Case<ClassWithCtorParameter<int>>(
            //     "123",
            //     o => o.Value.Should().Be(123));
            //
            // yield return Case<ClassWithSetter<int>>(
            //     "123",
            //     o => o.Value.Should().Be(123));

            yield return Case<ClassWithCtorParameter<string>>(
                "123",
                o => o.Value.Should().Be("123"));

            yield return Case<ClassWithSetter<string>>(
                "123",
                o => o.Value.Should().Be("123"));

            var directory = Directory.GetCurrentDirectory();
            var file1 = Path.Combine(directory, "file1.txt");
            var file2 = Path.Combine(directory, "file2.txt");

            yield return Case<FileInfo>(
                file1,
                o => o.FullName.Should().Be(file1));

            yield return Case<FileInfo[]>(
                $"{file1} {file2}",
                o => o.Select(f => f.FullName)
                      .Should()
                      .BeEquivalentTo(new[] { file1, file2 }));

            yield return Case<string[]>(
                "one two",
                o => o.Should().BeEquivalentTo(new[] { "one", "two" }));

            yield return Case<List<string>>(
                "one two",
                o => o.Should().BeEquivalentTo(new List<string> { "one", "two" }));

            yield return Case<int[]>(
                "1 2",
                o => o.Should().BeEquivalentTo(new[] { 1, 2 }));

            yield return Case<List<int>>(
                "1 2",
                o => o.Should().BeEquivalentTo(new List<int> { 1, 2 }));

            object[] Case<T>(string commandLine, Action<T> assert) =>
                new object[]
                {
                    commandLine,
                    typeof(T),
                    new Action<object>(o => assert((T)o))
                };
        }
    }
}
