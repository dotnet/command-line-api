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
        [InlineData(typeof(ClassWithCtorParameter<int>), Skip = "wip")]
        [InlineData(typeof(ClassWithSetter<int>), Skip = "wip")]
        [InlineData(typeof(ClassWithCtorParameter<string>))]
        [InlineData(typeof(ClassWithSetter<string>), Skip = "wip")]
        [InlineData(typeof(FileInfo))]
        [InlineData(typeof(FileInfo[]))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(List<int>))]
        public async Task Handler_method_receives_option_arguments_bound_to_the_specified_type(Type type)
        {
            var testCase = _bindingCases[type];

            var captureMethod = GetType()
                                .GetMethod(nameof(Capture), BindingFlags.NonPublic | BindingFlags.Instance)
                                .MakeGenericMethod(testCase.ParameterType);

            var handler = CommandHandler.Create(captureMethod);

            var command = new Command(
                "command",
                handler: handler)
                          {
                              new Option("--value")
                              {
                                  Argument = new Argument
                                             {
                                                 ArgumentType = testCase.ParameterType
                                             }
                              }
                          };

            var parseResult = command.Parse($"--value {testCase.CommandLine}");

            var invocationContext = new InvocationContext(parseResult);

            await handler.InvokeAsync(invocationContext);

            var boundValue = ((BoundValueCapturer)invocationContext.InvocationResult).BoundValue;

            boundValue.Should().BeOfType(testCase.ParameterType);

            testCase.AssertBoundValue(boundValue);
        }

        [Theory]
        [InlineData(typeof(ClassWithCtorParameter<int>), Skip = "wip")]
        [InlineData(typeof(ClassWithSetter<int>), Skip = "wip")]
        [InlineData(typeof(ClassWithCtorParameter<string>))]
        [InlineData(typeof(ClassWithSetter<string>))]
        [InlineData(typeof(FileInfo))]
        [InlineData(typeof(FileInfo[]))]
        [InlineData(typeof(string[]))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(List<int>))]
        public async Task Handler_method_receives_command_arguments_bound_to_the_specified_type(
          Type type)
        {
            var c = _bindingCases[type];

            var captureMethod = GetType()
                                .GetMethod(nameof(Capture), BindingFlags.NonPublic | BindingFlags.Instance)
                                .MakeGenericMethod(c.ParameterType);

            var handler = CommandHandler.Create(captureMethod);

            var command = new Command(
                "command",
                argument: new Argument
                          {
                              Name = "value",
                              ArgumentType = c.ParameterType
                          },
                handler: handler);

            var parseResult = command.Parse(c.CommandLine);

            var invocationContext = new InvocationContext(parseResult);

            await handler.InvokeAsync(invocationContext);

            var boundValue = ((BoundValueCapturer)invocationContext.InvocationResult).BoundValue;

            boundValue.Should().BeOfType(c.ParameterType);

            c.AssertBoundValue(boundValue);
        }

        private readonly BindingTestSet _bindingCases = new BindingTestSet
        {
              BindingTestCase.Create<ClassWithCtorParameter<int>>(
                 "123",
                 o => o.Value.Should().Be(123)),
            
              BindingTestCase.Create<ClassWithSetter<int>>(
                 "123",
                 o => o.Value.Should().Be(123)),

            BindingTestCase.Create<ClassWithCtorParameter<string>>(
                "123",
                o => o.Value.Should().Be("123")),

            BindingTestCase.Create<ClassWithSetter<string>>(
                "123",
                o => o.Value.Should().Be("123")),

            BindingTestCase.Create<FileInfo>(
                Path.Combine(Directory.GetCurrentDirectory(), "file1.txt"),
                o => o.FullName.Should().Be(Path.Combine(Directory.GetCurrentDirectory(), "file1.txt"))),

            BindingTestCase.Create<FileInfo[]>(
                $"{Path.Combine(Directory.GetCurrentDirectory(), "file1.txt")} {Path.Combine(Directory.GetCurrentDirectory(), "file2.txt")}",
                o => o.Select(f => f.FullName)
                      .Should()
                      .BeEquivalentTo(new[] { Path.Combine(Directory.GetCurrentDirectory(), "file1.txt"), Path.Combine(Directory.GetCurrentDirectory(), "file2.txt") })),

            BindingTestCase.Create<string[]>(
                "one two",
                o => o.Should().BeEquivalentTo(new[] { "one", "two" })),

            BindingTestCase.Create<List<string>>(
                "one two",
                o => o.Should().BeEquivalentTo(new List<string> { "one", "two" })),

            BindingTestCase.Create<int[]>(
                "1 2",
                o => o.Should().BeEquivalentTo(new[] { 1, 2 })),

            BindingTestCase.Create<List<int>>(
                "1 2",
                o => o.Should().BeEquivalentTo(new List<int> { 1, 2 })),
            





        };



        [Fact]
        public async Task issue_431_bool()
        {
            bool? received = null;

            var handler = CommandHandler.Create((bool x) =>
            {
                received = x;
            });

            var root = new RootCommand(handler: handler)
            {
                new Option("-x", "Explanation"
                    //   , argument: new Argument<bool>() // <-- Both assertions pass if you uncomment this
                )
            };

            var result = root.Parse("-x").ValueForOption<bool>("-x");

            result.Should().BeTrue(); // <-- Passes

            await root.InvokeAsync("-x");

            received.Should().BeTrue(); // <-- Fails (bug)
        }

        [Fact]
        public async Task issue_431_int()
        {
            int received = 0;

            var handler = CommandHandler.Create((int x) =>
            {
                received = x;
            });

            var root = new RootCommand(handler: handler)
            {
                new Option("-x", "Explanation"
                           , argument: new Argument { Arity = new ArgumentArity(1, 1) }
                )
            };

            var parseResult = root.Parse("-x 123");

            var result = parseResult.ValueForOption<int>("-x");

            result.Should().Be(123); // <-- Passes

            await root.InvokeAsync("-x 123");

            received.Should().Be(123); // <-- Fails (bug)
        }
    }
}
